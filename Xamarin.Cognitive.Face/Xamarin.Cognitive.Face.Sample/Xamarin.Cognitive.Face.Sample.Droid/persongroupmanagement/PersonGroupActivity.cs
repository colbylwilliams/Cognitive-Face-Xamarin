using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person_group",
			  ParentActivity = typeof (PersonGroupListActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  WindowSoftInputMode = SoftInput.AdjustNothing,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonGroupActivity : AppCompatActivity
	{
		private bool addNewPersonGroup, personGroupExists = false;
		private String personGroupId = null;
		private String oldPersonGroupName = null;
		private PersonGridViewAdapter personGridViewAdapter = null;
		private ProgressDialog mProgressDialog = null;
		private GridView gridView = null;
		private Button add_person, done_and_save = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_person_group);

			Bundle bundle = Intent.Extras;
			if (bundle != null)
			{
				addNewPersonGroup = bundle.GetBoolean ("AddNewPersonGroup");
				oldPersonGroupName = bundle.GetString ("PersonGroupName");
				personGroupId = bundle.GetString ("PersonGroupId");
				personGroupExists = !addNewPersonGroup;
			}

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			EditText editTextPersonGroupName = (EditText) FindViewById (Resource.Id.edit_person_group_name);
			editTextPersonGroupName.Text = oldPersonGroupName;

			gridView = (GridView) FindViewById (Resource.Id.gridView_persons);
			gridView.ChoiceMode = ChoiceMode.MultipleModal;
			gridView.SetMultiChoiceModeListener (new MultiChoiceModeListener (this));

			add_person = (Button) FindViewById (Resource.Id.add_person);
			done_and_save = (Button) FindViewById (Resource.Id.done_and_save);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			gridView.ItemClick += GridView_ItemClick;
			add_person.Click += Add_Person_Click;
			done_and_save.Click += Done_And_Save_Click;

			if (personGroupExists)
			{
				personGridViewAdapter = new PersonGridViewAdapter (this);
				gridView.Adapter = personGridViewAdapter;
			}
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			gridView.ItemClick -= GridView_ItemClick;
			add_person.Click -= Add_Person_Click;
			done_and_save.Click -= Done_And_Save_Click;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);

			outState.PutBoolean ("AddNewPersonGroup", addNewPersonGroup);
			outState.PutString ("OldPersonGroupName", oldPersonGroupName);
			outState.PutString ("PersonGroupId", personGroupId);
			outState.PutBoolean ("PersonGroupExists", personGroupExists);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);

			addNewPersonGroup = savedInstanceState.GetBoolean ("AddNewPersonGroup");
			personGroupId = savedInstanceState.GetString ("PersonGroupId");
			oldPersonGroupName = savedInstanceState.GetString ("OldPersonGroupName");
			personGroupExists = savedInstanceState.GetBoolean ("PersonGroupExists");
		}

		private void GridView_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			if (!personGridViewAdapter.longPressed)
			{
				String personId = personGridViewAdapter.personIdList [e.Position];
				String personName = StorageHelper.GetPersonName (
						personId, personGroupId, this);

				Intent intent = new Intent (this, typeof (PersonActivity));
				intent.PutExtra ("AddNewPerson", false);
				intent.PutExtra ("PersonName", personName);
				intent.PutExtra ("PersonId", personId);
				intent.PutExtra ("PersonGroupId", personGroupId);

				StartActivity (intent);
			}
		}

		private void Add_Person_Click (object sender, EventArgs e)
		{
			if (!personGroupExists)
			{
				ExecuteAddPersonGroup (true, personGroupId);
			}
			else
			{
				AddPerson ();
			}
		}

		private async void ExecuteAddPersonGroup (bool mAddPerson, string mPersonGroupId)
		{
			string result = string.Empty;

			mProgressDialog.Show ();
			AddLog ("Request: Creating person group " + mPersonGroupId);

			try
			{
				mProgressDialog.SetMessage ("Syncing with server to add person group...");
				SetInfo ("Syncing with server to add person group...");
				await FaceClient.Shared.CreatePersonGroup (
					mPersonGroupId,
					Application.Context.GetString (Resource.String.user_provided_person_group_name),
					Application.Context.GetString (Resource.String.user_provided_person_group_description_data));

				result = mPersonGroupId;
			}
			catch (Java.Lang.Exception e)
			{
				result = null;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 mProgressDialog.Dismiss ();

				 if (result != null)
				 {
					 AddLog ("Response: Success. Person group " + result + " created");

					 personGroupExists = true;
					 personGridViewAdapter = new PersonGridViewAdapter (this);
					 gridView.Adapter = personGridViewAdapter;

					 SetInfo ("Success. Group " + result + " created");

					 if (mAddPerson)
					 {
						 AddPerson ();
					 }
					 else
					 {
						 DoneAndSave (false);
					 }
				 }
			 });
		}

		private void AddPerson ()
		{
			SetInfo ("");

			Intent intent = new Intent (this, typeof (PersonActivity));
			intent.PutExtra ("AddNewPerson", true);
			intent.PutExtra ("PersonName", "");
			intent.PutExtra ("PersonGroupId", personGroupId);

			StartActivity (intent);
		}

		private void Done_And_Save_Click (object sender, EventArgs e)
		{
			if (!personGroupExists)
			{
				ExecuteAddPersonGroup (false, personGroupId);
			}
			else
			{
				DoneAndSave (true);
			}
		}

		private void DoneAndSave (bool trainPersonGroup)
		{
			EditText editTextPersonGroupName = (EditText) FindViewById (Resource.Id.edit_person_group_name);
			String newPersonGroupName = editTextPersonGroupName.Text;
			if (newPersonGroupName.Equals (""))
			{
				SetInfo ("Person group name could not be empty");
				return;
			}

			StorageHelper.SetPersonGroupName (personGroupId, newPersonGroupName, this);

			if (trainPersonGroup)
			{
				ExecuteTrainPersonGroup (personGroupId);
			}
			else
			{
				Finish ();
			}
		}

		private async void ExecuteTrainPersonGroup (string mPersonGroupId)
		{
			string result = string.Empty;

			mProgressDialog.Show ();
			AddLog ("Request: Training group " + mPersonGroupId);

			try
			{
				mProgressDialog.SetMessage ("Training person group...");
				SetInfo ("Training person group...");

				await FaceClient.Shared.TrainPersonGroup (mPersonGroupId);

				result = mPersonGroupId;
			}
			catch (Java.Lang.Exception e)
			{
				result = null;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 mProgressDialog.Dismiss ();

				 if (result != null)
				 {
					 AddLog ("Response: Success. Group " + result + " training completed");
					 Finish ();
				 }
			 });
		}

		private void DeleteSelectedItems ()
		{
			List<String> newPersonIdList = new List<String> ();
			List<Boolean> newPersonChecked = new List<Boolean> ();
			List<String> personIdsToDelete = new List<String> ();
			for (int i = 0; i < personGridViewAdapter.personChecked.Count; ++i)
			{
				if (personGridViewAdapter.personChecked [i])
				{
					String personId = personGridViewAdapter.personIdList [i];
					personIdsToDelete.Add (personId);
					ExecuteDeletePerson (personGroupId, personId);
				}
				else
				{
					newPersonIdList.Add (personGridViewAdapter.personIdList [i]);
					newPersonChecked.Add (false);
				}
			}

			StorageHelper.DeletePersons (personIdsToDelete, personGroupId, this);

			personGridViewAdapter.personIdList = newPersonIdList;
			personGridViewAdapter.personChecked = newPersonChecked;
			personGridViewAdapter.NotifyDataSetChanged ();
		}

		private async void ExecuteDeletePerson (string mPersonGroupId, string mPersonId)
		{
			string result = string.Empty;

			mProgressDialog.Show ();
			AddLog ("Request: Deleting person " + mPersonId);

			try
			{
				mProgressDialog.SetMessage ("Deleting selected persons...");
				SetInfo ("Deleting selected persons...");
				UUID personId = UUID.FromString (mPersonId);
				await FaceClient.Shared.DeletePerson (mPersonGroupId, personId);

				result = mPersonId;
			}
			catch (Java.Lang.Exception e)
			{
				result = null;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			 {
				 mProgressDialog.Dismiss ();

				 if (result != null)
				 {
					 SetInfo ("Person " + result + " successfully deleted");
					 AddLog ("Response: Success. Deleting person " + result + " succeed");
				 }
			 });
		}

		private void AddLog (String _log)
		{
			LogHelper.AddIdentificationLog (_log);
		}

		private void SetInfo (String info)
		{
			TextView textView = (TextView) FindViewById (Resource.Id.info);
			textView.Text = info;
		}

		private class MultiChoiceModeListener : Java.Lang.Object, AbsListView.IMultiChoiceModeListener
		{
			private PersonGroupActivity activity;

			public MultiChoiceModeListener (PersonGroupActivity act)
			{
				this.activity = act;
			}

			public bool OnActionItemClicked (ActionMode mode, IMenuItem item)
			{
				switch (item.ItemId)
				{
					case Resource.Id.menu_delete_items:
						activity.DeleteSelectedItems ();
						return true;
					default:
						return false;
				}
			}

			public bool OnCreateActionMode (ActionMode mode, IMenu menu)
			{
				MenuInflater inflater = mode.MenuInflater;
				inflater.Inflate (Resource.Menu.menu_delete_items, menu);

				activity.personGridViewAdapter.longPressed = true;
				activity.gridView.Adapter = activity.personGridViewAdapter;

				Button addNewItem = (Button) activity.FindViewById (Resource.Id.add_person);
				addNewItem.Enabled = false;

				return true;
			}

			public void OnDestroyActionMode (ActionMode mode)
			{
				activity.personGridViewAdapter.longPressed = false;

				for (int i = 0; i < activity.personGridViewAdapter.personChecked.Count; ++i)
				{
					activity.personGridViewAdapter.personChecked [i] = false;
				}

				activity.gridView.Adapter = activity.personGridViewAdapter;

				Button addNewItem = (Button) activity.FindViewById (Resource.Id.add_person);
				addNewItem.Enabled = true;
			}

			public void OnItemCheckedStateChanged (ActionMode mode, int position, long id, bool @checked)
			{
				activity.personGridViewAdapter.personChecked [position] = @checked;
				activity.gridView.Adapter = activity.personGridViewAdapter;
			}

			public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
			{
				return false;
			}
		}

		private class PersonGridViewAdapter : BaseAdapter
		{
			public List<String> personIdList;
			public List<Boolean> personChecked;
			public bool longPressed;
			private PersonGroupActivity activity;

			public PersonGridViewAdapter (PersonGroupActivity act)
			{
				longPressed = false;
				personIdList = new List<String> ();
				personChecked = new List<Boolean> ();
				activity = act;

				ICollection<String> personIdSet = StorageHelper.GetAllPersonIds (activity.personGroupId, activity);
				foreach (String personId in personIdSet)
				{
					personIdList.Add (personId);
					personChecked.Add (false);
				}
			}

			public override int Count
			{
				get
				{
					return personIdList.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return personIdList [position];
			}

			public override long GetItemId (int position)
			{
				return position;
			}

			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					LayoutInflater layoutInflater = (LayoutInflater) Application.Context.GetSystemService (Context.LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_person, parent, false);
				}

				convertView.Id = position;

				String personId = personIdList [position];
				ICollection<String> faceIdSet = StorageHelper.GetAllFaceIds (personId, activity);

				if (faceIdSet.Count != 0)
				{
					foreach (String str in faceIdSet)
					{
						var uri = global::Android.Net.Uri.Parse (StorageHelper.GetFaceUri (str, activity));
						((ImageView) convertView.FindViewById (Resource.Id.image_person)).SetImageURI (uri);
					}
				}
				else
				{
					Drawable drawable = ContextCompat.GetDrawable (activity, Resource.Drawable.select_image);
					((ImageView) convertView.FindViewById (Resource.Id.image_person)).SetImageDrawable (drawable);
				}

				// set the text of the item
				String personName = StorageHelper.GetPersonName (personId, activity.personGroupId, activity);
				((TextView) convertView.FindViewById (Resource.Id.text_person)).Text = personName;

				// set the checked status of the item
				CheckBox checkBox = (CheckBox) convertView.FindViewById (Resource.Id.checkbox_person);

				if (longPressed)
				{
					checkBox.Visibility = ViewStates.Visible;
					checkBox.SetOnCheckedChangeListener (new SetOnCheckedChangeListener (this, position));
					checkBox.Checked = personChecked [position];
				}
				else
				{
					checkBox.Visibility = ViewStates.Invisible;
				}

				return convertView;
			}
		}

		private class SetOnCheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			private PersonGridViewAdapter adapter;
			private int position;

			public SetOnCheckedChangeListener (PersonGridViewAdapter adap, int pos)
			{
				this.adapter = adap;
				this.position = pos;
			}

			public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
			{
				adapter.personChecked [position] = isChecked;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Util;
using Xamarin.Cognitive.Face.Droid.Contract;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person",
			  ParentActivity = typeof (PersonGroupActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  WindowSoftInputMode = SoftInput.AdjustNothing,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonActivity : AppCompatActivity
	{
		private bool addNewPerson = false;
		private String personId, personGroupId, oldPersonName = null;
		private const int REQUEST_SELECT_IMAGE = 0;
		private FaceGridViewAdapter faceGridViewAdapter;
		private ProgressDialog mProgressDialog;
		private GridView gridView = null;
		private Button add_face, done_and_save = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_person);

			Bundle bundle = Intent.Extras;

			if (bundle != null)
			{
				addNewPerson = bundle.GetBoolean ("AddNewPerson");
				personGroupId = bundle.GetString ("PersonGroupId");
				oldPersonName = bundle.GetString ("PersonName");

				if (!addNewPerson)
				{
					personId = bundle.GetString ("PersonId");
				}
			}

			EditText editTextPersonName = (EditText) FindViewById (Resource.Id.edit_person_name);
			editTextPersonName.Text = oldPersonName;

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			gridView = (GridView) FindViewById (Resource.Id.gridView_faces);
			gridView.ChoiceMode = ChoiceMode.MultipleModal;
			gridView.SetMultiChoiceModeListener (new MultiChoiceModeListener (this));

			add_face = (Button) FindViewById (Resource.Id.add_face);
			done_and_save = (Button) FindViewById (Resource.Id.done_and_save);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			add_face.Click += Add_Face_Click;
			done_and_save.Click += Done_And_Save_Click;

			faceGridViewAdapter = new FaceGridViewAdapter (this);
			gridView.Adapter = faceGridViewAdapter;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			add_face.Click -= Add_Face_Click;
			done_and_save.Click -= Done_And_Save_Click;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			outState.PutBoolean ("AddNewPerson", addNewPerson);
			outState.PutString ("PersonId", personId);
			outState.PutString ("PersonGroupId", personGroupId);
			outState.PutString ("OldPersonName", oldPersonName);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
			addNewPerson = savedInstanceState.GetBoolean ("AddNewPerson");
			personId = savedInstanceState.GetString ("PersonId");
			personGroupId = savedInstanceState.GetString ("PersonGroupId");
			oldPersonName = savedInstanceState.GetString ("OldPersonName");
		}

		private void Add_Face_Click (object sender, EventArgs e)
		{
			if (personId == null)
			{
				ExecuteAddPerson (true, personGroupId);
			}
			else
			{
				AddFace ();
			}
		}

		private async void ExecuteAddPerson (bool mAddFace, string mPersonGroupId)
		{
			string result = string.Empty;

			mProgressDialog.Show ();
			AddLog ("Request: Creating Person in person group" + mPersonGroupId);

			try
			{
				mProgressDialog.SetMessage ("Syncing with server to add person...");
				SetInfo ("Syncing with server to add person...");
				CreatePersonResult person = await FaceClient.Shared.CreatePerson (mPersonGroupId,
											  Application.Context.GetString (Resource.String.user_provided_person_name),
											  Application.Context.GetString (Resource.String.user_provided_description_data));

				result = person.PersonId.ToString ();
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
					 AddLog ("Response: Success. Person " + result + " created.");
					 personId = result;
					 SetInfo ("Successfully Synchronized!");

					 if (mAddFace)
					 {
						 AddFace ();
					 }
					 else
					 {
						 DoneAndSave ();
					 }
				 }
			 });
		}

		private void AddFace ()
		{
			SetInfo ("");
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}

		private void Done_And_Save_Click (object sender, EventArgs e)
		{
			if (personId == null)
			{
				ExecuteAddPerson (false, personGroupId);
			}
			else
			{
				DoneAndSave ();
			}
		}

		private void DoneAndSave ()
		{
			TextView textWarning = (TextView) FindViewById (Resource.Id.info);
			EditText editTextPersonName = (EditText) FindViewById (Resource.Id.edit_person_name);
			String newPersonName = editTextPersonName.Text;
			if (newPersonName.Equals (""))
			{
				textWarning.Text = Application.Context.GetString (Resource.String.person_name_empty_warning_message);
				return;
			}

			StorageHelper.SetPersonName (personId, newPersonName, personGroupId, this);

			Finish ();
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode)
			{
				case (int) REQUEST_SELECT_IMAGE:
					if (resultCode == Result.Ok)
					{
						var uriImagePicked = data.Data;
						Intent intent = new Intent (this, typeof (AddFaceToPersonActivity));
						intent.PutExtra ("PersonId", personId);
						intent.PutExtra ("PersonGroupId", personGroupId);
						intent.PutExtra ("ImageUriStr", uriImagePicked.ToString ());
						StartActivity (intent);
					}
					break;
				default:
					break;
			}
		}

		private void DeleteSelectedItems ()
		{
			List<String> newFaceIdList = new List<String> ();
			List<Boolean> newFaceChecked = new List<Boolean> ();
			List<String> faceIdsToDelete = new List<String> ();
			for (int i = 0; i < faceGridViewAdapter.faceChecked.Count; ++i)
			{
				bool bchecked = faceGridViewAdapter.faceChecked [i];
				if (bchecked)
				{
					String faceId = faceGridViewAdapter.faceIdList [i];
					faceIdsToDelete.Add (faceId);
					ExecuteDeleteFace (personGroupId, personId, faceId);
				}
				else
				{
					newFaceIdList.Add (faceGridViewAdapter.faceIdList [i]);
					newFaceChecked.Add (false);
				}
			}

			StorageHelper.DeleteFaces (faceIdsToDelete, personId, this);

			faceGridViewAdapter.faceIdList = newFaceIdList;
			faceGridViewAdapter.faceChecked = newFaceChecked;
			faceGridViewAdapter.NotifyDataSetChanged ();
		}

		private async void ExecuteDeleteFace (string mPersonGroupId, string mPersonId, string mFaceId)
		{
			string result = string.Empty;

			mProgressDialog.Show ();
			AddLog ("Request: Deleting face " + mFaceId);

			try
			{
				mProgressDialog.SetMessage ("Deleting selected faces...");
				SetInfo ("Deleting selected faces...");
				UUID _personId = UUID.FromString (mPersonId);
				UUID _faceId = UUID.FromString (mFaceId);
				await FaceClient.Shared.DeletePersonFace (mPersonGroupId, _personId, _faceId);

				result = mFaceId;
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
					 SetInfo ("Face " + result + " successfully deleted");
					 AddLog ("Response: Success. Deleting face " + result + " succeed");
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
			private PersonActivity activity;

			public MultiChoiceModeListener (PersonActivity act)
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

				activity.faceGridViewAdapter.longPressed = true;

				activity.gridView.Adapter = activity.faceGridViewAdapter;

				Button addNewItem = (Button) activity.FindViewById (Resource.Id.add_face);
				addNewItem.Enabled = false;

				return true;
			}

			public void OnDestroyActionMode (ActionMode mode)
			{
				activity.faceGridViewAdapter.longPressed = false;

				for (int i = 0; i < activity.faceGridViewAdapter.faceChecked.Count; ++i)
				{
					activity.faceGridViewAdapter.faceChecked [i] = false;
				}

				activity.gridView.Adapter = activity.faceGridViewAdapter;

				Button addNewItem = (Button) activity.FindViewById (Resource.Id.add_face);
				addNewItem.Enabled = true;
			}

			public void OnItemCheckedStateChanged (ActionMode mode, int position, long id, bool @checked)
			{
				activity.faceGridViewAdapter.faceChecked [position] = @checked;
				activity.gridView.Adapter = activity.faceGridViewAdapter;
			}

			public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
			{
				return false;
			}
		}

		private class FaceGridViewAdapter : BaseAdapter
		{
			public List<String> faceIdList;
			public List<Boolean> faceChecked;
			public bool longPressed;
			private PersonActivity activity;

			public FaceGridViewAdapter (PersonActivity act)
			{
				longPressed = false;
				faceIdList = new List<String> ();
				faceChecked = new List<Boolean> ();
				activity = act;

				ICollection<String> faceIdSet = StorageHelper.GetAllFaceIds (activity.personId, activity);
				foreach (String faceId in faceIdSet)
				{
					faceIdList.Add (faceId);
					faceChecked.Add (false);
				}
			}

			public override int Count
			{
				get
				{
					return faceIdList.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return faceIdList [position];
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_checkbox, parent, false);
				}
				convertView.Id = position;

				var uri = global::Android.Net.Uri.Parse (StorageHelper.GetFaceUri (faceIdList [position], activity));
				((ImageView) convertView.FindViewById (Resource.Id.image_face)).SetImageURI (uri);

				// set the checked status of the item
				CheckBox checkBox = (CheckBox) convertView.FindViewById (Resource.Id.checkbox_face);
				if (longPressed)
				{
					checkBox.Visibility = ViewStates.Visible;
					checkBox.SetOnCheckedChangeListener (new SetOnCheckedChangeListener (this, position));
					checkBox.Checked = faceChecked [position];
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
			private FaceGridViewAdapter adapter;
			private int position;

			public SetOnCheckedChangeListener (FaceGridViewAdapter adap, int pos)
			{
				this.adapter = adap;
				this.position = pos;
			}

			public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
			{
				adapter.faceChecked [position] = isChecked;
			}
		}

	}
}
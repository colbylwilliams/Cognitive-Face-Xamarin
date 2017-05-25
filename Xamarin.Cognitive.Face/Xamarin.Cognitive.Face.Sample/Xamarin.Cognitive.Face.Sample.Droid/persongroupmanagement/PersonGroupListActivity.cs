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

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person_group_list",
			  ParentActivity = typeof (IdentificationActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonGroupListActivity : AppCompatActivity
	{
		private PersonGroupsListAdapter personGroupsListAdapter = null;
		private ProgressDialog mProgressDialog = null;
		private ListView listView = null;
		private Button add_person_group, done_and_save = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_person_group_list);

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			listView = (ListView) FindViewById (Resource.Id.list_person_groups);
			listView.ChoiceMode = ChoiceMode.MultipleModal;
			listView.SetMultiChoiceModeListener (new MultiChoiceModeListener (this));

			add_person_group = (Button) FindViewById (Resource.Id.add_person_group);
			done_and_save = (Button) FindViewById (Resource.Id.done_and_save);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			listView.ItemClick += ListView_ItemClick;
			add_person_group.Click += Add_Person_Group_Click;
			done_and_save.Click += Done_And_Save_Click;

			personGroupsListAdapter = new PersonGroupsListAdapter (this);
			listView.Adapter = personGroupsListAdapter;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			listView.ItemClick -= ListView_ItemClick;
			add_person_group.Click -= Add_Person_Group_Click;
			done_and_save.Click -= Done_And_Save_Click;
		}

		private void ListView_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			if (!personGroupsListAdapter.longPressed)
			{
				String personGroupId = personGroupsListAdapter.personGroupIdList [e.Position];
				String personGroupName = StorageHelper.GetPersonGroupName (
						personGroupId, this);

				Intent intent = new Intent (this, typeof (PersonGroupActivity));
				intent.PutExtra ("AddNewPersonGroup", false);
				intent.PutExtra ("PersonGroupName", personGroupName);
				intent.PutExtra ("PersonGroupId", personGroupId);

				StartActivity (intent);
			}
		}

		private void Add_Person_Group_Click (object sender, EventArgs e)
		{
			String personGroupId = UUID.RandomUUID ().ToString ();

			Intent intent = new Intent (this, typeof (PersonGroupActivity));
			intent.PutExtra ("AddNewPersonGroup", true);
			intent.PutExtra ("PersonGroupName", "");
			intent.PutExtra ("PersonGroupId", personGroupId);

			StartActivity (intent);
		}

		private void Done_And_Save_Click (object sender, EventArgs e)
		{
			Finish ();
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

		private void DeleteSelectedItems ()
		{
			List<String> newPersonGroupIdList = new List<String> ();
			List<Boolean> newPersonGroupChecked = new List<Boolean> ();
			List<String> personGroupIdsToDelete = new List<String> ();
			for (int i = 0; i < personGroupsListAdapter.personGroupChecked.Count; ++i)
			{
				if (personGroupsListAdapter.personGroupChecked [i])
				{
					String personGroupId = personGroupsListAdapter.personGroupIdList [i];
					personGroupIdsToDelete.Add (personGroupId);
					ExecuteDeletePersonGroup (personGroupId);
				}
				else
				{
					newPersonGroupIdList.Add (personGroupsListAdapter.personGroupIdList [i]);
					newPersonGroupChecked.Add (false);
				}
			}

			StorageHelper.DeletePersonGroups (personGroupIdsToDelete, this);

			personGroupsListAdapter.personGroupIdList = newPersonGroupIdList;
			personGroupsListAdapter.personGroupChecked = newPersonGroupChecked;
			personGroupsListAdapter.NotifyDataSetChanged ();
		}

		private async void ExecuteDeletePersonGroup (string mPersonGroupId)
		{
			string result = string.Empty;

			mProgressDialog.Show ();
			AddLog ("Request: Delete Group " + mPersonGroupId);

			try
			{
				mProgressDialog.SetMessage ("Deleting selected person groups...");
				SetInfo ("Deleting selected person groups...");
				await FaceClient.Shared.DeletePersonGroup (mPersonGroupId);
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
					 SetInfo ("Person group " + result + " successfully deleted");
					 AddLog ("Response: Success. Deleting Group " + result + " succeed");
				 }
			 });
		}

		private class MultiChoiceModeListener : Java.Lang.Object, AbsListView.IMultiChoiceModeListener
		{
			private PersonGroupListActivity activity;

			public MultiChoiceModeListener (PersonGroupListActivity act)
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

				activity.personGroupsListAdapter.longPressed = true;
				activity.listView.Adapter = activity.personGroupsListAdapter;

				Button addNewItem = (Button) activity.FindViewById (Resource.Id.add_person_group);
				addNewItem.Enabled = false;

				return true;
			}

			public void OnDestroyActionMode (ActionMode mode)
			{
				activity.personGroupsListAdapter.longPressed = false;

				for (int i = 0; i < activity.personGroupsListAdapter.personGroupChecked.Count; ++i)
				{
					activity.personGroupsListAdapter.personGroupChecked [i] = false;
				}

				activity.listView.Adapter = activity.personGroupsListAdapter;

				Button addNewItem = (Button) activity.FindViewById (Resource.Id.add_person_group);
				addNewItem.Enabled = true;
			}

			public void OnItemCheckedStateChanged (ActionMode mode, int position, long id, bool @checked)
			{
				activity.personGroupsListAdapter.personGroupChecked [position] = @checked;
				activity.listView.Adapter = activity.personGroupsListAdapter;
			}

			public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
			{
				return false;
			}
		}

		private class PersonGroupsListAdapter : BaseAdapter
		{
			public List<String> personGroupIdList;
			public List<bool> personGroupChecked;
			public bool longPressed;
			private PersonGroupListActivity activity;

			public PersonGroupsListAdapter (PersonGroupListActivity act)
			{
				longPressed = false;
				personGroupIdList = new List<String> ();
				personGroupChecked = new List<bool> ();
				activity = act;

				ICollection<String> personGroupIds = StorageHelper.GetAllPersonGroupIds (activity);

				foreach (String personGroupId in personGroupIds)
				{
					personGroupIdList.Add (personGroupId);
					personGroupChecked.Add (false);
				}
			}

			public override int Count
			{
				get
				{
					return personGroupIdList.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return personGroupIdList [position];
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_person_group_with_checkbox, parent, false);
				}
				convertView.Id = position;

				// set the text of the item
				String personGroupName = StorageHelper.GetPersonGroupName (
						personGroupIdList [position], activity);
				int personNumberInGroup = StorageHelper.GetAllPersonIds (
						personGroupIdList [position], activity).Count;
				((TextView) convertView.FindViewById (Resource.Id.text_person_group)).Text =
					String.Format ("{0} (Person count: {1})", personGroupName, personNumberInGroup);

				// set the checked status of the item
				CheckBox checkBox = (CheckBox) convertView.FindViewById (Resource.Id.checkbox_person_group);
				if (longPressed)
				{
					checkBox.Visibility = ViewStates.Visible;
					checkBox.SetOnCheckedChangeListener (new SetOnCheckedChangeListener (this, position));
					checkBox.Checked = personGroupChecked [position];
				}
				else
				{
					checkBox.Visibility = ViewStates.Invisible;
				}

				return convertView;
			}

			private class SetOnCheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
			{
				private PersonGroupsListAdapter adapter;
				private int position;

				public SetOnCheckedChangeListener (PersonGroupsListAdapter adap, int pos)
				{
					this.adapter = adap;
					this.position = pos;
				}

				public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
				{
					adapter.personGroupChecked [position] = isChecked;
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Sample.Droid.Extensions;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;
using Xamarin.Cognitive.Face.Sample.Shared.Utilities;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person_group_list",
			  ParentActivity = typeof (IdentificationActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonGroupListActivity : AppCompatActivity, AbsListView.IMultiChoiceModeListener
	{
		PersonGroupsListAdapter personGroupsListAdapter;
		ProgressDialog progressDialog;
		ListView listView;
		Button add_person_group, done_and_save;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_person_group_list);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			listView = FindViewById<ListView> (Resource.Id.list_person_groups);
			listView.ChoiceMode = ChoiceMode.MultipleModal;
			listView.SetMultiChoiceModeListener (this);

			add_person_group = FindViewById<Button> (Resource.Id.add_person_group);
			done_and_save = FindViewById<Button> (Resource.Id.done_and_save);
		}


		protected override async void OnResume ()
		{
			base.OnResume ();

			listView.ItemClick += ListView_ItemClick;
			add_person_group.Click += Add_Person_Group_Click;
			done_and_save.Click += Done_And_Save_Click;

			var groups = await FaceClient.Shared.GetPersonGroups ();
			personGroupsListAdapter = new PersonGroupsListAdapter (groups);
			listView.Adapter = personGroupsListAdapter;

			//reset current group
			FaceState.Current.CurrentGroup = null;
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			listView.ItemClick -= ListView_ItemClick;
			add_person_group.Click -= Add_Person_Group_Click;
			done_and_save.Click -= Done_And_Save_Click;
		}


		void ListView_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			if (!personGroupsListAdapter.LongPressed)
			{
				var personGroup = personGroupsListAdapter.GetGroup (e.Position);

				FaceState.Current.CurrentGroup = personGroup;

				var intent = new Intent (this, typeof (PersonGroupActivity));

				StartActivity (intent);
			}
		}


		void Add_Person_Group_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (PersonGroupActivity));

			StartActivity (intent);
		}


		void Done_And_Save_Click (object sender, EventArgs e)
		{
			Finish ();
		}


		void AddLog (string log)
		{
			LogHelper.AddIdentificationLog (log);
		}


		void SetInfo (string info)
		{
			var textView = FindViewById<TextView> (Resource.Id.info);
			textView.Text = info;
		}


		async Task DeleteSelectedItems ()
		{
			var checkedGroups = personGroupsListAdapter.GetCheckedItems ();

			foreach (var checkedGroup in checkedGroups)
			{
				await ExecuteDeletePersonGroup (checkedGroup);
			}

			var groupIds = checkedGroups.Select (g => g.Id).ToList ();

			StorageHelper.DeletePersonGroups (groupIds, this);

			personGroupsListAdapter.ResetCheckedItems ();
			personGroupsListAdapter.NotifyDataSetChanged ();
		}


		async Task ExecuteDeletePersonGroup (PersonGroup personGroup)
		{
			var result = string.Empty;

			progressDialog.Show ();
			AddLog ("Request: Delete Group " + personGroup.Id);

			try
			{
				progressDialog.SetMessage ("Deleting selected person groups...");
				SetInfo ("Deleting selected person groups...");

				await FaceClient.Shared.DeletePersonGroup (personGroup);

				SetInfo ("Person group " + personGroup.Id + " successfully deleted");
				AddLog ("Response: Success. Deleting Group " + personGroup.Id + " succeed");
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		#region AbsListView.IMultiChoiceModeListener


		public void OnItemCheckedStateChanged (ActionMode mode, int position, long id, bool @checked)
		{
			personGroupsListAdapter.SetChecked (position, @checked);
		}


		public bool OnActionItemClicked (ActionMode mode, IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.menu_delete_items:
					DeleteSelectedItems ().Forget ();
					return true;
				default:
					return false;
			}
		}


		public bool OnCreateActionMode (ActionMode mode, IMenu menu)
		{
			mode.MenuInflater.Inflate (Resource.Menu.menu_delete_items, menu);

			personGroupsListAdapter.LongPressed = true;

			var addNewItem = FindViewById<Button> (Resource.Id.add_person_group);
			addNewItem.Enabled = false;

			return true;
		}


		public void OnDestroyActionMode (ActionMode mode)
		{
			personGroupsListAdapter.LongPressed = false;
			personGroupsListAdapter.ResetCheckedItems ();

			var addNewItem = FindViewById<Button> (Resource.Id.add_person_group);
			addNewItem.Enabled = true;
		}


		public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
		{
			return false;
		}


		#endregion


		class PersonGroupsListAdapter : BaseAdapter<PersonGroup>, CompoundButton.IOnCheckedChangeListener
		{
			public bool LongPressed;

			readonly TaskQueue queue = new TaskQueue ();
			readonly List<PersonGroup> personGroups;
			List<bool> personGroupChecked;

			public PersonGroupsListAdapter (List<PersonGroup> personGroups)
			{
				this.personGroups = personGroups;

				ResetCheckedItems ();
			}


			public override PersonGroup this [int position] => personGroups [position];


			public override int Count => personGroups.Count;


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_person_group_with_checkbox, parent, false);
				}

				convertView.Id = position;

				var currentGroup = GetGroup (position);

				//int personNumberInGroup = StorageHelper.GetAllPersonIds (currentGroup.Id, context).Count;

				var nameTextView = convertView.FindViewById<TextView> (Resource.Id.text_person_group);

				if (currentGroup.PeopleLoaded)
				{
					nameTextView.Text = currentGroup.GetFormattedGroupName ();
				}
				else nameTextView.Text = currentGroup.Name;

				var checkBox = convertView.FindViewById<CheckBox> (Resource.Id.checkbox_person_group);

				if (LongPressed)
				{
					checkBox.Visibility = ViewStates.Visible;
					checkBox.Tag = position;
					checkBox.SetOnCheckedChangeListener (this);
					checkBox.Checked = personGroupChecked [position];
				}
				else
				{
					checkBox.Visibility = ViewStates.Invisible;
				}

				//load the people from the server...
				if (!currentGroup.PeopleLoaded)
				{
					queue.Enqueue (async () => await FaceClient.Shared.GetPeopleForGroup (currentGroup))
						 .ContinueWith (t =>
					{
						nameTextView.Text = currentGroup.GetFormattedGroupName ();
					}, TaskScheduler.FromCurrentSynchronizationContext ());
				}

				return convertView;
			}


			public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
			{
				var position = (int) buttonView.Tag;
				personGroupChecked [position] = isChecked;
			}


			public PersonGroup GetGroup (int position)
			{
				return personGroups [position];
			}


			public void SetChecked (int position, bool @checked)
			{
				personGroupChecked [position] = @checked;
			}


			public PersonGroup [] GetCheckedItems ()
			{
				return personGroups.Where ((grp, index) => personGroupChecked [index]).ToArray ();
			}


			public void ResetCheckedItems ()
			{
				personGroupChecked = new List<bool> (personGroups.Select (g => false));
			}
		}
	}
}
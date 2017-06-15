using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;

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
			personGroupsListAdapter = new PersonGroupsListAdapter (groups, true);
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
	}
}
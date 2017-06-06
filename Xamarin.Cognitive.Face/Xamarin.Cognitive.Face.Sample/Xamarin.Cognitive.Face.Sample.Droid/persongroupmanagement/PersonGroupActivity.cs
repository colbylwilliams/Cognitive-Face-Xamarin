using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Sample.Shared;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person_group",
			  ParentActivity = typeof (PersonGroupListActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  WindowSoftInputMode = SoftInput.AdjustNothing,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonGroupActivity : AppCompatActivity, AbsListView.IMultiChoiceModeListener
	{
		PersonGridViewAdapter personGridViewAdapter;
		ProgressDialog progressDialog;
		GridView gridView;
		Button add_person, done_and_save;

		public PersonGroup Group => FaceState.Current.CurrentGroup;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_person_group);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			gridView = FindViewById<GridView> (Resource.Id.gridView_persons);
			gridView.ChoiceMode = ChoiceMode.MultipleModal;
			gridView.SetMultiChoiceModeListener (this);

			add_person = FindViewById<Button> (Resource.Id.add_person);
			done_and_save = FindViewById<Button> (Resource.Id.done_and_save);
		}


		protected override async void OnResume ()
		{
			base.OnResume ();

			gridView.ItemClick += GridView_ItemClick;
			add_person.Click += Add_Person_Click;
			done_and_save.Click += Done_And_Save_Click;

			try
			{
				if (Group != null)
				{
					var editTextPersonGroupName = FindViewById<EditText> (Resource.Id.edit_person_group_name);
					editTextPersonGroupName.Text = Group.Name;

					if (!Group.PeopleLoaded)
					{
						await FaceClient.Shared.GetPeopleForGroup (Group);
					}

					foreach (var person in Group.People)
					{
						if (person.Faces?.Count == 0)
						{
							await FaceClient.Shared.GetFacesForPerson (person, Group);
						}
					}

					personGridViewAdapter = new PersonGridViewAdapter (Group, this);
					gridView.Adapter = personGridViewAdapter;

					await CheckTrainingStatus ();
				}
			}
			catch (Exception ex)
			{
				Log.Error ($"Error getting people/faces for group :: {ex.Message}");
			}
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			gridView.ItemClick -= GridView_ItemClick;
			add_person.Click -= Add_Person_Click;
			done_and_save.Click -= Done_And_Save_Click;
		}


		async Task CheckTrainingStatus ()
		{
			try
			{
				var status = await FaceClient.Shared.GetGroupTrainingStatus (Group.Id);

				switch (status.Status)
				{
					case TrainingStatus.TrainingStatusType.NotStarted:
					case TrainingStatus.TrainingStatusType.Failed:
						//ask them to train it!
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
			}
		}


		void GridView_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			if (!personGridViewAdapter.LongPressed)
			{
				var person = personGridViewAdapter.GetPerson (e.Position);

				FaceState.Current.CurrentPerson = person;

				var intent = new Intent (this, typeof (PersonActivity));

				StartActivity (intent);
			}
		}


		async void Add_Person_Click (object sender, EventArgs e)
		{
			if (Group == null)
			{
				await ExecuteAddPersonGroup (true);
			}
			else
			{
				AddPerson ();
			}
		}


		async Task ExecuteAddPersonGroup (bool addPerson)
		{
			progressDialog.Show ();
			AddLog ("Request: Creating person group");

			try
			{
				progressDialog.SetMessage ("Syncing with server to add person group...");
				SetInfo ("Syncing with server to add person group...");

				var editTextPersonGroupName = FindViewById<EditText> (Resource.Id.edit_person_group_name);

				FaceState.Current.CurrentGroup = await FaceClient.Shared.CreatePersonGroup (editTextPersonGroupName.Text);

				AddLog ("Response: Success. Person group " + Group.Id + " created");

				personGridViewAdapter = new PersonGridViewAdapter (Group, this);
				gridView.Adapter = personGridViewAdapter;

				SetInfo ("Success. Group " + Group.Id + " created");

				if (addPerson)
				{
					AddPerson ();
				}
				else
				{
					await DoneAndSave (false);
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		void AddPerson ()
		{
			SetInfo (string.Empty);

			var intent = new Intent (this, typeof (PersonActivity));
			intent.PutExtra ("AddNewPerson", true);
			intent.PutExtra ("PersonName", "");

			StartActivity (intent);
		}


		async void Done_And_Save_Click (object sender, EventArgs e)
		{
			if (Group == null)
			{
				await ExecuteAddPersonGroup (false);
			}
			else
			{
				await DoneAndSave (true);
			}
		}


		async Task DoneAndSave (bool saveAndTrainPersonGroup)
		{
			try
			{
				progressDialog.Show ();

				var editTextPersonGroupName = FindViewById<EditText> (Resource.Id.edit_person_group_name);
				var newPersonGroupName = editTextPersonGroupName.Text;

				if (newPersonGroupName == string.Empty)
				{
					SetInfo ("Person group name can not be empty");
					return;
				}

				StorageHelper.SetPersonGroupName (Group.Id, newPersonGroupName, this);

				if (saveAndTrainPersonGroup)
				{
					await FaceClient.Shared.UpdatePersonGroup (Group, editTextPersonGroupName.Text);

					await ExecuteTrainPersonGroup ();
				}
				else
				{
					Finish ();
				}

				FaceState.Current.CurrentGroup = null;
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("Error updating group with Id " + Group.Id);
			}
			finally
			{
				progressDialog.Dismiss ();
			}
		}


		async Task ExecuteTrainPersonGroup ()
		{
			progressDialog.Show ();
			AddLog ("Request: Training group " + Group.Id);

			try
			{
				progressDialog.SetMessage ("Training person group...");
				SetInfo ("Training person group...");

				await FaceClient.Shared.TrainPersonGroup (Group);

				AddLog ("Response: Success. Group " + Group.Id + " training completed");
				Finish ();
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("Error training group");
			}

			progressDialog.Dismiss ();
		}


		async Task DeleteSelectedItems ()
		{
			var checkedPeople = personGridViewAdapter.GetCheckedItems ();

			foreach (var person in checkedPeople)
			{
				await ExecuteDeletePerson (person);
			}

			var peopleIds = checkedPeople.Select (p => p.Id).ToList ();

			StorageHelper.DeletePersons (peopleIds, Group.Id, this);

			personGridViewAdapter.ResetCheckedItems ();
			personGridViewAdapter.NotifyDataSetChanged ();
		}


		async Task ExecuteDeletePerson (Person person)
		{
			progressDialog.Show ();
			AddLog ("Request: Deleting person " + person.Id);

			try
			{
				progressDialog.SetMessage ("Deleting selected persons...");
				SetInfo ("Deleting selected persons...");

				await FaceClient.Shared.DeletePerson (Group, person);

				SetInfo ("Person " + person.Id + " successfully deleted");
				AddLog ("Response: Success. Deleting person " + person.Id + " succeed");
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("Error deleting person with Id " + person.Id);
			}

			progressDialog.Dismiss ();
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


		#region AbsListView.IMultiChoiceModeListener


		public void OnItemCheckedStateChanged (ActionMode mode, int position, long id, bool @checked)
		{
			personGridViewAdapter.SetChecked (position, @checked);
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

			personGridViewAdapter.LongPressed = true;

			var addNewItem = FindViewById<Button> (Resource.Id.add_person);
			addNewItem.Enabled = false;

			return true;
		}


		public void OnDestroyActionMode (ActionMode mode)
		{
			personGridViewAdapter.LongPressed = false;
			personGridViewAdapter.ResetCheckedItems ();

			var addNewItem = FindViewById<Button> (Resource.Id.add_person);
			addNewItem.Enabled = true;
		}


		public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
		{
			return false;
		}


		#endregion


		class PersonGridViewAdapter : BaseAdapter<Person>, CompoundButton.IOnCheckedChangeListener
		{
			readonly Context context;
			readonly PersonGroup personGroup;
			List<bool> personChecked;
			public bool LongPressed;

			public PersonGridViewAdapter (PersonGroup personGroup, Context context)
			{
				this.personGroup = personGroup;
				this.context = context;

				ResetCheckedItems ();
			}


			public override int Count => personGroup.People?.Count ?? 0;


			public override Person this [int position] => personGroup.People? [position];


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_person, parent, false);
				}

				convertView.Id = position;

				var person = GetPerson (position);

				ICollection<string> faceIdSet = StorageHelper.GetAllFaceIds (person.Id, context);

				if (faceIdSet.Count != 0)
				{
					foreach (string id in faceIdSet)
					{
						var uri = global::Android.Net.Uri.Parse (StorageHelper.GetFaceUri (id, context));
						convertView.FindViewById<ImageView> (Resource.Id.image_person).SetImageURI (uri);
					}
				}
				else
				{
					var drawable = ContextCompat.GetDrawable (context, Resource.Drawable.select_image);
					convertView.FindViewById<ImageView> (Resource.Id.image_person).SetImageDrawable (drawable);
				}

				convertView.FindViewById<TextView> (Resource.Id.text_person).Text = person.Name;

				var checkBox = convertView.FindViewById<CheckBox> (Resource.Id.checkbox_person);

				if (LongPressed)
				{
					checkBox.Visibility = ViewStates.Visible;
					checkBox.SetOnCheckedChangeListener (this);
					checkBox.Tag = position;
					checkBox.Checked = personChecked [position];
				}
				else
				{
					checkBox.Visibility = ViewStates.Invisible;
				}

				return convertView;
			}


			public void OnCheckedChanged (CompoundButton buttonView, bool isChecked)
			{
				var position = (int) buttonView.Tag;
				personChecked [position] = isChecked;
			}


			public Person GetPerson (int position)
			{
				return personGroup.People [position];
			}


			public void SetChecked (int position, bool @checked)
			{
				personChecked [position] = @checked;
			}


			public Person [] GetCheckedItems ()
			{
				return personGroup.People.Where ((p, index) => personChecked [index]).ToArray ();
			}


			public void ResetCheckedItems ()
			{
				personChecked = new List<bool> (personGroup.People.Select (g => false));
			}
		}
	}
}
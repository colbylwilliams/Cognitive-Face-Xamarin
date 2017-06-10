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
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/person",
			  ParentActivity = typeof (PersonGroupActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  WindowSoftInputMode = SoftInput.AdjustNothing,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class PersonActivity : AppCompatActivity, AbsListView.IMultiChoiceModeListener
	{
		const int REQUEST_SELECT_IMAGE = 0;

		FaceGridViewAdapter faceGridViewAdapter;
		ProgressDialog progressDialog;
		GridView gridView;
		Button add_face, done_and_save;

		PersonGroup Group => FaceState.Current.CurrentGroup;
		Person Person => FaceState.Current.CurrentPerson;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_person);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			gridView = FindViewById<GridView> (Resource.Id.gridView_faces);
			gridView.ChoiceMode = ChoiceMode.MultipleModal;
			gridView.SetMultiChoiceModeListener (this);

			add_face = FindViewById<Button> (Resource.Id.add_face);
			done_and_save = FindViewById<Button> (Resource.Id.done_and_save);
		}


		protected override void OnResume ()
		{
			base.OnResume ();

			add_face.Click += Add_Face_Click;
			done_and_save.Click += Done_And_Save_Click;

			if (Person != null)
			{
				var editTextPersonName = FindViewById<EditText> (Resource.Id.edit_person_name);
				editTextPersonName.Text = Person.Name;

				faceGridViewAdapter = new FaceGridViewAdapter (Person, this);
				gridView.Adapter = faceGridViewAdapter;
			}
		}


		protected override void OnPause ()
		{
			base.OnPause ();

			add_face.Click -= Add_Face_Click;
			done_and_save.Click -= Done_And_Save_Click;
		}


		async void Add_Face_Click (object sender, EventArgs e)
		{
			if (Person == null)
			{
				await ExecuteAddPerson (true);
			}
			else
			{
				AddFace ();
			}
		}


		async Task ExecuteAddPerson (bool addFace)
		{
			var editTextPersonName = FindViewById<EditText> (Resource.Id.edit_person_name);

			if (editTextPersonName.Text.Equals (string.Empty))
			{
				SetInfo (Application.Context.GetString (Resource.String.person_name_empty_warning_message));
				return;
			}

			progressDialog.Show ();
			AddLog ("Request: Creating Person in person group " + Group.Id);

			try
			{
				progressDialog.SetMessage ("Syncing with server to add person...");
				SetInfo ("Syncing with server to add person...");

				FaceState.Current.CurrentPerson = await FaceClient.Shared.CreatePerson (editTextPersonName.Text, Group);

				AddLog ("Response: Success. Person " + FaceState.Current.CurrentPerson.Id + " created.");
				SetInfo ("Successfully Synchronized!");

				if (addFace)
				{
					AddFace ();
				}
				else
				{
					await DoneAndSave ();
				}
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
		}


		void AddFace ()
		{
			SetInfo (string.Empty);

			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}


		async void Done_And_Save_Click (object sender, EventArgs e)
		{
			if (Person == null)
			{
				await ExecuteAddPerson (false);
			}
			else
			{
				await DoneAndSave (true);
			}
		}


		async Task DoneAndSave (bool savePerson = false)
		{
			try
			{
				progressDialog.Show ();

				var editTextPersonName = FindViewById<EditText> (Resource.Id.edit_person_name);

				if (editTextPersonName.Text.Equals (string.Empty))
				{
					SetInfo (Application.Context.GetString (Resource.String.person_name_empty_warning_message));
					return;
				}

				if (savePerson)
				{
					await FaceClient.Shared.UpdatePerson (Person, Group, editTextPersonName.Text);
				}

				StorageHelper.SetPersonName (Person.Id, editTextPersonName.Text, Group.Id, this);

				FaceState.Current.CurrentPerson = null;

				Finish ();
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("Error updating person with Id " + Person.Id);
			}
			finally
			{
				progressDialog.Dismiss ();
			}
		}


		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode)
			{
				case REQUEST_SELECT_IMAGE:
					if (resultCode == Result.Ok)
					{
						var uriImagePicked = data.Data;

						var intent = new Intent (this, typeof (AddFaceToPersonActivity));
						intent.SetData (uriImagePicked);

						StartActivity (intent);
					}
					break;
			}
		}


		async Task DeleteSelectedItems ()
		{
			var checkedFaces = faceGridViewAdapter.GetCheckedItems ();

			foreach (var face in checkedFaces)
			{
				await ExecuteDeleteFace (face);
			}

			var faceIds = checkedFaces.Select (f => f.Id).ToList ();

			StorageHelper.DeleteFaces (faceIds, Person.Id, this);

			faceGridViewAdapter.ResetCheckedItems ();
			faceGridViewAdapter.NotifyDataSetChanged ();
		}


		async Task ExecuteDeleteFace (Shared.Face face)
		{
			progressDialog.Show ();
			AddLog ("Request: Deleting face " + face.Id);

			try
			{
				progressDialog.SetMessage ("Deleting selected faces...");
				SetInfo ("Deleting selected faces...");

				await FaceClient.Shared.DeletePersonFace (Person, Group, face);

				SetInfo ("Face " + face.Id + " successfully deleted");
				AddLog ("Response: Success. Deleting face " + face.Id + " succeed");
			}
			catch (Exception e)
			{
				AddLog (e.Message);
				SetInfo ("Error deleting face with Id " + face.Id);
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
			faceGridViewAdapter.SetChecked (position, @checked);
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

			faceGridViewAdapter.LongPressed = true;

			var addNewItem = FindViewById<Button> (Resource.Id.add_face);
			addNewItem.Enabled = false;

			return true;
		}


		public void OnDestroyActionMode (ActionMode mode)
		{
			faceGridViewAdapter.LongPressed = false;
			faceGridViewAdapter.ResetCheckedItems ();

			var addNewItem = FindViewById<Button> (Resource.Id.add_face);
			addNewItem.Enabled = true;
		}


		public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
		{
			return false;
		}


		#endregion


		class FaceGridViewAdapter : BaseAdapter<Shared.Face>, CompoundButton.IOnCheckedChangeListener
		{
			readonly Context context;
			readonly Person person;
			public List<bool> faceChecked;
			public bool LongPressed;

			public FaceGridViewAdapter (Person person, Context context)
			{
				this.person = person;
				this.context = context;

				ResetCheckedItems ();
			}


			public override int Count => person.Faces?.Count ?? 0;


			public override Shared.Face this [int position] => person.Faces? [position];


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					LayoutInflater layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_checkbox, parent, false);
				}

				convertView.Id = position;

				var face = GetFace (position);

				var uri = global::Android.Net.Uri.Parse (StorageHelper.GetFaceUri (face.Id, context));
				convertView.FindViewById<ImageView> (Resource.Id.image_face).SetImageURI (uri);

				var checkBox = convertView.FindViewById<CheckBox> (Resource.Id.checkbox_face);

				if (LongPressed)
				{
					checkBox.Visibility = ViewStates.Visible;
					checkBox.SetOnCheckedChangeListener (this);
					checkBox.Checked = faceChecked [position];
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
				faceChecked [position] = isChecked;
			}


			public Shared.Face GetFace (int position)
			{
				return this [position];
			}


			public void SetChecked (int position, bool @checked)
			{
				faceChecked [position] = @checked;
			}


			public Shared.Face [] GetCheckedItems ()
			{
				return person.Faces.Where ((f, index) => faceChecked [index]).ToArray ();
			}


			public void ResetCheckedItems ()
			{
				faceChecked = new List<bool> (person.Faces.Select (g => false));
			}
		}
	}
}
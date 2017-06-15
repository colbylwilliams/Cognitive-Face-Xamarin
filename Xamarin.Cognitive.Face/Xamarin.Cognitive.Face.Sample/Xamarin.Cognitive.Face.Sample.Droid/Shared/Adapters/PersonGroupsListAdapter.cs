using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;
using Xamarin.Cognitive.Face.Shared.Utilities;

namespace Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters
{
	public class PersonGroupsListAdapter : BaseAdapter<PersonGroup>, CompoundButton.IOnCheckedChangeListener
	{
		readonly List<PersonGroup> personGroups;
		readonly bool itemCheckEnabled;
		readonly TaskQueue queue = new TaskQueue ();

		List<bool> personGroupChecked;
		int selectedPosition = -1;

		public bool LongPressed { get; set; }

		public PersonGroupsListAdapter (List<PersonGroup> personGroups, bool itemCheckEnabled = false)
		{
			this.personGroups = personGroups;
			this.itemCheckEnabled = itemCheckEnabled;

			ResetCheckedItems ();
		}


		public override PersonGroup this [int position] => personGroups [position];


		public override int Count => personGroups.Count;


		public override long GetItemId (int position) => position;


		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			if (convertView == null)
			{
				var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (Context.LayoutInflaterService);
				var layout = itemCheckEnabled ? Resource.Layout.item_person_group_with_checkbox : Resource.Layout.item_person_group;
				convertView = layoutInflater.Inflate (layout, parent, false);
			}

			convertView.Id = position;

			var currentGroup = GetGroup (position);

			var nameTextView = convertView.FindViewById<TextView> (Resource.Id.text_person_group);

			if (currentGroup.PeopleLoaded)
			{
				nameTextView.Text = currentGroup.GetFormattedGroupName ();
			}
			else
			{
				nameTextView.Text = currentGroup.Name;

				//load the people from the server...
				queue.Enqueue (async () => await FaceClient.Shared.GetPeopleForGroup (currentGroup))
					 .ContinueWith (t =>
				{
					nameTextView.Text = currentGroup.GetFormattedGroupName ();
				}, TaskScheduler.FromCurrentSynchronizationContext ());
			}

			if (position == selectedPosition)
			{
				nameTextView.SetTextColor (global::Android.Graphics.Color.ParseColor ("#3399FF"));
			}
			else
			{
				nameTextView.SetTextColor (global::Android.Graphics.Color.Black);
			}

			if (itemCheckEnabled)
			{
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


		internal void SetSelectedPosition (int position)
		{
			selectedPosition = position;

			NotifyDataSetChanged ();
		}
	}
}
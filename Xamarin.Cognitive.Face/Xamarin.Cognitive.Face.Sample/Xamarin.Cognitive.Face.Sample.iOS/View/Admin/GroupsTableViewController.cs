using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.Sample.iOS.Extensions;
using Xamarin.Cognitive.Face.Shared;
using Xamarin.Cognitive.Face.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS
{
	public partial class GroupsTableViewController : BaseTableViewController
	{
		List<PersonGroup> Groups;

		public bool AllowDelete { get; set; }
		public bool AutoSelect { get; set; }

		public PersonGroup SelectedPersonGroup { get; private set; }

		public event EventHandler<PersonGroup> GroupSelectionChanged;

		public GroupsTableViewController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (IsInitialLoad && Groups == null)
			{
				LoadGroups ().Forget ();
			}
			else
			{
				ReloadData ();
			}
		}


		async Task LoadGroups ()
		{
			try
			{
				this.ShowHUD ("Loading groups");

				Groups = await FaceClient.Shared.GetPersonGroups ();

				ReloadData ();

				this.HideHUD ();
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Error loading groups.");
			}
		}


		void ReloadData ()
		{
			TableView.ReloadData ();

			if (IsInitialLoad)
			{
				if (AutoSelect)
				{
					if (Groups.Count == 1)
					{
						TableView.SelectRow (TableView.IndexPathsForVisibleRows [0], true, UITableViewScrollPosition.Top);
						SelectedPersonGroup = Groups [TableView.IndexPathForSelectedRow.Row];
					}
				}
			}
			else if (SelectedPersonGroup != null)
			{
				var selectedIndex = Groups.IndexOf (SelectedPersonGroup);

				if (selectedIndex > -1)
				{
					TableView.SelectRow (NSIndexPath.FromRowSection (selectedIndex, 0), true, UITableViewScrollPosition.Top);
				}
			}
		}


		public override nint NumberOfSections (UITableView tableView) => 1;


		public override nint RowsInSection (UITableView tableView, nint section) => Groups?.Count ?? 0;


		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("Cell", indexPath);

			cell.TextLabel.Text = Groups [indexPath.Row].Name;
			cell.BackgroundColor = UIColor.Clear;

			if (AutoSelect)
			{
				cell.Accessory = UITableViewCellAccessory.None;
			}

			return cell;
		}


		public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return AllowDelete;
		}


		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle == UITableViewCellEditingStyle.Delete)
			{
				tableView.BeginUpdates ();

				var group = Groups [indexPath.Row];
				Groups.Remove (group);
				DeleteGroup (group).Forget ();

				tableView.DeleteRows (new NSIndexPath [] { indexPath }, UITableViewRowAnimation.Automatic);
				tableView.EndUpdates ();
			}
		}


		async Task DeleteGroup (PersonGroup personGroup)
		{
			try
			{
				//no UI feedback here since this is done via swipe to delete

				//await this.ShowHUD ()

				await FaceClient.Shared.DeletePersonGroup (personGroup);

				//TableView.ReloadData ();
			}
			catch (Exception)
			{
				this.HideHUD ().ShowSimpleAlert ("Error deleting group.");
			}
		}


		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			SelectedPersonGroup = Groups [TableView.IndexPathForSelectedRow.Row];

			GroupSelectionChanged?.Invoke (this, SelectedPersonGroup);
		}
	}
}
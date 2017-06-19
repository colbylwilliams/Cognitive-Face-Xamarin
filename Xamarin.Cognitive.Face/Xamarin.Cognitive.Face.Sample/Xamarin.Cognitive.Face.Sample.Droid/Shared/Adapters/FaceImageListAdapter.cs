using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Xamarin.Cognitive.Face.Extensions;

namespace Xamarin.Cognitive.Face.Sample.Droid.Shared.Adapters
{
	public class FaceImageListAdapter : BaseAdapter<Model.Face>
	{
		readonly List<Bitmap> faceThumbnails = new List<Bitmap> ();
		Bitmap highlightedBitmap;

		public List<Model.Face> Faces { get; } = new List<Model.Face> ();

		public int SelectedIndex { get; private set; } = -1;

		public FaceImageListAdapter ()
		{
			//blank adapter, to be populated later
		}


		public FaceImageListAdapter (List<Model.Face> faces, Bitmap photo)
		{
			AddFaces (faces, photo);
		}


		public FaceImageListAdapter (List<Model.Face> faces, List<Bitmap> thumbnails)
		{
			AddFaces (faces, thumbnails);
		}


		protected override void Dispose (bool disposing)
		{
			highlightedBitmap?.Dispose ();
			faceThumbnails?.ForEach (b => b.Dispose ());

			base.Dispose (disposing);
		}


		public void AddFaces (List<Model.Face> faces, Bitmap photo)
		{
			if (faces != null && photo != null)
			{
				AddFaces (faces, faces.GenerateThumbnails (photo));
			}
		}


		public void AddFaces (List<Model.Face> faces, List<Bitmap> thumbnails)
		{
			if (faces != null && thumbnails != null)
			{
				System.Diagnostics.Debug.Assert (thumbnails?.Count == faces?.Count, "Must have an equal count of faces and thumbnails");

				Faces.AddRange (faces);
				faceThumbnails.AddRange (thumbnails);

				NotifyDataSetChanged ();
			}
		}


		public override int Count => Faces?.Count ?? 0;


		public override Model.Face this [int position] => Faces [position];


		public Model.Face SelectedFace => SelectedIndex > -1 ? Faces [SelectedIndex] : null;


		public Bitmap GetThumbnailForPosition (int index)
		{
			if (index > -1 && faceThumbnails.Count > index)
			{
				return faceThumbnails [index];
			}

			return null;
		}


		public Bitmap GetThumbnailForFace (Model.Face face)
		{
			var index = Faces.IndexOf (face);

			return GetThumbnailForPosition (index);
		}


		public List<Bitmap> GetThumbnailsForFaceList (List<Model.Face> faces)
		{
			var list = new List<Bitmap> ();

			foreach (var face in faces)
			{
				var index = Faces.IndexOf (face);

				if (index > -1 && faceThumbnails.Count > index)
				{
					list.Add (faceThumbnails [index]);
				}
			}

			return list;
		}


		public void SetSelectedIndex (int index)
		{
			SelectedIndex = index;

			highlightedBitmap?.Dispose ();

			NotifyDataSetChanged ();
		}


		public override long GetItemId (int position) => position;


		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			if (convertView == null)
			{
				var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (Context.LayoutInflaterService);
				convertView = layoutInflater.Inflate (Resource.Layout.item_face, parent, false);
			}

			convertView.Id = position;

			var thumbnailToShow = faceThumbnails [position];

			if (SelectedIndex == position)
			{
				highlightedBitmap = thumbnailToShow.AddHighlight ();
				thumbnailToShow = highlightedBitmap;
			}

			convertView.FindViewById<ImageView> (Resource.Id.image_face).SetImageBitmap (thumbnailToShow);

			return convertView;
		}
	}
}
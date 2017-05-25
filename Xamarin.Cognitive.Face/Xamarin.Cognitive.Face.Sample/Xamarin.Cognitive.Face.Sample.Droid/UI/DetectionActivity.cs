using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Icu.Text;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using Xamarin.Cognitive.Face.Droid;
using Xamarin.Cognitive.Face.Droid.Contract;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/detection",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class DetectionActivity : AppCompatActivity
	{
		private const int REQUEST_SELECT_IMAGE = 0;
		private Button select_image, detect, view_log = null;
		private Bitmap mBitmap = null;
		private ProgressDialog mProgressDialog = null;
		private global::Android.Net.Uri mImageUri = null;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			SetContentView (Resource.Layout.activity_detection);

			mProgressDialog = new ProgressDialog (this);
			mProgressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

			select_image = FindViewById<Button> (Resource.Id.select_image);
			detect = FindViewById<Button> (Resource.Id.detect);
			view_log = FindViewById<Button> (Resource.Id.view_log);

			SetDetectButtonEnabledStatus (false);
			LogHelper.ClearDetectionLog ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			select_image.Click += Select_Image_Click;
			detect.Click += Detect_Click;
			view_log.Click += View_Log_Click;
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			select_image.Click -= Select_Image_Click;
			detect.Click -= Detect_Click;
			view_log.Click -= View_Log_Click;
		}

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			outState.PutParcelable ("ImageUri", mImageUri);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
			mImageUri = (global::Android.Net.Uri) savedInstanceState.GetParcelable ("ImageUri");

			if (mImageUri != null)
			{
				mBitmap = ImageHelper.LoadSizeLimitedBitmapFromUri (mImageUri, this.ContentResolver);
			}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode)
			{
				case (int) REQUEST_SELECT_IMAGE:
					if (resultCode == Result.Ok)
					{
						mImageUri = data.Data;
						mBitmap = ImageHelper.LoadSizeLimitedBitmapFromUri (mImageUri, this.ContentResolver);

						if (mBitmap != null)
						{
							ImageView image = FindViewById<ImageView> (Resource.Id.image);
							image.SetImageBitmap (mBitmap);
							AddLog ("Image: " + mImageUri + " resized to " + mBitmap.Width + "x" + mBitmap.Height);
						}

						FaceListAdapter faceListAdapter = new FaceListAdapter (null, this);
						ListView list_detected_faces = FindViewById<ListView> (Resource.Id.list_detected_faces);
						list_detected_faces.Adapter = faceListAdapter;

						SetInfo ("");

						SetDetectButtonEnabledStatus (true);
					}
					break;
				default:
					break;
			}
		}

		private void Select_Image_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (SelectImageActivity));
			this.StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}

		private void Detect_Click (object sender, EventArgs e)
		{
			ExecuteDetection ();
			SetAllButtonsEnabledStatus (false);
		}

		private async void ExecuteDetection ()
		{
			Face.Droid.Contract.Face [] faces = null;
			bool mSucceed = true;

			mProgressDialog.Show ();
			AddLog ("Request: Detecting in image " + mImageUri);

			try
			{
				using (MemoryStream pre_output = new MemoryStream ())
				{
					mBitmap.Compress (Bitmap.CompressFormat.Jpeg, 100, pre_output);

					using (ByteArrayInputStream inputStream = new ByteArrayInputStream (pre_output.ToArray ()))
					{
						byte [] arr = new byte [inputStream.Available ()];
						inputStream.Read (arr);
						var output = new MemoryStream (arr);

						mProgressDialog.SetMessage ("Detecting...");
						SetInfo ("Detecting...");
						faces = await FaceClient.Shared.Detect (output, true, true, new [] {
								  FaceServiceClientFaceAttributeType.Age,
								  FaceServiceClientFaceAttributeType.Gender,
								  FaceServiceClientFaceAttributeType.Smile,
								  FaceServiceClientFaceAttributeType.Glasses,
								  FaceServiceClientFaceAttributeType.FacialHair,
								  FaceServiceClientFaceAttributeType.Emotion,
								  FaceServiceClientFaceAttributeType.HeadPose
								});
					}
				}
			}
			catch (Java.Lang.Exception e)
			{
				mSucceed = false;
				AddLog (e.Message);
			}

			RunOnUiThread (() =>
			{
				AddLog ("Response: Success. Detected " + (faces == null ? 0 : faces.Length) + " face(s) in " + mImageUri);

				// Show the result on screen when detection is done.
				ListView list_detected_faces = FindViewById<ListView> (Resource.Id.list_detected_faces);
				SetUiAfterDetection (faces, mSucceed, list_detected_faces);
			});
		}

		private void View_Log_Click (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof (DetectionLogActivity));
			this.StartActivity (intent);
		}

		private void SetUiAfterDetection (Face.Droid.Contract.Face [] result, bool succeed, ListView list_detected_faces)
		{
			mProgressDialog.Dismiss ();
			SetAllButtonsEnabledStatus (true);
			SetDetectButtonEnabledStatus (false);

			if (succeed)
			{
				string detectionResult;

				if (result != null)
				{
					detectionResult = result.Length + " face"
					   + (result.Length != 1 ? "s" : "") + " detected";

					ImageView image = FindViewById<ImageView> (Resource.Id.image);
					image.SetImageBitmap (ImageHelper.DrawFaceRectanglesOnBitmap (mBitmap, result, true));
					FaceListAdapter faceListAdapter = new FaceListAdapter (result, this);
					list_detected_faces.Adapter = faceListAdapter;
				}
				else
				{
					detectionResult = "0 face detected";
				}

				SetInfo (detectionResult);
			}

			mImageUri = null;
			mBitmap = null;
		}

		private void SetDetectButtonEnabledStatus (bool isEnabled)
		{
			detect.Enabled = isEnabled;
		}

		private void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			select_image.Enabled = isEnabled;
			detect.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}

		private void SetInfo (string inf)
		{
			TextView info = FindViewById<TextView> (Resource.Id.info);
			info.Text = inf;
		}

		private void AddLog (string _log)
		{
			LogHelper.AddDetectionLog (_log);
		}

		private class FaceListAdapter : BaseAdapter
		{
			private List<Face.Droid.Contract.Face> faces;
			private List<Bitmap> faceThumbnails;
			private DetectionActivity activity;

			public FaceListAdapter (Face.Droid.Contract.Face [] detectionResult, DetectionActivity act)
			{
				faces = new List<Face.Droid.Contract.Face> ();
				faceThumbnails = new List<Bitmap> ();
				activity = act;

				if (detectionResult != null)
				{
					faces = detectionResult.ToList ();

					foreach (Face.Droid.Contract.Face face in faces)
					{
						try
						{
							faceThumbnails.Add (ImageHelper.GenerateFaceThumbnail (activity.mBitmap, face.FaceRectangle));
						}
						catch (Java.IO.IOException ex)
						{
							activity.SetInfo (ex.Message);
						}
					}
				}
			}

			public override bool IsEnabled (int position)
			{
				return false;
			}

			public override int Count
			{
				get
				{
					return faces.Count;
				}
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return faces [position];
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
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_description, parent, false);
				}
				convertView.Id = position;

				((ImageView) convertView.FindViewById (Resource.Id.face_thumbnail)).SetImageBitmap (faceThumbnails [position]);

				DecimalFormat formatter = new DecimalFormat ("#0.0");
				string face_description = string.Format ("Age: {0}\nGender: {1}\nSmile: {2}\nGlasses: {3}\nFacialHair: {4}\nHeadPose: {5}",
						faces [position].FaceAttributes.Age,
						faces [position].FaceAttributes.Gender,
						faces [position].FaceAttributes.Smile,
						faces [position].FaceAttributes.Glasses,
						GetFacialHair (faces [position].FaceAttributes.FacialHair),
						GetEmotion (faces [position].FaceAttributes.Emotion),
						GetHeadPose (faces [position].FaceAttributes.HeadPose)
						);

				TextView text_detected_face = convertView.FindViewById<TextView> (Resource.Id.text_detected_face);
				text_detected_face.Text = face_description;

				return convertView;
			}

			private string GetFacialHair (FacialHair facialHair)
			{
				return (facialHair.Moustache + facialHair.Beard + facialHair.Sideburns > 0) ? "Yes" : "No";
			}

			private string GetEmotion (Emotion emotion)
			{
				string emotionType = "";
				double emotionValue = 0.0;
				if (emotion.Anger > emotionValue)
				{
					emotionValue = emotion.Anger;
					emotionType = "Anger";
				}
				if (emotion.Contempt > emotionValue)
				{
					emotionValue = emotion.Contempt;
					emotionType = "Contempt";
				}
				if (emotion.Disgust > emotionValue)
				{
					emotionValue = emotion.Disgust;
					emotionType = "Disgust";
				}
				if (emotion.Fear > emotionValue)
				{
					emotionValue = emotion.Fear;
					emotionType = "Fear";
				}
				if (emotion.Happiness > emotionValue)
				{
					emotionValue = emotion.Happiness;
					emotionType = "Happiness";
				}
				if (emotion.Neutral > emotionValue)
				{
					emotionValue = emotion.Neutral;
					emotionType = "Neutral";
				}
				if (emotion.Sadness > emotionValue)
				{
					emotionValue = emotion.Sadness;
					emotionType = "Sadness";
				}
				if (emotion.Surprise > emotionValue)
				{
					emotionValue = emotion.Surprise;
					emotionType = "Surprise";
				}
				return string.Format ("{0}: {1}", emotionType, emotionValue);
			}

			private string GetHeadPose (HeadPose headPose)
			{
				return string.Format ("Pitch: {0}, Roll: {1}, Yaw: {2}", headPose.Pitch, headPose.Roll, headPose.Yaw);
			}
		}
	}
}
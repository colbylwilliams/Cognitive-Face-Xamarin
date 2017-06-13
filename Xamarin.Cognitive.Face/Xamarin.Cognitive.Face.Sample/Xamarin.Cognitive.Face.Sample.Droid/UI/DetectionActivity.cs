using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Droid.Extensions;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Sample.Droid
{
	[Activity (Label = "@string/detection",
			  ParentActivity = typeof (MainActivity),
			  LaunchMode = LaunchMode.SingleTop,
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class DetectionActivity : AppCompatActivity
	{
		const int REQUEST_SELECT_IMAGE = 0;

		Button select_image, detect, view_log;
		Bitmap bitmap;
		ProgressDialog progressDialog;
		string imageUri;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.activity_detection);

			progressDialog = new ProgressDialog (this);
			progressDialog.SetTitle (Application.Context.GetString (Resource.String.progress_dialog_title));

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


		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			switch (requestCode)
			{
				case REQUEST_SELECT_IMAGE:
					if (resultCode == Result.Ok)
					{
						imageUri = data.Data.ToString ();

						bitmap = ContentResolver.LoadSizeLimitedBitmapFromUri (data.Data);

						if (bitmap != null)
						{
							var image = FindViewById<ImageView> (Resource.Id.image);
							image.SetImageBitmap (bitmap);

							AddLog ($"Image: {imageUri} resized to {bitmap.Width}x{bitmap.Height}");
						}

						var faceListAdapter = new FaceListAdapter (null, null);
						var list_detected_faces = FindViewById<ListView> (Resource.Id.list_detected_faces);
						list_detected_faces.Adapter = faceListAdapter;

						SetInfo ("");

						SetDetectButtonEnabledStatus (true);
					}
					break;
			}
		}


		void Select_Image_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (SelectImageActivity));
			StartActivityForResult (intent, REQUEST_SELECT_IMAGE);
		}


		async void Detect_Click (object sender, EventArgs e)
		{
			await ExecuteDetection ();

			SetAllButtonsEnabledStatus (false);
		}


		async Task ExecuteDetection ()
		{
			try
			{
				progressDialog.Show ();
				progressDialog.SetMessage ("Detecting...");
				SetInfo ("Detecting...");
				AddLog ($"Request: Detecting in image {imageUri}");

				var faces = await FaceClient.Shared.DetectFacesInPhoto (
					() => bitmap.AsJpeg (),
					true,
			   		FaceAttributeType.Age,
					FaceAttributeType.Gender,
					FaceAttributeType.Smile,
					FaceAttributeType.Glasses,
					FaceAttributeType.FacialHair,
					FaceAttributeType.Emotion,
					FaceAttributeType.HeadPose);

				// Show the result on screen when detection is done.
				var list_detected_faces = FindViewById<ListView> (Resource.Id.list_detected_faces);

				string detectionResult = "0 face detected";

				if (faces?.Count > 0)
				{
					detectionResult = $"{faces.Count} face{(faces.Count != 1 ? "s" : "")} detected";
					AddLog ($"Response: Success. {detectionResult} in {imageUri}");

					var image = FindViewById<ImageView> (Resource.Id.image);
					image.SetImageBitmap (ImageHelper.DrawFaceRectanglesOnBitmap (bitmap, faces, true));

					var faceListAdapter = new FaceListAdapter (faces, bitmap);
					list_detected_faces.Adapter = faceListAdapter;
				}

				SetInfo (detectionResult);
			}
			catch (Exception e)
			{
				AddLog (e.Message);
			}

			progressDialog.Dismiss ();
			SetAllButtonsEnabledStatus (true);
			SetDetectButtonEnabledStatus (false);

			imageUri = null;
			bitmap = null;
		}


		void View_Log_Click (object sender, EventArgs e)
		{
			var intent = new Intent (this, typeof (DetectionLogActivity));
			StartActivity (intent);
		}


		void SetDetectButtonEnabledStatus (bool isEnabled)
		{
			detect.Enabled = isEnabled;
		}


		void SetAllButtonsEnabledStatus (bool isEnabled)
		{
			select_image.Enabled = isEnabled;
			detect.Enabled = isEnabled;
			view_log.Enabled = isEnabled;
		}


		void SetInfo (string inf)
		{
			var info = FindViewById<TextView> (Resource.Id.info);
			info.Text = inf;
		}


		void AddLog (string _log)
		{
			LogHelper.AddDetectionLog (_log);
		}


		class FaceListAdapter : BaseAdapter<Model.Face>
		{
			readonly List<Bitmap> faceThumbnails;

			public List<Model.Face> DetectedFaces { get; private set; }

			public FaceListAdapter (List<Model.Face> detectedFaces, Bitmap photo)
			{
				DetectedFaces = detectedFaces;

				if (detectedFaces != null && photo != null)
				{
					faceThumbnails = detectedFaces.GenerateThumbnails (photo);
				}
			}


			public override bool IsEnabled (int position) => false;


			public override int Count => DetectedFaces?.Count ?? 0;


			public override Model.Face this [int position] => DetectedFaces [position];


			public override long GetItemId (int position) => position;


			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					var layoutInflater = (LayoutInflater) Application.Context.GetSystemService (LayoutInflaterService);
					convertView = layoutInflater.Inflate (Resource.Layout.item_face_with_description, parent, false);
				}

				convertView.Id = position;

				var face = DetectedFaces [position];

				convertView.FindViewById<ImageView> (Resource.Id.face_thumbnail).SetImageBitmap (faceThumbnails [position]);

				string face_description = string.Format ("Age: {0}\nGender: {1}\nSmile: {2}\nGlasses: {3}\nFacialHair: {4}\nHeadPose: {5}",
						face.Attributes.Age,
						face.Attributes.Gender,
						face.Attributes.SmileIntensity,
						face.Attributes.Glasses,
						GetFacialHair (face.Attributes.FacialHair),
						GetEmotion (face.Attributes.Emotion),
						GetHeadPose (face.Attributes.HeadPose));

				var text_detected_face = convertView.FindViewById<TextView> (Resource.Id.text_detected_face);
				text_detected_face.Text = face_description;

				return convertView;
			}


			string GetFacialHair (FacialHair facialHair)
			{
				return (facialHair.Mustache + facialHair.Beard + facialHair.Sideburns > 0) ? "Yes" : "No";
			}


			string GetEmotion (FaceEmotion emotion)
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


			string GetHeadPose (FaceHeadPose headPose)
			{
				return string.Format ("Roll: {0}, Yaw: {1}", headPose.Roll, headPose.Yaw);
			}
		}
	}
}
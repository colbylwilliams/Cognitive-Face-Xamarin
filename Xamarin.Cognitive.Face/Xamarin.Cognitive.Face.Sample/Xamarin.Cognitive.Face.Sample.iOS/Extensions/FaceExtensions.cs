using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Xamarin.Cognitive.Face.Sample.Shared;
using Foundation;
using NomadCode.UIExtensions;
using UIKit;
using Xamarin.Cognitive.Face.iOS;
using Xamarin.Cognitive.Face.Sample.Shared.Extensions;

namespace Xamarin.Cognitive.Face.Sample.iOS.Extensions
{
	public static class FaceExtensions
	{
		static string docsDir;

		static FaceExtensions ()
		{
			docsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}


		public static float AsFloatSafe (this NSNumber number, float defaultValue = 0)
		{
			return number?.FloatValue ?? defaultValue;
		}


		public static bool AsBoolSafe (this NSNumber number, bool defaultValue = false)
		{
			return number?.BoolValue ?? defaultValue;
		}


		public static DateTime? AsDateSafe (this string dateString, DateTime? defaultValue = null)
		{
			DateTime? date = defaultValue;

			if (DateTime.TryParse (dateString, out DateTime dt))
			{
				date = dt;
			}

			return date;
		}


		public static PersonGroup ToPersonGroup (this MPOPersonGroup personGroup)
		{
			return new PersonGroup
			{
				Id = personGroup.PersonGroupId,
				Name = personGroup.Name,
				UserData = personGroup.UserData
			};
		}


		public static Person ToPerson (this MPOPerson person)
		{
			return new Person
			{
				Id = person.PersonId,
				Name = person.Name,
				UserData = person.UserData,
				FaceIds = person.PersistedFaceIds?.ToList ()
			};
		}


		public static TrainingStatus ToTrainingStatus (this MPOTrainingStatus status)
		{
			var traingingStatus = TrainingStatus.FromString (status.Status);

			traingingStatus.CreatedDateTime = status.CreatedDateTime.AsDateSafe ();
			traingingStatus.LastActionDateTime = status.LastActionDateTime.AsDateSafe ();
			traingingStatus.Message = status.Message;

			return traingingStatus;
		}


		public static Shared.Face ToFace (this MPOFace mpoFace)
		{
			var rect = new RectangleF (mpoFace.FaceRectangle.Left,
									   mpoFace.FaceRectangle.Top,
									   mpoFace.FaceRectangle.Width,
									   mpoFace.FaceRectangle.Height);

			var face = new Shared.Face
			{
				Id = mpoFace.FaceId,
				FaceRectangle = rect,
				Landmarks = mpoFace.FaceLandmarks?.ToFaceLandmarks (),
				Attributes = mpoFace.Attributes?.ToFaceAttributes ()
			};

			face.UpdatePhotoPath ();

			return face;
		}


		public static Shared.Face ToFace (this MPOPersonFace mpoFace)
		{
			var face = new Shared.Face
			{
				Id = mpoFace.PersistedFaceId,
				UserData = mpoFace.UserData
			};

			face.UpdatePhotoPath ();

			return face;
		}


		public static FaceLandmarks ToFaceLandmarks (this MPOFaceLandmarks landmarks)
		{
			return new FaceLandmarks
			{
				EyebrowLeftInner = landmarks.EyebrowLeftInner.ToFeatureCoordinate (),
				EyebrowLeftOuter = landmarks.EyebrowLeftOuter.ToFeatureCoordinate (),
				EyebrowRightInner = landmarks.EyebrowLeftInner.ToFeatureCoordinate (),
				//EyebrowRightOuter = landmarks.EyebrowRightOuter.ToFeatureCoordinate (),
				EyebrowRightOuter = null,
				EyeLeftBottom = landmarks.EyeLeftBottom.ToFeatureCoordinate (),
				EyeLeftInner = landmarks.EyeLeftInner.ToFeatureCoordinate (),
				EyeLeftOuter = landmarks.EyeLeftOuter.ToFeatureCoordinate (),
				EyeRightBottom = landmarks.EyeRightBottom.ToFeatureCoordinate (),
				EyeRightInner = landmarks.EyeRightInner.ToFeatureCoordinate (),
				EyeRightOuter = landmarks.EyeRightOuter.ToFeatureCoordinate (),
				MouthLeft = landmarks.MouthLeft.ToFeatureCoordinate (),
				MouthRight = landmarks.MouthRight.ToFeatureCoordinate (),
				NoseLeftAlarOutTip = landmarks.NoseLeftAlarOutTip.ToFeatureCoordinate (),
				NoseLeftAlarTop = landmarks.NoseLeftAlarTop.ToFeatureCoordinate (),
				NoseRightAlarOutTip = landmarks.NoseRightAlarOutTip.ToFeatureCoordinate (),
				//NoseRightAlarTop = landmarks.NoseRightAlarTop.ToFeatureCoordinate (),
				NoseRightAlarTop = null,
				NoseRootLeft = landmarks.NoseRootLeft.ToFeatureCoordinate (),
				EyeLeftTop = landmarks.EyeLeftTop.ToFeatureCoordinate (),
				EyeRightTop = landmarks.EyeRightTop.ToFeatureCoordinate (),
				NoseRootRight = landmarks.NoseRootRight.ToFeatureCoordinate (),
				NoseTip = landmarks.NoseTip.ToFeatureCoordinate (),
				PupilLeft = landmarks.PupilLeft.ToFeatureCoordinate (),
				PupilRight = landmarks.PupilRight.ToFeatureCoordinate (),
				UnderLipBottom = landmarks.UnderLipBottom.ToFeatureCoordinate (),
				UnderLipTop = landmarks.UnderLipTop.ToFeatureCoordinate (),
				UpperLipBottom = landmarks.UpperLipBottom.ToFeatureCoordinate (),
				UpperLipTop = landmarks.UpperLipTop.ToFeatureCoordinate ()
			};
		}


		public static FeatureCoordinate ToFeatureCoordinate (this MPOFaceFeatureCoordinate feature)
		{
			return new FeatureCoordinate
			{
				X = feature.X,
				Y = feature.Y
			};
		}


		public static FaceAttributes ToFaceAttributes (this MPOFaceAttributes attrs)
		{
			return new FaceAttributes
			{
				Age = attrs.Age.AsFloatSafe (),
				SmileIntensity = attrs.Smile.AsFloatSafe (),
				Gender = attrs.Gender,
				Glasses = attrs.Glasses.AsEnum<Glasses> (),
				FacialHair = attrs.FacialHair.ToFacialHair (),
				HeadPose = attrs.HeadPose.ToFaceHeadPose (),
				Emotion = attrs.Emotion.ToFaceEmotion (),
				Hair = attrs.Hair.ToHair (),
				Makeup = attrs.Makeup.ToMakeup (),
				Occlusion = attrs.Occlusion.ToOcclusion (),
				Accessories = attrs.Accessories.ToAccessories (),
				Blur = attrs.Blur.ToBlur (),
				Exposure = attrs.Exposure.ToExposure (),
				Noise = attrs.Noise.ToNoise ()
			};
		}


		public static FacialHair ToFacialHair (this MPOFacialHair mpoFacialHair)
		{
			return new FacialHair
			{
				Mustache = mpoFacialHair.Mustache.AsFloatSafe (),
				Beard = mpoFacialHair.Beard.AsFloatSafe (),
				Sideburns = mpoFacialHair.Sideburns.AsFloatSafe ()
			};
		}


		public static FaceHeadPose ToFaceHeadPose (this MPOFaceHeadPose mpoHeadPose)
		{
			return new FaceHeadPose
			{
				Roll = mpoHeadPose.Roll.AsFloatSafe (),
				Yaw = mpoHeadPose.Yaw.AsFloatSafe ()
				//HeadPose's pitch value is a reserved field and will always return 0.
				//Pitch = mpoHeadPose.Pitch.AsFloatSafe ()
			};
		}


		public static FaceEmotion ToFaceEmotion (this MPOFaceEmotion mpoEmotion)
		{
			return new FaceEmotion
			{
				Anger = mpoEmotion.Anger.AsFloatSafe (),
				Contempt = mpoEmotion.Contempt.AsFloatSafe (),
				Disgust = mpoEmotion.Disgust.AsFloatSafe (),
				Fear = mpoEmotion.Fear.AsFloatSafe (),
				Happiness = mpoEmotion.Happiness.AsFloatSafe (),
				Neutral = mpoEmotion.Neutral.AsFloatSafe (),
				Sadness = mpoEmotion.Sadness.AsFloatSafe (),
				Surprise = mpoEmotion.Surprise.AsFloatSafe (),
				MostEmotionValue = mpoEmotion.MostEmotionValue.AsFloatSafe (),
				MostEmotion = mpoEmotion.MostEmotion
			};
		}


		public static Hair ToHair (this MPOHair mpoHair)
		{
			var hair = new Hair
			{
				Bald = mpoHair.Bald.AsFloatSafe (),
				Invisible = mpoHair.Invisible.AsBoolSafe (),
				HairString = mpoHair.Hair
			};

			foreach (var dict in mpoHair.HairColor)
			{
				var color = dict ["color"].ToString ();
				var confidence = dict ["confidence"] as NSNumber;

				hair.HairColor.Add (color.AsEnum<HairColorType> (), confidence.AsFloatSafe ());
			}

			return hair;
		}


		public static Makeup ToMakeup (this MPOMakeup mpoMakeup)
		{
			return new Makeup
			{
				EyeMakeup = mpoMakeup.EyeMakeup.AsBoolSafe (),
				LipMakeup = mpoMakeup.LipMakeup.AsBoolSafe ()
			};
		}


		public static Occlusion ToOcclusion (this MPOOcclusion mpoOcclusion)
		{
			return new Occlusion
			{
				ForeheadOccluded = mpoOcclusion.ForeheadOccluded.AsBoolSafe (),
				EyeOccluded = mpoOcclusion.EyeOccluded.AsBoolSafe (),
				MouthOccluded = mpoOcclusion.MouthOccluded.AsBoolSafe ()
			};
		}


		public static Accessories ToAccessories (this MPOAccessories mpoAccessories)
		{
			var accessories = new Accessories
			{
				AccessoriesString = mpoAccessories.AccessoriesString
			};

			if (mpoAccessories.Accessories.Length > 0)
			{
				foreach (var dict in mpoAccessories.Accessories)
				{
					var type = dict ["type"].ToString ();
					var confidence = dict ["confidence"] as NSNumber;

					accessories.AccessoriesList.Add (type.AsEnum<AccessoryType> (), confidence.AsFloatSafe ());
				}
			}

			return accessories;
		}


		public static Blur ToBlur (this MPOBlur mpoBlur)
		{
			return new Blur
			{
				BlurLevel = mpoBlur.BlurLevel.AsEnum<BlurLevel> (),
				Value = mpoBlur.Value.AsFloatSafe ()
			};
		}


		public static Exposure ToExposure (this MPOExposure mpoExposure)
		{
			return new Exposure
			{
				ExposureLevel = mpoExposure.ExposureLevel.AsEnum<ExposureLevel> (),
				Value = mpoExposure.Value.AsFloatSafe ()
			};
		}


		public static Noise ToNoise (this MPONoise mpoNoise)
		{
			return new Noise
			{
				NoiseLevel = mpoNoise.NoiseLevel.AsEnum<NoiseLevel> (),
				Value = mpoNoise.Value.AsFloatSafe ()
			};
		}


		public static void UpdatePhotoPath (this Shared.Face face)
		{
			var filePath = Path.Combine (docsDir, face.FileName);
			face.PhotoPath = filePath;
		}


		public static void SavePhotoFromCropped (this Shared.Face face, UIImage croppedImage)
		{
			face.UpdatePhotoPath ();
			croppedImage.SaveAsJpeg (face.PhotoPath);
		}


		public static void SavePhotoFromSource (this Shared.Face face, UIImage sourceImage)
		{
			using (var croppedFaceImg = sourceImage.Crop (face.FaceRectangle))
			{
				face.SavePhotoFromCropped (croppedFaceImg);
			}
		}


		public static UIImage GetImage (this Shared.Face face)
		{
			return UIImage.FromFile (face.PhotoPath);
		}


		public static MPOFaceRectangle ToMPOFaceRect (this RectangleF rect)
		{
			return new MPOFaceRectangle
			{
				Left = rect.Left,
				Top = rect.Top,
				Width = rect.Width,
				Height = rect.Height
			};
		}
	}
}
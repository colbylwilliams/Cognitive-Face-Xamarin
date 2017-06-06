using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Android.Graphics;
using Java.Util;
using NomadCode.UIExtensions;
using Xamarin.Cognitive.Face.Sample.Shared;

namespace Xamarin.Cognitive.Face.Sample.Droid.Extensions
{
	public static class FaceExtensions
	{
		static string docsDir;
		static DateTime epoch;

		static FaceExtensions ()
		{
			docsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);

			epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		}


		public static DateTime ToDateTime (this Java.Util.Date jDate)
		{
			return epoch.AddMilliseconds (jDate.Time);
		}


		public static PersonGroup ToPersonGroup (this Face.Droid.Contract.PersonGroup personGroup)
		{
			return new PersonGroup
			{
				Id = personGroup.PersonGroupId,
				Name = personGroup.Name,
				UserData = personGroup.UserData
			};
		}


		public static string GetFormattedGroupName (this PersonGroup personGroup)
		{
			return string.Format ("{0} (Person count: {1})", personGroup.Name, personGroup.People?.Count);
		}


		public static Person ToPerson (this Face.Droid.Contract.Person person)
		{
			return new Person
			{
				Id = person.PersonId.ToString (),
				Name = person.Name,
				UserData = person.UserData,
				//FaceIds = person.FaceIds?.Select (id => id.ToString ()).ToList ()
				FaceIds = person.PersistedFaceIds?.Select (id => id.ToString ()).ToList ()
			};
		}


		public static TrainingStatus ToTrainingStatus (this Face.Droid.Contract.TrainingStatus status)
		{
			return new TrainingStatus
			{
				CreatedDateTime = status.CreatedDateTime.ToDateTime (),
				LastActionDateTime = status.LastActionDateTime.ToDateTime (),
				Message = status.Message,
				Status = status.Status.AsEnum<TrainingStatus.TrainingStatusType> (1)
			};
		}


		public static Shared.Face ToFace (this Face.Droid.Contract.Face thisFace)
		{
			var rect = new RectangleF (thisFace.FaceRectangle.Left,
									   thisFace.FaceRectangle.Top,
									   thisFace.FaceRectangle.Width,
									   thisFace.FaceRectangle.Height);

			var thatFace = new Shared.Face
			{
				Id = thisFace.FaceId.ToString (),
				FaceRectangle = rect,
				Landmarks = thisFace.FaceLandmarks?.ToFaceLandmarks (),
				Attributes = thisFace.FaceAttributes?.ToFaceAttributes ()
			};

			thatFace.UpdatePhotoPath ();

			return thatFace;
		}


		public static Shared.Face ToFace (this Face.Droid.Contract.PersonFace personFace)
		{
			var face = new Shared.Face
			{
				Id = personFace.PersistedFaceId.ToString (),
				UserData = personFace.UserData
			};

			face.UpdatePhotoPath ();

			return face;
		}


		public static FaceLandmarks ToFaceLandmarks (this Face.Droid.Contract.FaceLandmarks landmarks)
		{
			return new FaceLandmarks
			{
				EyebrowLeftInner = landmarks.EyebrowLeftInner.ToFeatureCoordinate (),
				EyebrowLeftOuter = landmarks.EyebrowLeftOuter.ToFeatureCoordinate (),
				EyebrowRightInner = landmarks.EyebrowLeftInner.ToFeatureCoordinate (),
				EyebrowRightOuter = landmarks.EyebrowRightOuter.ToFeatureCoordinate (),
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
				NoseRightAlarTop = landmarks.NoseRightAlarTop.ToFeatureCoordinate (),
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


		public static FeatureCoordinate ToFeatureCoordinate (this Face.Droid.Contract.FeatureCoordinate feature)
		{
			return new FeatureCoordinate
			{
				X = (float) feature.X,
				Y = (float) feature.Y
			};
		}


		public static FaceAttributes ToFaceAttributes (this Face.Droid.Contract.FaceAttribute attrs)
		{
			return new FaceAttributes
			{
				Age = (float) attrs.Age,
				SmileIntensity = (float) attrs.Smile,
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


		public static FacialHair ToFacialHair (this Face.Droid.Contract.FacialHair facialHair)
		{
			return new FacialHair
			{
				Mustache = (float) facialHair.Moustache,
				Beard = (float) facialHair.Beard,
				Sideburns = (float) facialHair.Sideburns
			};
		}


		public static FaceHeadPose ToFaceHeadPose (this Face.Droid.Contract.HeadPose headPose)
		{
			return new FaceHeadPose
			{
				Roll = (float) headPose.Roll,
				Yaw = (float) headPose.Yaw
				//HeadPose's pitch value is a reserved field and will always return 0.
				//Pitch = mpoHeadPose.Pitch
			};
		}


		public static FaceEmotion ToFaceEmotion (this Face.Droid.Contract.Emotion emotion)
		{
			var faceEmotion = new FaceEmotion
			{
				Anger = (float) emotion.Anger,
				Contempt = (float) emotion.Contempt,
				Disgust = (float) emotion.Disgust,
				Fear = (float) emotion.Fear,
				Happiness = (float) emotion.Happiness,
				Neutral = (float) emotion.Neutral,
				Sadness = (float) emotion.Sadness,
				Surprise = (float) emotion.Surprise,
				MostEmotion = nameof (emotion.Anger),
				MostEmotionValue = (float) emotion.Anger
			};

			if (faceEmotion.Contempt > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Contempt);
				faceEmotion.MostEmotionValue = faceEmotion.Contempt;
			}

			if (faceEmotion.Disgust > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Disgust);
				faceEmotion.MostEmotionValue = faceEmotion.Disgust;
			}

			if (faceEmotion.Fear > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Fear);
				faceEmotion.MostEmotionValue = faceEmotion.Fear;
			}

			if (faceEmotion.Happiness > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Happiness);
				faceEmotion.MostEmotionValue = faceEmotion.Happiness;
			}

			if (faceEmotion.Neutral > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Neutral);
				faceEmotion.MostEmotionValue = faceEmotion.Neutral;
			}

			if (faceEmotion.Sadness > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Sadness);
				faceEmotion.MostEmotionValue = faceEmotion.Sadness;
			}

			if (faceEmotion.Surprise > faceEmotion.MostEmotionValue)
			{
				faceEmotion.MostEmotion = nameof (faceEmotion.Surprise);
				faceEmotion.MostEmotionValue = faceEmotion.Surprise;
			}

			return faceEmotion;
		}


		public static Hair ToHair (this Face.Droid.Contract.Hair hair)
		{
			var theHair = new Hair
			{
				Bald = (float) hair.Bald,
				Invisible = hair.Invisible
			};

			if (hair.HairColor.Count == 0)
			{
				if (hair.Invisible)
				{
					theHair.HairString = nameof (theHair.Invisible);
				}
				else
				{
					theHair.HairString = nameof (theHair.Bald);
				}
			}
			else
			{
				theHair.HairString = string.Empty;
				double hairMaxConfidence = 0.0;

				foreach (var hairColor in hair.HairColor)
				{
					theHair.HairColor.Add (hairColor.Color.AsEnum<HairColorType> (), (float) hairColor.Confidence);

					if (hairColor.Confidence > hairMaxConfidence)
					{
						theHair.HairString = hairColor.Color.ToString ();
						hairMaxConfidence = hairColor.Confidence;
					}
				}
			}

			return theHair;
		}


		public static Makeup ToMakeup (this Face.Droid.Contract.Makeup makeup)
		{
			return new Makeup
			{
				EyeMakeup = makeup.EyeMakeup,
				LipMakeup = makeup.LipMakeup
			};
		}


		public static Occlusion ToOcclusion (this Face.Droid.Contract.Occlusion occlusion)
		{
			return new Occlusion
			{
				ForeheadOccluded = occlusion.ForeheadOccluded,
				EyeOccluded = occlusion.EyeOccluded,
				MouthOccluded = occlusion.MouthOccluded
			};
		}


		public static Accessories ToAccessories (this IList<Face.Droid.Contract.Accessory> accessoryList)
		{
			var accessories = new Accessories ();

			if (accessoryList.Count > 0)
			{
				foreach (var accessoryItem in accessoryList)
				{
					accessories.AccessoriesList.Add (accessoryItem.Type.AsEnum<AccessoryType> (), (float) accessoryItem.Confidence);
				}

				accessories.AccessoriesString = string.Join (", ", accessoryList.Select (a => a.Type.Name ()));
			}
			else
			{
				accessories.AccessoriesString = "NoAccessories";
			}

			return accessories;
		}


		public static Blur ToBlur (this Face.Droid.Contract.Blur blur)
		{
			return new Blur
			{
				BlurLevel = blur.BlurLevel.AsEnum<BlurLevel> (),
				Value = (float) blur.Value
			};
		}


		public static Exposure ToExposure (this Face.Droid.Contract.Exposure exposure)
		{
			return new Exposure
			{
				ExposureLevel = exposure.ExposureLevel.AsEnum<ExposureLevel> (),
				Value = (float) exposure.Value
			};
		}


		public static Noise ToNoise (this Face.Droid.Contract.Noise noise)
		{
			return new Noise
			{
				NoiseLevel = noise.NoiseLevel.AsEnum<NoiseLevel> (),
				Value = (float) noise.Value
			};
		}


		public static void UpdatePhotoPath (this Shared.Face face)
		{
			var filePath = System.IO.Path.Combine (docsDir, face.FileName);
			face.PhotoPath = filePath;
		}


		public static void SavePhotoFromCropped (this Shared.Face face, Bitmap croppedImage)
		{
			face.UpdatePhotoPath ();
			croppedImage.SaveAsJpeg (face.PhotoPath);
		}


		public static void SavePhotoFromSource (this Shared.Face face, Bitmap sourceImage)
		{
			using (var croppedFaceImg = sourceImage.Crop (face.FaceRectangleLarge ?? face.FaceRectangle))
			{
				face.SavePhotoFromCropped (croppedFaceImg);
			}
		}


		//public static UIImage GetImage (this Shared.Face face)
		//{
		//	return UIImage.FromFile (face.PhotoPath);
		//}


		public static TEnum AsEnum<TEnum> (this Java.Lang.Enum jEnum, int offset = 0)
		{
			return (TEnum) Enum.ToObject (typeof (TEnum), jEnum.Ordinal () + offset);
		}


		public static UUID ToUUID (this string Id)
		{
			return UUID.FromString (Id);
		}


		public static Face.Droid.Contract.FaceRectangle ToFaceRect (this RectangleF rect)
		{
			return new Face.Droid.Contract.FaceRectangle
			{
				Left = (int) rect.Left,
				Top = (int) rect.Top,
				Width = (int) rect.Width,
				Height = (int) rect.Height
			};
		}
	}
}
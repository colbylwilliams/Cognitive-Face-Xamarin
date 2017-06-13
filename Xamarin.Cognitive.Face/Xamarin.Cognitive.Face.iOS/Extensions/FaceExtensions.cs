using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Foundation;
using UIKit;
using Xamarin.Cognitive.Face.iOS;
using Xamarin.Cognitive.Face.Extensions;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Extensions
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


		public static Model.Face ToFace (this MPOFace mpoFace, bool adaptLandmarks = false, FaceAttributeType [] attributes = null)
		{
			var rect = new RectangleF (mpoFace.FaceRectangle.Left,
									   mpoFace.FaceRectangle.Top,
									   mpoFace.FaceRectangle.Width,
									   mpoFace.FaceRectangle.Height);

			var face = new Model.Face
			{
				Id = mpoFace.FaceId,
				FaceRectangle = rect
			};

			face.Attributes = mpoFace.Attributes?.ToFaceAttributes (attributes);

			if (adaptLandmarks)
			{
				face.Landmarks = mpoFace.FaceLandmarks?.ToFaceLandmarks ();
			}

			face.UpdatePhotoPath ();

			return face;
		}


		public static Model.Face ToFace (this MPOPersonFace mpoFace)
		{
			var face = new Model.Face
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
				X = feature.X.AsFloatSafe (),
				Y = feature.Y.AsFloatSafe ()
			};
		}


		public static FaceAttributes ToFaceAttributes (this MPOFaceAttributes attrs, FaceAttributeType [] attributes = null)
		{
			if (attributes?.Length > 0)
			{
				var faceAttributes = new FaceAttributes ();

				foreach (var attr in attributes)
				{
					switch (attr)
					{
						case FaceAttributeType.Age:
							faceAttributes.Age = attrs.Age.AsFloatSafe ();
							break;
						case FaceAttributeType.Smile:
							faceAttributes.SmileIntensity = attrs.Smile.AsFloatSafe ();
							break;
						case FaceAttributeType.Gender:
							faceAttributes.Gender = attrs.Gender;
							break;
						case FaceAttributeType.Glasses:
							faceAttributes.Glasses = attrs.Glasses.AsEnum<Glasses> ();
							break;
						case FaceAttributeType.FacialHair:
							faceAttributes.FacialHair = attrs.FacialHair.ToFacialHair ();
							break;
						case FaceAttributeType.HeadPose:
							faceAttributes.HeadPose = attrs.HeadPose.ToFaceHeadPose ();
							break;
						case FaceAttributeType.Emotion:
							faceAttributes.Emotion = attrs.Emotion.ToFaceEmotion ();
							break;
						case FaceAttributeType.Hair:
							faceAttributes.Hair = attrs.Hair.ToHair ();
							break;
						case FaceAttributeType.Makeup:
							faceAttributes.Makeup = attrs.Makeup.ToMakeup ();
							break;
						case FaceAttributeType.Occlusion:
							faceAttributes.Occlusion = attrs.Occlusion.ToOcclusion ();
							break;
						case FaceAttributeType.Accessories:
							faceAttributes.Accessories = attrs.Accessories.ToAccessories ();
							break;
						case FaceAttributeType.Blur:
							faceAttributes.Blur = attrs.Blur.ToBlur ();
							break;
						case FaceAttributeType.Exposure:
							faceAttributes.Exposure = attrs.Exposure.ToExposure ();
							break;
						case FaceAttributeType.Noise:
							faceAttributes.Noise = attrs.Noise.ToNoise ();
							break;
					}
				}

				return faceAttributes;
			}

			return null;
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
			if (mpoEmotion.MostEmotionValue != null)
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

			return null;
		}


		public static Hair ToHair (this MPOHair mpoHair)
		{
			if (mpoHair.HairColor?.Length > 0)
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

			return null;
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

			if (mpoAccessories.Accessories?.Length > 0)
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
			if (mpoBlur.Value != null)
			{
				return new Blur
				{
					BlurLevel = mpoBlur.BlurLevel.AsEnum<BlurLevel> (),
					Value = mpoBlur.Value.AsFloatSafe ()
				};
			}

			return null;
		}


		public static Exposure ToExposure (this MPOExposure mpoExposure)
		{
			if (mpoExposure.Value != null)
			{
				return new Exposure
				{
					ExposureLevel = mpoExposure.ExposureLevel.AsEnum<ExposureLevel> (),
					Value = mpoExposure.Value.AsFloatSafe ()
				};
			}

			return null;
		}


		public static Noise ToNoise (this MPONoise mpoNoise)
		{
			if (mpoNoise.Value != null)
			{
				return new Noise
				{
					NoiseLevel = mpoNoise.NoiseLevel.AsEnum<NoiseLevel> (),
					Value = mpoNoise.Value.AsFloatSafe ()
				};
			}

			return null;
		}


		public static MPOFaceAttributeType ToNativeFaceAttributeType (this FaceAttributeType type)
		{
			return (MPOFaceAttributeType) ((int) type + 1);
		}


		public static SimilarFaceResult ToSimilarFaceResult (this MPOSimilarFace similarFace)
		{
			return new SimilarFaceResult
			{
				FaceId = similarFace.FaceId,
				Confidence = similarFace.Confidence
			};
		}


		public static GroupResult ToGroupResult (this MPOGroupResult groupResult)
		{
			var groupingResult = new GroupResult
			{
				Groups = groupResult.Groups.Select ((grp, index) => new FaceGroup
				{
					Title = $"Face Group #{index + 1}",
					FaceIds = grp.ToList ()
				}).ToList ()
			};

			if (groupResult.MesseyGroup?.Length > 0)
			{
				groupingResult.MessyGroup = new FaceGroup
				{
					Title = "Messy Group",
					FaceIds = groupResult.MesseyGroup.ToList ()
				};
			}

			return groupingResult;
		}


		public static IdentificationResult ToIdentificationResult (this MPOIdentifyResult result)
		{
			return new IdentificationResult
			{
				FaceId = result.FaceId,
				CandidateResults =
					result.Candidates
						  .Select (c => new CandidateResult
						  {
							  PersonId = c.PersonId,
							  Confidence = c.Confidence
						  })
						  .ToList ()
			};
		}


		public static VerifyResult ToVerifyResult (this MPOVerifyResult result)
		{
			return new VerifyResult
			{
				IsIdentical = result.IsIdentical,
				Confidence = result.Confidence
			};
		}


		public static void UpdatePhotoPath (this Model.Face face)
		{
			var filePath = Path.Combine (docsDir, face.FileName);
			face.PhotoPath = filePath;
		}


		public static void SavePhotoFromCropped (this Model.Face face, UIImage croppedImage)
		{
			face.UpdatePhotoPath ();
			croppedImage.SaveAsJpeg (face.PhotoPath);
		}


		public static void SavePhotoFromSource (this Model.Face face, UIImage sourceImage)
		{
			using (var croppedFaceImg = sourceImage.Crop (face.FaceRectangle))
			{
				face.SavePhotoFromCropped (croppedFaceImg);
			}
		}


		public static UIImage CreateThumbnail (this Model.Face face, UIImage sourceImage)
		{
			return sourceImage.Crop (face.FaceRectangle);
		}


		public static UIImage GetImage (this Model.Face face)
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
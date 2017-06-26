using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Java.Util;
using Xamarin.Cognitive.Face.Model;

namespace Xamarin.Cognitive.Face.Extensions
{
	/// <summary>
	/// Contains extension methods for working with and mapping native Droid.Contact.* types from the bound Android Face SDK.
	/// </summary>
	public static class MappingExtensions
	{
		internal static Droid.Contract.FaceRectangle ToNativeFaceRect (this FaceRectangle rect)
		{
			return new Droid.Contract.FaceRectangle
			{
				Left = (int) rect.Left,
				Top = (int) rect.Top,
				Width = (int) rect.Width,
				Height = (int) rect.Height
			};
		}


		internal static FaceRectangle ToFaceRectangle (this Droid.Contract.FaceRectangle rect)
		{
			return new FaceRectangle
			{
				Left = rect.Left,
				Top = rect.Top,
				Width = rect.Width,
				Height = rect.Height
			};
		}


		internal static PersonGroup ToPersonGroup (this Droid.Contract.PersonGroup personGroup)
		{
			return new PersonGroup
			{
				Id = personGroup.PersonGroupId,
				Name = personGroup.Name,
				UserData = personGroup.UserData
			};
		}


		internal static Person ToPerson (this Droid.Contract.Person person)
		{
			return new Person
			{
				Id = person.PersonId.ToString (),
				Name = person.Name,
				UserData = person.UserData,
				FaceIds = person.PersistedFaceIds?.Select (id => id.ToString ()).ToList ()
			};
		}


		internal static TrainingStatus ToTrainingStatus (this Droid.Contract.TrainingStatus status)
		{
			return new TrainingStatus
			{
				CreatedDateTime = status.CreatedDateTime.ToDateTime (),
				LastActionDateTime = status.LastActionDateTime.ToDateTime (),
				Message = status.Message,
				Status = status.Status.AsEnum<TrainingStatus.TrainingStatusType> (1)
			};
		}


		internal static Model.Face ToFace (this Droid.Contract.Face thisFace, bool adaptLandmarks = false, FaceAttributeType [] attributes = null)
		{
			var thatFace = new Model.Face
			{
				Id = thisFace.FaceId.ToString (),
				FaceRectangle = thisFace.FaceRectangle.ToFaceRectangle (),
				Attributes = thisFace.FaceAttributes?.ToFaceAttributes (attributes)
			};

			if (adaptLandmarks)
			{
				thatFace.Landmarks = thisFace.FaceLandmarks?.ToFaceLandmarks ();
			}

			thatFace.UpdateThumbnailPath ();

			return thatFace;
		}


		internal static Model.Face ToFace (this Droid.Contract.PersonFace personFace)
		{
			var face = new Model.Face
			{
				Id = personFace.PersistedFaceId.ToString (),
				UserData = personFace.UserData
			};

			face.UpdateThumbnailPath ();

			return face;
		}


		internal static FaceLandmarks ToFaceLandmarks (this Droid.Contract.FaceLandmarks landmarks)
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


		internal static FeatureCoordinate ToFeatureCoordinate (this Droid.Contract.FeatureCoordinate feature)
		{
			return new FeatureCoordinate
			{
				X = (float) feature.X,
				Y = (float) feature.Y
			};
		}


		internal static FaceAttributes ToFaceAttributes (this Droid.Contract.FaceAttribute attrs, FaceAttributeType [] attributes = null)
		{
			if (attributes?.Length > 0)
			{
				var faceAttributes = new FaceAttributes ();

				foreach (var attr in attributes)
				{
					switch (attr)
					{
						case FaceAttributeType.Age:
							faceAttributes.Age = (float) attrs.Age;
							break;
						case FaceAttributeType.Smile:
							faceAttributes.SmileIntensity = (float) attrs.Smile;
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


		internal static FacialHair ToFacialHair (this Droid.Contract.FacialHair facialHair)
		{
			return new FacialHair
			{
				Mustache = (float) facialHair.Moustache,
				Beard = (float) facialHair.Beard,
				Sideburns = (float) facialHair.Sideburns
			};
		}


		internal static FaceHeadPose ToFaceHeadPose (this Droid.Contract.HeadPose headPose)
		{
			return new FaceHeadPose
			{
				Roll = (float) headPose.Roll,
				Yaw = (float) headPose.Yaw
				//HeadPose's pitch value is a reserved field and will always return 0.
				//Pitch = mpoHeadPose.Pitch
			};
		}


		internal static FaceEmotion ToFaceEmotion (this Droid.Contract.Emotion emotion)
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


		internal static Hair ToHair (this Droid.Contract.Hair hair)
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


		internal static Makeup ToMakeup (this Droid.Contract.Makeup makeup)
		{
			return new Makeup
			{
				EyeMakeup = makeup.EyeMakeup,
				LipMakeup = makeup.LipMakeup
			};
		}


		internal static Occlusion ToOcclusion (this Droid.Contract.Occlusion occlusion)
		{
			return new Occlusion
			{
				ForeheadOccluded = occlusion.ForeheadOccluded,
				EyeOccluded = occlusion.EyeOccluded,
				MouthOccluded = occlusion.MouthOccluded
			};
		}


		internal static Accessories ToAccessories (this IList<Droid.Contract.Accessory> accessoryList)
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


		internal static Blur ToBlur (this Droid.Contract.Blur blur)
		{
			return new Blur
			{
				BlurLevel = blur.BlurLevel.AsEnum<BlurLevel> (),
				Value = (float) blur.Value
			};
		}


		internal static Exposure ToExposure (this Droid.Contract.Exposure exposure)
		{
			return new Exposure
			{
				ExposureLevel = exposure.ExposureLevel.AsEnum<ExposureLevel> (),
				Value = (float) exposure.Value
			};
		}


		internal static Noise ToNoise (this Droid.Contract.Noise noise)
		{
			return new Noise
			{
				NoiseLevel = noise.NoiseLevel.AsEnum<NoiseLevel> (),
				Value = (float) noise.Value
			};
		}


		internal static SimilarFaceResult ToSimilarFaceResult (this Droid.Contract.SimilarFace similarFace)
		{
			return new SimilarFaceResult
			{
				FaceId = similarFace.FaceId.ToString (),
				Confidence = (float) similarFace.Confidence
			};
		}


		internal static GroupResult ToGroupResult (this Droid.Contract.GroupResult groupResult)
		{
			var typedList = ((JavaList) groupResult.Groups).JavaCast<JavaList<UUID []>> ();

			var groupingResult = new GroupResult
			{
				Groups = typedList.Select ((grp, index) => new FaceGroup
				{
					Title = $"Face Group #{index + 1}",
					FaceIds = grp.AsStrings ()
				}).ToList ()
			};

			if (groupResult.MessyGroup?.Count > 0)
			{
				groupingResult.MessyGroup = new FaceGroup
				{
					Title = "Messy Group",
					FaceIds = groupResult.MessyGroup.Cast<UUID> ().AsStrings ()
				};
			}

			return groupingResult;
		}


		internal static IdentificationResult ToIdentificationResult (this Droid.Contract.IdentifyResult result)
		{
			return new IdentificationResult
			{
				FaceId = result.FaceId.ToString (),
				CandidateResults =
					result.Candidates.Cast<Droid.Contract.Candidate> ()
						  .Select (c => new CandidateResult
						  {
							  PersonId = c.PersonId.ToString (),
							  Confidence = (float) c.Confidence
						  })
						  .ToList ()
			};
		}


		internal static VerifyResult ToVerifyResult (this Droid.Contract.VerifyResult result)
		{
			return new VerifyResult
			{
				IsIdentical = result.IsIdentical,
				Confidence = (float) result.Confidence
			};
		}


		internal static Error ToError (this Droid.Rest.ClientException ce)
		{
			return new Error
			{
				Code = ce?.Error?.Code,
				Message = ce?.Error?.Message
			};
		}
	}
}
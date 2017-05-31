using System;
using System.Linq;
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
				FaceIds = person.FaceIds?.Select (id => id.ToString ()).ToList ()
			};
		}


		public static TrainingStatus ToTrainingStatus (this Face.Droid.Contract.TrainingStatus status)
		{
			return new TrainingStatus
			{
				CreatedDateTime = status.CreatedDateTime.ToDateTime (),
				LastActionDateTime = status.LastActionDateTime.ToDateTime (),
				Message = status.Message,
				Status = (TrainingStatus.TrainingStatusType) (status.Status.Ordinal () + 1)
			};
		}


		//public static Shared.Face ToFace (this MPOFace mpoFace)
		//{
		//	var rect = new RectangleF (mpoFace.FaceRectangle.Left,
		//							   mpoFace.FaceRectangle.Top,
		//							   mpoFace.FaceRectangle.Width,
		//							   mpoFace.FaceRectangle.Height);

		//	var face = new Shared.Face
		//	{
		//		Id = mpoFace.FaceId,
		//		FaceRectangle = rect,
		//		Attributes = mpoFace.Attributes?.ToFaceAttributes ()
		//	};

		//	face.UpdatePhotoPath ();

		//	return face;
		//}


		//public static Shared.Face ToFace (this MPOPersonFace mpoFace)
		//{
		//	var face = new Shared.Face
		//	{
		//		Id = mpoFace.PersistedFaceId,
		//		UserData = mpoFace.UserData
		//	};

		//	face.UpdatePhotoPath ();

		//	return face;
		//}


		//public static FaceAttributes ToFaceAttributes (this MPOFaceAttributes attrs)
		//{
		//	return new FaceAttributes
		//	{
		//		Age = attrs.Age.AsFloatSafe (),
		//		SmileIntensity = attrs.Smile.AsFloatSafe (),
		//		Gender = attrs.Gender,
		//		Glasses = attrs.Glasses,
		//		HeadPose = attrs.HeadPose.ToFaceHeadPose (),
		//		Emotion = attrs.Emotion.ToFaceEmotion (),
		//		Hair = attrs.Hair.ToHair (),
		//		Makeup = attrs.Makeup.ToMakeup (),
		//		Occlusion = attrs.Occlusion.ToOcclusion (),
		//		Accessories = attrs.Accessories.ToAccessories (),
		//		Blur = attrs.Blur.ToBlur (),
		//		Exposure = attrs.Exposure.ToExposure (),
		//		Noise = attrs.Noise.ToNoise ()
		//	};
		//}


		//public static FacialHair ToFacialHair (this MPOFacialHair mpoFacialHair)
		//{
		//	return new FacialHair
		//	{
		//		Mustache = mpoFacialHair.Mustache.AsFloatSafe (),
		//		Beard = mpoFacialHair.Beard.AsFloatSafe (),
		//		Sideburns = mpoFacialHair.Sideburns.AsFloatSafe ()
		//	};
		//}


		//public static FaceHeadPose ToFaceHeadPose (this MPOFaceHeadPose mpoHeadPose)
		//{
		//	return new FaceHeadPose
		//	{
		//		Roll = mpoHeadPose.Roll.AsFloatSafe (),
		//		Yaw = mpoHeadPose.Yaw.AsFloatSafe ()
		//		//HeadPose's pitch value is a reserved field and will always return 0.
		//		//Pitch = mpoHeadPose.Pitch.AsFloatSafe ()
		//	};
		//}


		//public static FaceEmotion ToFaceEmotion (this MPOFaceEmotion mpoEmotion)
		//{
		//	return new FaceEmotion
		//	{
		//		Anger = mpoEmotion.Anger.AsFloatSafe (),
		//		Contempt = mpoEmotion.Contempt.AsFloatSafe (),
		//		Disgust = mpoEmotion.Disgust.AsFloatSafe (),
		//		Fear = mpoEmotion.Fear.AsFloatSafe (),
		//		Happiness = mpoEmotion.Happiness.AsFloatSafe (),
		//		Neutral = mpoEmotion.Neutral.AsFloatSafe (),
		//		Sadness = mpoEmotion.Sadness.AsFloatSafe (),
		//		Surprise = mpoEmotion.Surprise.AsFloatSafe (),
		//		MostEmotionValue = mpoEmotion.MostEmotionValue.AsFloatSafe (),
		//		MostEmotion = mpoEmotion.MostEmotion
		//	};
		//}


		//public static Hair ToHair (this MPOHair mpoHair)
		//{
		//	return new Hair
		//	{
		//		Bald = mpoHair.Bald.AsFloatSafe (),
		//		Invisible = mpoHair.Invisible.AsFloatSafe (),
		//		//HairColor = mpoHair.HairColor,
		//		HairString = mpoHair.Hair
		//	};
		//}


		//public static Makeup ToMakeup (this MPOMakeup mpoMakeup)
		//{
		//	return new Makeup
		//	{
		//		EyeMakeup = mpoMakeup.EyeMakeup.AsBoolSafe (),
		//		LipMakeup = mpoMakeup.LipMakeup.AsBoolSafe ()
		//	};
		//}


		//public static Occlusion ToOcclusion (this MPOOcclusion mpoOcclusion)
		//{
		//	return new Occlusion
		//	{
		//		ForeheadOccluded = mpoOcclusion.ForeheadOccluded.AsBoolSafe (),
		//		EyeOccluded = mpoOcclusion.EyeOccluded.AsBoolSafe (),
		//		MouthOccluded = mpoOcclusion.MouthOccluded.AsBoolSafe ()
		//	};
		//}


		//public static Accessories ToAccessories (this MPOAccessories mpoAccessories)
		//{
		//	return new Accessories
		//	{
		//		AccessoriesString = mpoAccessories.AccessoriesString,
		//		//LipMakeup = mpoAccessories.LipMakeup
		//	};
		//}


		//public static Blur ToBlur (this MPOBlur mpoBlur)
		//{
		//	return new Blur
		//	{
		//		BlurLevel = mpoBlur.BlurLevel,
		//		Value = mpoBlur.Value.AsFloatSafe ()
		//	};
		//}


		//public static Exposure ToExposure (this MPOExposure mpoExposure)
		//{
		//	return new Exposure
		//	{
		//		ExposureLevel = mpoExposure.ExposureLevel,
		//		Value = mpoExposure.Value.AsFloatSafe ()
		//	};
		//}


		//public static Noise ToNoise (this MPONoise mpoNoise)
		//{
		//	return new Noise
		//	{
		//		NoiseLevel = mpoNoise.NoiseLevel,
		//		Value = mpoNoise.Value.AsFloatSafe ()
		//	};
		//}


		//public static void UpdatePhotoPath (this Shared.Face face)
		//{
		//	var filePath = Path.Combine (docsDir, face.FileName);
		//	face.PhotoPath = filePath;
		//}


		//public static void SavePhotoFromCropped (this Shared.Face face, UIImage croppedImage)
		//{
		//	face.UpdatePhotoPath ();
		//	croppedImage.SaveAsJpeg (face.PhotoPath);
		//}


		//public static void SavePhotoFromSource (this Shared.Face face, UIImage sourceImage)
		//{
		//	using (var croppedFaceImg = sourceImage.Crop (face.FaceRectangle))
		//	{
		//		face.SavePhotoFromCropped (croppedFaceImg);
		//	}
		//}


		//public static UIImage GetImage (this Shared.Face face)
		//{
		//	return UIImage.FromFile (face.PhotoPath);
		//}


		//public static MPOFaceRectangle ToMPOFaceRect (this RectangleF rect)
		//{
		//	return new MPOFaceRectangle
		//	{
		//		Left = rect.Left,
		//		Top = rect.Top,
		//		Width = rect.Width,
		//		Height = rect.Height
		//	};
		//}
	}
}
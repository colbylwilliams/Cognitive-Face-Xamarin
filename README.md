# Xamarin.Cognitive.Face

`Xamarin.Cognitive.Face` is a client library that makes it easy to work with the [Microsoft Cognitive Services Face API](https://azure.microsoft.com/en-us/services/cognitive-services/face/) on Xamarin.iOS, Xamarin.Android, and Xamarin.Forms and/or Portable Class Library (PCL) projects.

Resources about the Face API and what it is:

- [Learn about the Face API](https://azure.microsoft.com/en-us/services/cognitive-services/face/)
- [Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/face/overview)
- [Face API Reference](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/)


# Installation

`Xamarin.Cognitive.Face` is available as a [NuGet package](https://www.nuget.org/packages/Xamarin.Cognitive.Face/) to be added to your Xamarin.iOS, Xamarin.Android, or Xamarin.Forms project(s).

The included PCL assembly is compiled against Profile 111, which is currently the same profile used for Xamarin.Forms shared PCLs.  In the case of Xamarin.Forms or any other PCL situation, you must install the package to the PCL project as well as any platform (iOS/Android) projects - this package uses the [Bait & Switch PCL pattern](https://blog.xamarin.com/creating-reusable-plugins-for-xamarin-forms/).


# What's in the box

## Bindings

This library uses two separate binding projects to native libraries:

- [Cognitive-Face-iOS-Xamarin](https://github.com/colbylwilliams/Cognitive-Face-iOS-Xamarin) - a binding for the native iOS [Microsoft Face API: iOS Client Library](https://github.com/microsoft/Cognitive-Face-iOS)
- [Cognitive-Face-Android-Xamarin](https://github.com/NateRickard/Cognitive-Face-Android-Xamarin) - a binding for the native Android [Microsoft Face API: Android Client Library](https://github.com/microsoft/Cognitive-Face-Android)

These bindings can of course be used individually in your Xamarin.iOS and/or Xamarin.Android projects if you want to build and use them directly.

The `Xamarin.Cognitive.Face` package contains pre-compiled versions of these bindings that you can use directly if so desired:

**iOS:** Use the types in the `Xamarin.Cognitive.Face.iOS` namespace, such as `MPOFaceServiceClient`.  Documentation and usage is demonstrated in the [Microsoft Face API: iOS Client Library](https://github.com/microsoft/Cognitive-Face-iOS).

**Android:** Use the types in the `Xamarin.Cognitive.Face.Droid` namespace, such as `FaceServiceRestClient`.  Documentation and usage is demonstrated in the [Microsoft Face API: Android Client Library](https://github.com/microsoft/Cognitive-Face-Android).

## Xamarin Client Library

`Xamarin.Cognitive.Face` brings the above bindings together into a single NuGet package and adds a unified API that can be called from either platform projects _or_ shared code (shared project or portable class library).

This higher-level library contains a number of value-adds and niceties that make it easy to work with the Face API:

- Full `async/await` support across the API, for awesome and responsive apps.
- A full set of extended domain/model classes that improve upon the native classes.
- Extensions to ease working with and consuming native images (UIImage/Bitmap) on each platform.
- Simple support for storing face thumbnail images "on disk."
- Data caching on retrieved items like `PersonGroup` lists, etc.
- API equivalnce on both platforms, i.e. the ability to also use the client library's API in a Shared Project.

## Sample Apps

- `Xamarin.Cognitive.Face.Sample.iOS` 
- `Xamarin.Cognitive.Face.Sample.Droid` 

These samples exercise most areas of the Face API, including `Person`/`PersonGroup`/`Face` management, face detection, person identification, verification, face grouping, and more.

These samples are based off of and heavily influenced by the original samples found in the native client libraries (linked above), but have been heavily updated to Xamarin-ize and use the [Xamarin Client Library](#Xamarin Client Library).


# What's NOT in the box

- Windows UWP support
- There's no unified API for or samples showing [FaceLists](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395250)
- Performance and/or memory profiling has not been done, so buyer beware!

If you'd like to address any of the above, we're happy to review and merge any quality [pull requests](https://github.com/colbylwilliams/Cognitive-Face-Xamarin/pulls)!


# Use

1. Add the [NuGet package](https://www.nuget.org/packages/Xamarin.Cognitive.Face/) to your Xamarin.iOS and/or Xamarin.Android project, and any PCL project where the `Xamarin.Cognitive.Face` library is needed.

2. The `Xamarin.Cognitive.Face` client is exposed as a Singleton that can be referenced via `FaceClient.Shared`.

	Initialize the `FaceClient` with a subscription key ([get one here!](https://azure.microsoft.com/en-us/try/cognitive-services/)), and, optionally, set the endpoint (defaults to WestUS):

	```
	FaceClient.Shared.Endpoint = Endpoints.WestUS;
	FaceClient.Shared.SubscriptionKey = faceApiKey;
	```

	**NOTE:** Subscription keys are tied to a specific endpoint, so pay attention when creating a new key.

	At this point, the `FaceClient` is configured and ready to use.

3. Use the `FaceClient` to interact with the Face API.  You can do things such as:

	- **Get all `PersonGroup`**
	
		```
		List<PersonGroup> groups = await FaceClient.Shared.GetPersonGroups ();
		```
		
		This operation will cache the list of groups the first time and continue to return the cahced list until your next session, or you tell it to refresh the cache:
		
		```
		List<PersonGroup> groups = await FaceClient.Shared.GetPersonGroups (true); //true == force refresh
		```
		
	- **Detect faces in an image**
	
		```
		List<Face> detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (SourceImage.AsJpegStream);
		```
	
		Or, with facial landmarks and face attributes specified:
	
		```
		List<Face> detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (
								SourceImage.AsJpegStream,
								true, //return landmarks
								FaceAttributeType.Age,
								FaceAttributeType.Gender,
								FaceAttributeType.Hair,
								FaceAttributeType.FacialHair,
								FaceAttributeType.Makeup,
								FaceAttributeType.Emotion,
								FaceAttributeType.Occlusion,
								FaceAttributeType.Smile,
								FaceAttributeType.Exposure,
								FaceAttributeType.Noise,
								FaceAttributeType.Blur,
								FaceAttributeType.Glasses,
								FaceAttributeType.HeadPose,
								FaceAttributeType.Accessories);
		```
		
		`SourceImage` can be either a Bitmap (Android) or UIImage (iOS).
		
	- **Person Identification**
	
		To identify a face to a `Person` within a `PersonGroup`:
	
		```
		PersonGroup MyPersonGroup;
		Face MyFaceToIdentify;
		
		List<IdentificationResult> results = await FaceClient.Shared.Identify (MyPersonGroup, MyFaceToIdentify);
		```
		
	...And much more.  Take a look at the [Sample Apps](../master/Xamarin.Cognitive.Face/Xamarin.Cognitive.Face.Sample) to see the usage of these and more features.
	

# Gotchas

## Face thumbnail images

It is **not possible** to retrieve stored Face images via the Face API.  If your use case requires working with face thubnail images, `Xamarin.Cognitive.Face` provides simple support for working with and storing any persisted Face images to the local file system on both platforms.

The following extensions are available on the `Face` class:

iOS:

- `UIImage CreateThumbnail (this Model.Face face, UIImage sourceImage)`
- `List<UIImage> GenerateThumbnails (this List<Model.Face> faces, UIImage photo, List<UIImage> thumbnailList = null)`
- `void SaveThumbnail (this Model.Face face, UIImage thumbnail)`
- `void SaveThumbnailFromSource (this Model.Face face, UIImage sourceImage)`
- `UIImage GetThumbnailImage (this Model.Face face)`

	Example:
	
	```
	Person myPerson;
	PersonGroup myPersonGroup;
	Face myFace;
	UIImage sourceImage;
	...
	await FaceClient.Shared.AddFaceForPerson (myPerson, myPersonGroup, myFace, sourceImage.AsStream);
	face.SaveThumbnailFromSource (sourceImage);
	```

Android:

- `Bitmap CreateThumbnail (this Model.Face face, Bitmap sourceImage)`
- `List<Bitmap> GenerateThumbnails (this List<Model.Face> faces, Bitmap photo, List<Bitmap> thumbnailList = null)`
- `void SaveThumbnail (this Model.Face face, Bitmap thumbnail)`
- `void SaveThumbnailFromSource (this Model.Face face, Bitmap sourceImage)`
- `Bitmap GetThumbnailImage (this Model.Face face)`

	Example:

	```
	Person myPerson;
	PersonGroup myPersonGroup;
	Face myFace;
	Bitmap sourceImage;
	...
	await FaceClient.Shared.AddFaceForPerson (myPerson, myPersonGroup, myFace, sourceImage.AsStream);
	face.SaveThumbnailFromSource (sourceImage);
	```

On either platform, if you already have a cropped face thumbnail image (via `CreateThumbnail()`), you may call `face.SaveThumbnail (thumbnail)` rather than `SaveThumbnailFromSource(sourceImage)`.

**NOTE:** Any images persisted via the `SaveThumbnail*` methods will be stored on disk at the path found on the `ThumbnailPath` property of the `Face`.  These thumbnails will only be available (via `GetThumbnailImage ()`) for the life of the app on a given device, i.e. they will not persist across app removals, etc.  If you desire long-term face thumbnail storage, we suggest using another storage option for your thumbnail images.

## Exceptions

The Face API throws a handful of exceptions for most API methods.  A list of possible exceptions that may be thrown for a given operation can be found [in the API documentation](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/).

Any API exception received from the Face API will be raised to the caller in the form of an `ErrorDetailException`.

### Rate limiting

A common example of one of these exceptions is a "rate limit exceeded" exception.  Too many API calls made within a window of time may result in this exception being returned.  In this case, the caller will receive an `ErrorDetailException` wrapping an `Error` similar to this:

```
Error
{
	Code: "429",
	Message: "Rate limit is exceeded. Try again in XXX seconds"
}
```

	
# Contributing

Contributions are welcome.  Feel free to file issues and pull requests on the repo and we'll address them as we can.


# About

- Created by [Nate Rickard](https://github.com/naterickard)
- Contributions from [Colby Williams](https://github.com/colbylwilliams) (iOS binding & packaging) and [Roberto Cervantes](https://github.com/rcervantes) (work on the Android sample app)

## License

Licensed under the MIT License (MIT). See [LICENSE](LICENSE) for details.
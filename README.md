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

- Full `async/await` support for awesome, responsive apps.
- A full set of extended domain/model classes that improve upon the native classes.
- Extensions to easily work with face thumbnails/images on both platforms.
- Data caching on retrieved items like `PersonGroup` lists, etc.

## Sample Apps

- `Xamarin.Cognitive.Face.Sample.iOS` 
- `Xamarin.Cognitive.Face.Sample.Droid` 

These samples exercise most areas of the Face API, including `Person`/`PersonGroup`/'`Face` management, face detection, person identification, verification, face grouping, and more.

These samples are based off of and heavily influenced by the original samples found in the native client libraries (linked above), but have been heavily updated to Xamarin-ize and use the [Xamarin Client Library](#Xamarin Client Library).


# What's NOT in the box

- Windows UWP support
- There's no unified API for or samples showing [FaceLists](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395250)
- Performance and/or memory profiling has not been done, so buyer beware!

If you'd like to addres any of the above, we're happy to review and merge any quality [pull requests](pulls)!


# Use

1. Add the [NuGet package](https://www.nuget.org/packages/Xamarin.Cognitive.Face/) to your Xamarin.iOS and/or Xamarin.Android project, and any PCL project where the `Xamarin.Cognitive.Face` library is needed.

2. The `Xamarin.Cognitive.Face` client is exposed as a Singleton that can be referenced via `FaceClient.Shared`.

	Initialize the `FaceClient` with a subscription key ([get one here!](https://azure.microsoft.com/en-us/try/cognitive-services/)), and, optionally, set the endpoint (defaults to WestUS):

	```
	FaceClient.Shared.Endpoint = FaceClient.Endpoints.WestUS;
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
		List<Face> detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (SourceImage.AsStream);
		```
	
		Or, with facial landmarks and face attributes specified:
	
		```
		List<Face> detectedFaces = await FaceClient.Shared.DetectFacesInPhoto (
								SourceImage.AsStream,
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
		
	- **Person Identification**
	
		To identify a face to a `Person` within a `PersonGroup`:
	
		```
		PersonGroup MyPersonGroup;
		Face MyFaceToIdentify;
		
		List<IdentificationResult> results = await FaceClient.Shared.Identify (MyPersonGroup, MyFaceToIdentify);
		```
		
	...And much more.  Take a look at the [Sample Apps](../master/Xamarin.Cognitive.Face/Xamarin.Cognitive.Face.Sample) to see the usage of these and more features.
	
	
# Contributing

Contributions are welcome.  Feel free to file issues and pull requests on the repo and we'll address them as we can.


# About

- Created by [Nate Rickard](https://github.com/naterickard)
- Contributions from [Colby Williams](https://github.com/colbylwilliams) (iOS binding & packaging) and [Roberto Cervantes](https://github.com/rcervantes) (work on the Android sample app)

## License

Licensed under the MIT License (MIT). See [LICENSE](LICENSE) for details.
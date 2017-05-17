VERSION=0.3.0-preview

CONFIG=Release

PROJECT=Xamarin.Cognitive.Face
PACKAGE=$(PROJECT).$(VERSION).nupkg


all : clean ./builds/$(CONFIG)/$(PACKAGE)


./builds/$(CONFIG)/$(PACKAGE) :
	$(MAKE) -C ./Cognitive-Face-iOS-Xamarin
	$(MAKE) -C ./Cognitive-Face-Android-Xamarin/Cognitive.Face.Android/extern
	msbuild ./$(PROJECT)/$(PROJECT).Nuget/$(PROJECT).Nuget.nuproj /t:Rebuild /p:Configuration=$(CONFIG)


clean :
	rm -rf ./builds
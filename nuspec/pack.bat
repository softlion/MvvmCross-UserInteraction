del *.nupkg

set version=108.0.0.7

nuget pack Chance.MvvmCross.Plugins.UserInteraction.nuspec -Version %version%
copy /y *.nupkg ..\..\nugets
pause

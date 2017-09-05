del *.nupkg

set version=200.6.3

nuget pack Chance.MvvmCross.Plugins.UserInteraction.nuspec -Version %version%
copy /y *.nupkg ..\..\nugets

NuGet Push Chance.MvvmCross.Plugins.UserInteraction.%version%.nupkg -Source http://nugets.vapolia.fr/
pause

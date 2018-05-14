del *.nupkg

set version=1.0.0

nuget pack Vapolia.MvvmCross.UserInteraction.nuspec -Version %version%
copy /y *.nupkg ..\..\nugets

NuGet Push Vapolia.MvvmCross.UserInteraction.%version%.nupkg -Source http://nugets.vapolia.fr/
pause

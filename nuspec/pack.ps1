######NuGet.CommandLine is too old and does not support Xamarin.iOS libs
#####Please install the package "NuGe t.CommandLine" from https://chocolatey.org/ before running this script
#####After chocolatey is installed, type: choco install NuGet.CommandLine
#####Before running this script, download nuget.exe from @echo https://nuget.codeplex.com/releases/view/133091
#####and put nuget.exe in the path.

#####set /p nugetServer=Enter base nuget server url (with /): 
$nugetServer="http://nugets.vapolia.fr/"

#####################
#Build release config
cd ..
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe'
nuget restore
$msbuildparams = '/t:Clean;Build', '/p:Configuration=Release', '/p:Platform=Any CPU', 'Vapolia.MvvmCross.UserInteraction.sln'
& $msbuild $msbuildparams
cd nuspec

del *.nupkg

$version="1.0.2"
nuget pack "Vapolia.MvvmCross.UserInteraction.nuspec" -Version $version
nuget push "Vapolia.MvvmCross.UserInteraction*.nupkg" -Source $nugetServer

#####set assembly info to version
#####https://gist.github.com/derekgates/4678882

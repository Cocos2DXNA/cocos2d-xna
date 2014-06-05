# Cocos2D-XNA Build Structure

Herein resides the build scripts used to build the framework on a nightly basis. These scripts are designed to be used with NAnt.

Full Build
----------

There is no single full build script. Each platform is built separately. There is a windows build script (build.bat) that can build all of the platforms as a single command.

Platform Build
--------------

Each platform has its own build file, e.g. Windows.build. There is a related build script for each of the platform build files. These build scripts are windows command scripts, e.g. build-Windows8.bat. The purpose of these build scripts is to build the target platform from a windows command shell. 

Deploy of VSIX and NuGet
------------------------

The VSIX templates are build and packaged using the build-Templates.bat script, which uses the deployments.build build file. The deployments.build file also contains the commands to build the NuGet packages. 

Versioning
----------

### VSIX

In the ProjectTemplates directory you will find the StarterKits for the VSIX. These projects are used to deploy the extensions that Visual Studio can download. In the VSIX part of the template source you will find a .vsixmanifest file. That file contains a version identifier for the template, which must be updated when the templates are uploaded to the Visual Studio gallery.

### NuGet Packages

The version number of the NuGet packages is referenced in the VSIX project files. Make sure that when you update the NuGet package version you also update the VSIX package reference version, and subsequently upload new VSIX to Visual Studio Gallery. The master NuGet version control is in the platform build files. In there you will see the ".X.Y.Z.nupkg" extension on the NuGet package. That determines the version number that is uploaded to the nuget repository. The nuspec files control the actual build version stamped on the NuGet package.



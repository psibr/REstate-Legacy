//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDirs = GetDirectories("./src/REstate.Services.*/bin/" + configuration);
var binDirs = GetDirectories("./src/REstate.Services.*/bin");
var rootDir = Directory("./src");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
	{
		CleanDirectories(binDirs);
	});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
	{
		NuGetRestore("./src/REstate.sln");
	});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
	{
		if(IsRunningOnWindows())
		{
		  // Use MSBuild
		  MSBuild("./src/REstate.sln", settings =>
			settings.SetConfiguration(configuration));
		}
		else
		{
		  // Use XBuild
		  XBuild("./src/REstate.sln", settings =>
			settings.SetConfiguration(configuration));
		}
	});

Task("Create-Artifacts")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var files = GetFiles("./*")
			+ GetFiles("./src/connectors/**/*")
			+ GetFiles("./tools/nuget.exe")
			+ GetFiles("./src/REstateConfig.json")
			+ GetFiles("./src/REstate.Service*/**/*");
		
		Zip("./", "REstate.zip", files);
	});
	
Task("AppVeyor")
	.IsDependentOn("Create-Artifacts")
	.Does(() =>
	{
		AppVeyor.UploadArtifact("./REstate.zip");
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

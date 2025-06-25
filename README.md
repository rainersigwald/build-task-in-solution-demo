# Building and using a task assembly in the same build

It’s often useful to create a custom task for your use, but consuming it presents some complexity.

## Task-deployment options

### NuGet package

The most common use of custom tasks is via NuGet package. You can package and publish a `.nupkg` to a NuGet feed from another repo or your primary repo and then reference it via the usual NuGet approaches.

This is the best way to produce a task for _others_ to consume. However, if your repo is the only consumer of a task this can be inconvenient:

* It requires a separate build/publish process.
* It’s hard to incrementally test the task (you have to produce a package and push or override the package in your NuGet cache).
* If you go too long between task modifications, you might forget how to publish it.

### Inline tasks

https://docs.microsoft.com/visualstudio/msbuild/msbuild-inline-tasks

These are convenient and easy to author, but hard to debug, can be inefficient, and can be difficult to configure to use custom references (for instance if you need the task to call into one of your own libraries).

### Task assemblies built in the same solution

You can also build a task assembly as a regular assembly in your solution. This is slightly harder to use (a simple NuGet reference no longer suffices), but gets you the benefits of a compiled task assembly (speed, debuggability, ease of reference) while avoiding the difficulty of NuGet packages (packaging step, reference updates).

`UsingTask` elements are evaluated before targets start executing in a project. Because of that, you cannot use a `ProjectReference` to get the path to a task assembly that is the output of another project. Instead, you must reference the task in two steps:

1. Include a `UsingTask` element.
   1. Use properties to construct an `AssemblyFile` attribute that points to the output of the task project.
   2. Specify `TaskFactory="TaskHostFactory"` to force MSBuild to load a new copy of the assembly in a new process for every build, allowing the task assembly to be updated after a build that uses it.
2. Include a `ProjectReference` to ensure that the task assembly is built before it is used.
   1. Set `ReferenceOutputAssembly="false"` because the task assembly only needs to be built, not referenced by the calling project.
   2. TODO: multitargeting compatibility (`SetTargetFramework` conditional on `$(MSBuildRuntimeType)`?)

While `UsingTask` elements are considered before running tasks, the assembly file referenced in the `UsingTask` is not loaded until the first use of a task within it, so it's ok for the task assembly to not yet be built when the `UsingTask` is initially evaluated.

If your task will be used in more than one project, define the `UsingTask`, the `Target` that uses it, and the `ProjectReference` in a `.targets` file and import it in the projects where you will use it.

By default, MSBuild uses normal .NET Framework assembly load behavior, which causes the task assembly DLL to be loaded into an `MSBuild.exe` process that may be long-lived. That can cause failures in subsequent builds, because the task assembly can't be overwritten. Fortunately, MSBuild can be configured to [run a task in its own short-lived process](https://docs.microsoft.com/visualstudio/msbuild/how-to-configure-targets-and-tasks), and you should use that option for tasks built within the same solution.

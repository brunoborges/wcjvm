# Windows Container JVM Launcher

This project is a proof of concept for a JVM launcher inside Windows Container.

## Why

The OpenJDK HotSpot JVM is not Windows Container aware. For this reason, the JVM will thus not be able to identify the amount of CPU and Memory given to the container. This will result in the JVM ergonomically sizing itself based on the total amount of CPU and memory of the entire Windows host. Often, this translates into a JVM crash due to insufficient memory.

Introducing... `wcjvm`.

## wcjvm

The `wcjvm`` command identifies the amount of CPU and Memory given to the container and will start the JVM with the appropriate parameters.

In essence, `wcjvm` is a wrapper (or a shim) of the `java` command, specifically for launching the JVM from within a Windows container.

## Build

`wcjvm` is a Windows native executable. It is written in .NET 7.0 and compiled with the .NET SDK. To build the project, make sure you have .NET SDK 7.0 installed. Then, run the following command:

```bash
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true
```

The above command will produce a single executable file `wcjvm.exe` in the `out` folder as per the `wcjvm.csproj` configuration file.

## Usage

Bundle `wcjvm` along with the JDK of your choice into a Windows container image. You can drop the `wcjvm.exe` command into the `bin` folder of your JDK. Then, use `wcjvm` to launch the JVM, instead of `java`.

Make sure your JDK of choice is properly configured in your system path as in:

```bash
JAVA_HOME="C:\Program Files\Java\jdk-17"
PATH="%JAVA_HOME%\bin;%PATH%;%WCJVM_OUT%"
```

Then, just launch the JVM:

```PowerShell
wcjvm -jar myapp.jar
```

## How it works

`wcjvm` uses a combination of Windows API and .NET API to identify the amount of CPU and Memory given to the container. It then parses the JVM command line arguments, and adjust any and every necessary JVM flag to ensure it fits in the Windows container. 

Primarily, it will set `-XX:ActiveProcessorCount=N` and `-XX:MaxRAM=NNN` to the appropriate values. This will right-size the JVM in terms of processor counting (and therefore GC threads, ForkJoinPool, and any other 3rd-party framework that needs this value), as well as the heap size.

It will also adjust other flags such as the GC selection. This one happens particularly because HotSpotJVM's ergonomic GC selection reads from `os::physical_memory` which does not reflect any value given to `MaxRAM`, and it is not Windows container-aware.

## Known issues

The `OperatingSystemMXBean` will still report `totalMemory` as the total amount of memory of the Windows host. This is a known issue and it is not possible to fix it. However, this is not a problem because the JVM will not use more memory than the one given to the container thanks to the JVM flags set by `wcjvm`.
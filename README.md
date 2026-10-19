# Hearthstone Community + Custom Patcher
An application that automates patching Hearthstone using the [HearthstoneAccess Community Patch](https://www.hearthstoneaccess.com/)

Last Updated: Wednesday, December 3, 2025

## Overview
This app (referred to as the patcher) performs the following tasks:
* Downloads the selected official HearthstoneAccess Community Patch
* Installs the official patch first
* Downloads and installs the newest custom overlay from this repository's GitHub Releases second
* Prompts if you want the official changelog put on your desktop

The ordering is intentional: the custom overlay replaces only the files it needs after the public patch has provided its baseline. The app always downloads `CustomPatch.zip` from the **latest GitHub Release**, so users get new custom patches without downloading a new patcher executable.

## Publishing a custom-patch update

1. Create a new GitHub Release in this repository.
2. Upload the new overlay archive as a release asset named exactly **`CustomPatch.zip`**.
3. Publish the release.

The next time a user runs the existing patcher, it downloads that release asset automatically. Only publish an archive whose `hsa_manifest.json` declares the same Hearthstone version as the current Community Patch. The patcher rejects mismatched versions before applying the custom overlay.

There are two general target audiences: those who wish to simply use the patcher, and those who wish to develop/build the patcher.

## Using the Patcher
To use the patcher, download the latest executable from the [latest release page](https://github.com/leibylucw/hearthstone-access-patcher/releases/latest) and run like any other program.

## Developing the Patcher
### Install System Dependencies
Make sure you have the following dependencies installed and available on your system:
* [Git](https://git-scm.com/): any recent version
* [GitHub CLI](https://cli.github.com/) (optional): any recent version
* .NET 8 SDK, either from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download) or install using Visual Studio.

### Clone the Repo
With Git:

```shell
git clone https://github.com/leibylucw/hearthstone-access-patcher.git
cd hearthstone-access-patcher
```

Or with gh:

```shell
gh repo clone leibylucw/hearthstone-access-patcher
cd hearthstone-access-patcher
```

### Create the distributable

From the repository root, build the single-file Windows executable:

```shell
dotnet publish -c Release
```

Distribute `bin\Release\net8.0-windows\win-x86\publish\HearthstoneAccessPatcher.exe`. It is self-contained, so recipients do not need to install .NET.

### Common Development Tasks:

- **Running the Program:**
  To quickly run your program during development, type `dotnet run` in the terminal. This command compiles and runs the program in **debug mode**, meaning it prioritizes fast compilation times without any optimization. It's useful for testing and debugging but not ideal for final performance tests or releases.

- **Building the Program:**
  If you want to compile the program without running it, use `dotnet build`. By default, this also builds in **debug mode**, where the focus is on fast feedback and ease of debugging. Optimizations are turned off, so the build is quick, but not the most efficient. Use this primarily during development when you need to verify that the program compiles correctly.

- **Publishing the Program:**
  For a version of the program that’s ready to distribute or deploy, use `dotnet publish`. This builds the program in **release mode**, enabling optimizations that improve performance and reduce the final file size. The output is typically placed in a folder like `bin\Release\net8.0-windows\win-x86\publish`. The exact path might vary based on your runtime and platform, but it will always follow a similar structure.

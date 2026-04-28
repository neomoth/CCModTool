# CCModTool

### !!! This repository is a work in progress! It is not feature complete and probably buggy. !!!

CCModTool is a utlity for assisting with the installation and management of CCModLoader to make getting into CrossCode
modding simpler and more efficient.

The utility is also capable of managing the installed NWJS version, applying fixes for common minor bugs, and has more
planned features that will be added at a later date.

## Installation
N/A

## Compiling
Open a terminial and run the following commands. You must have git/gitbash installed as well as the .NET10 SDK.
```shell
$ git clone https://github.com/neomoth/CCModTool
$ cd CCModTool
$ dotnet restore
$ dotnet run --project CCModTool.UI
```
This will compile and open the program.

## TODO
- Fix up issues with configuration menu.
- Clean up and refine AI slopcode in UI and analyzers/generators.
- Split up generators and analyzers.
- Find more reasons to justify ripping RT's dependency injection.
- Begin work on downloader module.
- Begin work on game file management/modification.
- Maybe figure out how the FUCK to use the proper cursor for resizing things diagonally n such in Avalonia.
- Bugfixing.

## Planned Features
- Mod downloading/management from mod repository.
- Add more options for auto-applying fixes.
- Adding more planned features IDK im out of ideas.
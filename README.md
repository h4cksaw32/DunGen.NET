# DunGen.NET
Also available on [itch.io](https://myanchnl.itch.io/dungen-net)
## About
DunGen.NET is a dungeon map generator library and app written using C# and Avalonia. It is meant for game developers that need some maps for their games. The library allows the options of having maps generated directly in projects.

## System Requirements
Desktop system (Windows, MacOS, Linux) with .NET Core 9+ installed.

## App
The app contains a GUI for generating maps, as well as adjusting individual tiles and visualizing with textures.
### How to use
1) In the releases tab, download the folder WITHOUT "library" in its name.
2) Run "DunGenApp.exe" from the folder.
### Content
The app contains a map generator, a tile editor, and a texture manager.
### Files
Generator settings are saved in a .json format, providing easy editing in a text editor.  
For information on the file format of the map and textures, see [Library: File Format](#file-format).

## Library
The library contains classes that can be used to generate maps. Besides being used in the app, it can be used in any C# project.
### How to use
1) In the releases tab, download the folder with "library" in its name.
2) Within the folder, use "DunGenLib.dll" as a reference in your project.
### About
This library contains a generator that uses a room-placement algorithm to create maps.
### File format
The map file contains the following:
1) Width (2 bytes, little endian)
2) Height (2 bytes, little endian)
3) Tile data (all the bytes in the tile array)  

The texture manager writes each image as a .bmp file, located in the same folder as the map file.
Each image is named "##.bmp", where "##" is the tile ID in hexadecimal (lowercase).

## Contributing
Developers are welcome to develop their own features if I don't update the project for a long time.
### Branches
- `main`: THe development branch, where frameworks for new features are added.
- `release`: The release branch, where only code for released features are kept.
### Recommended tools
- .NET 9.0 SDK or later
- Visual Studio 2022 or later
    - Avalonia extension installed
### Issues & Requests
Bug reports and suggestions are greatly appreciated, and are highly encouraged to be submitted to the issues tab.  
If you fix a bug or add a new feature to your fork and want to see it in the main project, you want to see in the main project, feel free to make a pull request.

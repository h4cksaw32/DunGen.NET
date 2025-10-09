# DunGen.NET

## About
DunGen.NET is a dungeon generator library and app written using C# and Avalonia. It is meant for game developers that need some maps for their games. The library allows the options of having maps generated directly in projects.

## App
The app contains a GUI for generating maps, as well as adjusting individual tiles and visualizing with textures.
### How to use
1) In the releases tab, download the folder WITHOUT "library" in its name.
2) Run "DunGenApp.exe" from the folder.
### Generator Options
### Textures
### Map Editor
### File format

## Library
The library contains classes that can be used to generate maps. Besides being used in the app, it can be used in any C# project.
### How to use
1) In the releases tab, download the folder with "library" in its name.
2) Within the folder, use "DunGenLib.dll" as a reference in your project.
### Map
The map is represented by an array of bytes, where each byte represents a tile and its ID.  
It has various methods for resizing, getting and setting tiles, inserting tiles, placing rectangles, etc.
### Generator
This generator mainly uses room placement. It generates rooms, paths, and bodies of liquid with many settings that can be tweaked.
### Textures
There is a "bonus" class that maps tiles IDs to bitmaps. It is meant for use in other projects to facilitate the allocation of textures.
### File format
The map file contains the following:
1) Width (2 bytes, little endian)
2) Height (2 bytes, little endian)
3) Tile data (all the bytes in the tile array)

## Contributing
### Branches
### Recommended tools
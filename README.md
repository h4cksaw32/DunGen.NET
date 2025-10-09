# DunGen.NET

## About
DunGen.NET is a dungeon generator library and app written using C# and Avalonia. It is meant for game developers that need some maps for their games. The library allows the options of having maps generated directly in projects.

## App
The app contains a GUI for generating maps, as well as adjusting individual tiles and visualizing with textures.
### How to use
1) In the releases tab, download the folder WITHOUT "library" in its name.
2) Run "DunGenApp.exe" from the folder.
### Generator Options
There are many options to tweak the generation of the map, including individual settings for each tile type.  
After adjusting the settings, press "Generate" at the bottom to generate the map, and "Edit map" to open the map editor.  
The top menu has options for saving and loading settings, resetting to default, and opening the texture selector.
### Texture Select
This window allows you to select textures for each tile type to be displayed in the map editor.
### Map Editor
The top menu has options for saving and loading maps, as well as clearing it entirely.  
Below that is the tile selector, where you can choose which tile to place.  
"View width" changes how many tiles are shown (horizontally and vertically) in the editing area below.  
The editing area is where you can place tiles, simply by clicking on the desired tile.  
To shift the editing area to a different part of the map, use the surrounding buttons.  
A toggle is also available to view the layout of the entire map.
### Files
Generator settings are saved in a .json format, providing easy editing in a text editor.  
For information on the file format of the map and textures, see [Library: File Format](#file-format).

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

The texture manager writes each image as a .bmp file, located in the same folder as the map file.
Each image is named "##.bmp", where "##" is the tile ID in hexadecimal (lowercase).

## Contributing
### Branches
### Recommended tools
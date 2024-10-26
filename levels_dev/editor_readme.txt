How to make a new or edit an old map.

--------------------------------------------------------------------------------
1. Choose a .tga or a .des -file in the open dialog.

For sample files of both types see the directory root. The produced files
 (.des and .map) should be placed in
 Windows: "C:\Users\*YOUR_USER*\AppData\LocalLow\TeamGalaxy\Galaxy Forces VR"
  to be playable from GFVR. If "MapEditor.exe" is placed in the same folder
  to get easy access to the custom maps it needs to be run as Admin.
 Linux and macOS: see the custom levels page in GFVR.

* tga file
 An image that shall contain a mask of the map. White becomes walls and black
  becomes space. The editor will make an approximation of the given tileset and
  the mask to create the map. This is the first step when building a new map.

 The generated map needs editing afterwards, but its better than starting with
  an empty map.

* des file
 It can be edited or will be outputted from the editor when a map is saved.

 It is a text file and must contain at least the following fields:
-START-
*MAPTYPE  RACE 3
*MAPDESC  "Some name/description"
*MAPSIZE  50 60
*MAPFILE  grid1.map
*TILESIZE 32
*TILESET  ts_alien.tga
-EOF-

row 1: "RACE" and the number of laps if the map contains checkpoints
       "MISSION" otherwise
row 2: A name displayed in GFVR when map is selected (keep it short)
row 3: The width and height of the map (in number of tiles)
row 4: The .map file containing the map grid (created if it does not exist)
row 5: Tile size of the tileset (32 for all built in tilesets)
row 6: The image file containing the tileset (must be .tga)

See below (3) for further explanation of the des file.

--------------------------------------------------------------------------------
2. When the editor is started you can do the following.

KEYBOARD
S        Save (.des and .map file)
L        Load (.des and .map file)
UP       Move the mapview up
DOWN     Move the mapview down
RIGHT    Move the mapview right
LEFT     Move the mapview left
PAGEUP   Move the mapview up fast
PAGEDOWN Move the mapview down fast
END      Move the mapview right fast
HOME     Move the mapview left fast
F1..F5   Change size of the "drawing tool"

W        Add waypoint to a selected enemy, or door button to selected door
O        Add object, this object is then the selected object
+,-      Change objecttype of the selected object
R        Rotate the selected object
X,SHIFTX Inc/dec xposition of the selected object (or waypoint)
Y,SHIFTY Inc/dec yposition of the selected object (or waypoint)
DEL      Delete the selected object (or waypoint)

1,2      Dec/inc number of selected checkpoint,
         or Dec/inc width of selected landing zone
         or Dec/inc width of selected door

SHIFT+PAGEUP   Crop map one row from top
SHIFT+PAGEDOWN Crop map one row from bottom
SHIFT+END      Crop map one column from right
SHIFT+HOME     Crop map one column from left

CTRL+PAGEUP    Extend map one row from top
CTRL+PAGEDOWN  Extend map one row from bottom
CTRL+END       Extend map one column from right
CTRL+HOME      Extend map one column from left

* Don't use enemy7 and Z-objects 2..3

MOUSE
* Click on an existing object to select it
* Click any of the tiles in the tileset down on the left to choose active tile
  (shown to the upper right, the "drawing tool")
* Hold the mouse button on the map or click on the map to draw the active tile

--------------------------------------------------------------------------------
3. Edit the .des file

The .des and .map files created by the editor shall work in the game as they
 are. But to change some things not supported by the editor, it is necessary to
 edit the .des file.

*TILESET ts_alien.tga
 There exists 7 built in tilesets currently:
  ts_alien.tga, ts_cave.tga, ts_cryptonite.tga,
  ts_evil.tga, ts_frost.tga, ts_lava.tga, ts_desert.tga
 External tilesets are not currently supported

*GRAVITY 0 70 //default if not set
 Gravity in X and Y, pixels per second^2

*RESISTANCE 0.68 0.43 //default if not set
 Resistance factor in X and Y. Higher value makes the ship slowdown faster. If 0
  the ship will keep going forever - like in space.

*LANDINGZONE   288  1148  3 CARGOBASE NOEXTRALIFE NOANTENNA NOWAREHOUSE 2 25 25
 The first two parameters are X and Y pos, next is the width in tiles, next either
 CARGOBASE or HOMEBASE, next NOEXTRALIFE/EXTRALIFE, NOANTENNA/ANTENNA,
 NOWAREHOUSE/WAREHOUSE, then the number of cargo packages and their weight (set 0
 for HOMEBASE)

Other attributes should be self explained.

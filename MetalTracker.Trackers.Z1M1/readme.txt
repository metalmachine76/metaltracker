MetalTracker for Z1M1 
Version 1.0


MENU
----

File
  New       -> Create a new tracking session.
               Allows you to define various Z1M1 flags. When the new session is created, 
               any currently tracked data will be reset.
  Open      -> Open a previously saved tracking session.
  Save      -> Save the current tracking session.
  Save As   -> Save current tracking session to a new file.
  Exit      -> Close the tracker application.

Session
  Flags     -> View/edit flags for current session.
               Allows you to view or change various Z1M1 flags. Changing a mirrored flag
               will mirror any tracked data. Changing other flags will not affect currently
               tracked data.
  Log       -> Session log will tell you where all major items and exits are located.
  Reset     -> Reset (clear) tracked data for current session.

Co-Op
  Connect   -> Open the co-op client window where you can connect to the co-op server. Once you are connected, you can close the window if desired.
  Rooms     -> View rooms opened on the co-op server, open a new room, or join an existing room. You must connect before you can view, open, or join rooms.
  Configure -> Set your player name and marker color for co-op. You must configure before you can connect.

Help
  View Help -> Opens the "readme.txt" file.
  About App -> Shows the "about" window.


MAIN UI
-------

Maps:

There are 11 maps you can select:

The overworld, the 9 dungeons, and Zebes.

Maps are divided into rooms, where each room is one game screen:

- If a room square is grayed out this means it is included in the shuffle (based on session flags).
- If a room square is not grayed out, and it contains a letter, this indicates the item slot class for
  the room, which can be useful for routing.
- Portal connections on the map are shown in green. 
- Non-portal connections (such as shops, take-any-road, etc) will be shown in white.
- Items will be shown on the map with icons.

Map interaction is currently mouse-based only:

- You can click and drag the maps which are larger than the visible area.
- Clicking a room will select it and show the details for that room in the "Room Detail" panel.
- Right-clicking applicable rooms will show a menu where you can choose an exit or destination to connect the room to.
  (this is not implemented for dungeons yet)
- Double-clicking a room will shade the room. Double-clicking again will remove the shade. Suggested use
  is to shade a room if you've collected the item there, explored the room, or don't care about it.


Room Detail:

Allows you to view and edit details for the room you clicked on the main map. Some rooms have nothing to track and won't show here. 


Inventory/Goals:

You can use this section to keep track of your current inventory, as far as major equipment and quest items. 


CO-OP SUPPORT
-------------

When you are connected to the co-op server and have joined a room, the following events will be synced from and to all players currently in the room:

- when an exit is marked
- when an item is marked

Note: The co-op server does not keep any state. It only replicates events to other players within a given room. This means that if you are disconnected 
from the room, and later reconnect, any events that occurred while you were not connected won't show on your tracker. 

A room is closed once all players leave the room.


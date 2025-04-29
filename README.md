# The ARchitects: Perspective Shifting

Final team project for 3D UI Design at Columbia University, Spring 2025

## Contents

- [Project Overview](#overview)
- [Recent Updates](#updates)
- [To Do List](#to-do-list)
- [Hand Gestures](#gestures)
- [Scripts](#scripts)

## Overview

The goal of this project is to investigate perspective shifting in a 3D VR
environment. It consists of two parts: A virtual tabletop at which participants
can join together and build structures from pre-defined models in miniature, and
an immersive 3D exploration of the completed tabletop scaled up to human size.
During the construction process, participants will have the opportunity to view
the scene in progress from the perspectives of other participants in order
to facilitate collaboration.

## Updates

```
4/28/25: Kyleigh - ARVRModeManager Improvements
```

- Fixed VR spawn point positioning and stability issues
- Added special physics handling for bench objects to prevent falling through the floor
- Improved scaling and positioning of objects during AR/VR mode transitions
- Adjusted player camera height in VR mode for better visibility and comfort
- Enhanced object restoration when switching back to AR mode
- Added robust error recovery for VR spawn points

```
4/28/25: Kyleigh - AR/VR Mode Switching
```

- Added ability to switch between AR tabletop view and immersive VR mode
- AR mode lets users build and interact with miniature structures on the tabletop
- VR mode scales up the environment to human size for immersive exploration
- Objects maintain their relative positions when switching between modes
- XR toggle button allows for switching modes using hand interactions

```
4/28/25: Dallas - Portals, New Assets, Test Scene, Gesture Revision
```

- Added positions around the table for 4 players
- Added marker objects for player start, teleport spawn, object spawn
- Numerical placards at each end of the table denoting players
- All four item ports functional with numerical buttons determining which player to transfer to
- New assets added for all 4 players - each player has unique set of 6 assets each
- Test scene designed
- Temporary secret position port menu behind each player to swap table positions (to be removed in final version)
- Revised hand gestures: Left hand now translates object, right hand rotates, grab remains the same
- Sound effect added for object rotation
- Replaced transformation hand colliders with collider on all fingers instead of just pointer
- Pointer collider on fingers remains for button presses

<p>
 <a href="https://www.youtube.com/watch?v=btehh4BpnYA">
  <img src="https://img.youtube.com/vi/btehh4BpnYA/0.jpg" width="500" 
  alt="City Scene"></a>
  <br>
  <em>City Scene, Portals, Player Placards</em>
</p>

```
4/22/25: Dallas - UI Object Selection
```

- Designed UI menu for player to choose and spawn building objects.
- Button activation on side of table to bring up menu.
- Object selection from menu spawns prefab on the table next to player.
- Transparent box as signifier for newly spawned object.
- Redesigned item transfer portals.
- Sound effects for button press, object selection, item portal transfer.
- Expanded table to accommodate 4 players.

<p>
 <a href="https://www.youtube.com/watch?v=PVe_PHboDSA">
  <img src="https://img.youtube.com/vi/PVe_PHboDSA/0.jpg" width="500" 
  alt="Object Selection Menu"></a>
  <br>
  <em>UI: Object Selection Menu</em>
</p>

```
4/21/25: Nathan - Photon Fusion Multiplayer and ParrelSync
```

- Set up initial Photon Fusion architecture for multiplayer.
- Players able to Host or Join a session using Fusion's network runner
- Players now spawn as networked prefabs and can see each other.
- Players can be cloned and tested via ParrelSync
- Integrated Photon Fusion to the main scene

```
4/21/25: Dallas - Predefined Assets
```

- Included free assets from Unity Store to the scene, scaled down to appropriate
  size.
- Scripts and colliders added to all assets to work with gestures.
<p>
 <a href="https://www.youtube.com/watch?v=6V5JzIdDubA">
  <img src="https://img.youtube.com/vi/6V5JzIdDubA/0.jpg" width="500" 
  alt="Custom Transformations"></a>
  <br>
  <em>Assets on Tabletop</em>
</p>

```
4/20/25: Dallas - Custom Translate/Rotate and Item Portals
```

- Translation and object rotation now functional.
- Move an object on the tabletop by pushing with the index finger in either the
  X or Z direction.
- Rotate an object by placing the index finger of either hand down into the top of
  the object and placing the index finger of the other hand anywhere on the object.
- The object will automatically rotate at a constant rate (45 degrees/sec) until
  one of the fingers is removed.
- Grab functionality revised/improved. Now uses raycasts from the hand. Pinch
  near an object to grab it. Upon release, it will quickly fall back onto the
  table in an aligned position by the force of gravity.
- Revised overall structure in Unity. Now using Open XR alone rather than in
  conjunction with Meta AIO.
- Device Simulator added and working.
<p>
 <a href="https://www.youtube.com/watch?v=Q_pKPHZifLI">
  <img src="https://img.youtube.com/vi/Q_pKPHZifLI/0.jpg" width="500" 
  alt="Custom Transformations"></a>
  <br>
  <em>Custom Translate/Rotate + Item Portals</em>
</p>

```
4/17/25: Dallas - Grab Functionality (Building Blocks)
```

- Simple grab functionality using Meta Building Blocks.
- Meta XR All-in-One SDK in conjunction with Open XR for hand gestures.
- Item portals for transfering objects between participants: Place an item in
  the portal on the side of the table to send it over to another participant.  
  It will appear hovering above their portal for 3 seconds to grab before it is
  automatically transferred onto the table next to the participant.

<p>
 <a href="https://www.youtube.com/watch?v=6XJnJ_h5r9A">
  <img src="https://img.youtube.com/vi/6XJnJ_h5r9A/0.jpg" width="500" alt="Item Transfer"></a>
  <br>
  <em>Item Portals</em>
</p>

```
4/15/25: Dallas - Basic Scene Setup
```

- Basic scene setup with tabletop and floor assets.
<p>
 <a href="https://www.youtube.com/watch?v=N58T2cYtn2g">
  <img src="https://img.youtube.com/vi/N58T2cYtn2g/0.jpg" width="500" alt="Grab Objects"></a>
  <br>
  <em>Grabbing Objects from the Tabletop</em>
</p>

## To Do List

```
Dallas
```

- [x] Custom transformation logic (4/20)
- [x] Prefab menu for building objects (4/22)
- [x] Prefab assets (4/21)
- [x] Assets for all four players (4/28)
- [x] Item portals for all four players (4/28)
- [x] Expand table, add more item portals w/position numbers (4/28)
- [x] Test scene (4/28)
- [ ] Overhead view switch to facilitate object placement in middle of table

Optional:

- [ ] Include controller input
- [ ] Item request feature
- [ ] Hand gestures to open/close object selection UI panels

```
Nathan
```

- [x] Photon Fusion Integration (4/21)
- [ ] Add authoritative interaction logic for scene objects
- [ ] Set up spawn points to support up to 4 players
- [ ] Ownership transfer of objects

## Gestures

The following hand gestures are currently available:

```
Grab

  To grab an object on the table and lift it away, simply pinch the index finger and
  the thumb together on the object.  The grab function uses raycasting from the
  hand to select the correct object.

  The hand visual prefabs will glow red on the thumb and index finger when you are
  near a grabbable object, indicating that you can pinch to pick it up.  When it is
  picked up, the thumb and index finger will turn blue.

  A grabbed item can be released by releasing the pinch gesture.  Once released,
  the object is subject to gravity and will fall back to the nearest surface (or
  away from the scene if thrown).
```

```
Move Object (*revised*)

    To move/translate an object with the hand, place any of your fingers on
    the left hand on the object and then move your hand.  As long as your
    left hand remains on the object, the object will follow the hand.  Lift
    up to stop translating.
```

```
Rotate Object (*revised*)

  To rotate an object, place any of your fingers of the right hand on the
  object. The object will begin to automatically rotate at a constant rate
  (45 deg/sec).

  As long as any part of the right hand is touching the object, it will
  continue to rotate. Once the desired rotation has been achieved, simply
  remove the right hand from the object to stop rotating.
```

```
Transfer Object

  To transfer an object to a player on the other side of the table, grab
  it and release it into the portal next to you on your side of the table.
  It will automatically transfer over to the other player's portal and
  hover above it for 3 seconds.

  The other player can either grab it or wait for 3 seconds, after which
  it will automatically travel from the portal to a position on the table
  right next to the player.  If an object is already there, it will stack
  on top of it or push it out of the way depending on the geometry.

  You must select which player number you wish to transfer an object to
  using the buttons on top of the portal before placing the object inside
  the portal.
```

## Scripts

The following scripts are currently available:

### AR Tabletop Scripts

```
ARVRModeManager.cs:

  This script manages the transition between AR tabletop view and immersive VR mode.
  In AR mode, users can build and manipulate miniature structures on the tabletop.
  In VR mode, the environment is scaled up to human size for immersive exploration.

  The manager handles:
  - Scaling and repositioning objects when switching modes
  - Preserving object relationships and positions during transitions
  - Managing physics properties appropriately for each mode
  - Storing the last known positions in AR mode for seamless return
  - Adjusting camera settings and skybox for each mode
```

```
XRModeToggleButton.cs:

  This script enables hand interaction with the AR/VR mode toggle button.
  When an index finger collider touches the button, it triggers a mode switch
  via the ARVRModeManager. Includes a cooldown timer to prevent accidental
  double-presses.
```

```
GrabPushRotate.cs:

  This script is used to translate and rotate buildable objects.  Place it on any
  prefab/object that will be used on the tabletop in order to move and rotate the
  object with the hands.

  It should automatically add the necessary components, but if not, the object
  must have a rigidbody (non-kinematic), a collider (non-trigger), and a child
  that has another collider (trigger).  The first collider is for transformations,
  the second trigger child collider is to be able to grab the object with the
  XR Grab Interactable.

```

```
HoverPortal.cs

  A simple script that animates item transfer portals in a slow up/down
  ping-pong vertical motion.

```

```
OpenObjectMenu.cs

  This is used for opening up the main object creation/spawn menu selection UI.
  It is placed on the cylindrical "button" child object of the ToggleObjectMenuButton
  GameObject and listens for triggers from the index fingers.  When the button is
  pressed, the Object Menu with all of its prefabs will become active.  When pressed
  a second time, it will become inactive.

  The Object Menu will also be able to brought up via hand gestures in the future,
  not only through this button press.  Raising the palm in an upward or downward
  direction will open or close the menu, respectively.

  The OpenObjectMenu script has several public variables that must be set in the
  inspector, including a reference to the ObjectMenu itself, as well as sound
  files for the button press and object select.
```

```
PortToPosition.cs (*new*)

  Allows teleportation from one table position to another using an in-game UI menu
  for location selection.  This feature is for testing and will be removed in the final
  version, along with this script.
```

```
PositionPlayer.cs

  This is placed on the XR Origin Hands rig to start the player's camera off in a
  particular location.  Assign the XR rig itself and an empty GameObject positioned
  at the desired start location to the serialized variables in the inspector.
```

```
RotateObject.cs

  A simple rotator script.  Slowly rotates a given GameObject continuously
  at a constant rate.
```

```
SelectNewObject.cs

  This script is responsible for spawning a new object when the player selects
  it from the Object Menu.  It is placed on each prefab child in the Object Menu
  and listens for triggers from the index fingers.  Upon making a selection,
  the object's prefab will be instantiated on the table in front of the player,
  as well as a transparent rotating box surrounding it to differentiate it from
  other objects on the table.  Once it is moved, the transparent box will
  disappear.

  This script disables the Object Menu when a new object is selected, so in order
  to be able to play the sound effect associated with an item select and to remove
  the transparent box once the object is moved, we need to pass those variables
  back to OpenObjectMenu.cs, where it is listening in its Update function for a new
  item spawn.

  This has several public variables to assign in the inspector, including a
  reference to the OpenObjectMenu script on our button, the transparent material
  to be used for the container box, the GameObject transform representing where
  on the table the new object should be spawned at, a reference to the actual
  prefab in assets that will be spawned, as well as the audio clip for selecting
  an object.
```

```
SelectRemotePortal.cs (*new*)

  This is used in conjunction with the in-game UI menu for item portals to transfer
  items between players.  It allows the player to select which table position (1,2,3,4)
  the item is to be transferred to via the portal before it is transferred.  Once a
  selection has been made, the item will travel to the other portal at the selected
  destination.
```

```
SnapOnRelease.cs (*deprecated*)

  This script previously snapped an object to the table upon being released from
  a grab.  It is no longer used in conjunction with GrabPushRotate.cs and can
  be ignored.
```

```
TransferItem.cs

  This script allows for the use of item portals to transfer items from one position
  of the table to another.  It should be placed on all ItemPort prefabs (already done)
  on the Opening child object (not on the port object itself).

  Make sure you assign the following serialized variables in the inspector:
    Portal 1 - item portal player 1 game object
    Portal 2 - item portal player 2 game object
    Portal 3 - item portal player 3 game object
    Portal 4 - item portal player 4 game object
    Table Drop Point 1 - marker game object denoting the location on the table where
      the object will land after coming out of the portal for player 1.
    Table Drop Point 2 - marker game object denoting the location on the table where
      the object will land after coming out of the portal for player 2.
    Table Drop Point 3 - marker game object denoting the location on the table where
      the object will land after coming out of the portal for player 3.
    Table Drop Point 4 - marker game object denoting the location on the table where
      the object will land after coming out of the portal for player 4.
```

### Networking Scripts

```
BasicSpawner.cs

  Sets up multiplayer spawner with Fusion. Players can host or join a session and
  are spawned at the different markers in the scene. Script handles player
  spawning, despawning and handling of non-authoritative players
```

```
Player.cs

  Allows player to move character based on input data.
```

```
NetworkVR.cs

  Syncs position and rotation of player's head, left controller, and right
  controller across the network. Uses state authority and updates the network with
  transforms of headset and controller. If player doesn't have authority, it reads
  and applies to local scene player. This allows for live movement of users.
```

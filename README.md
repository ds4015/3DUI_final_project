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
5/09/25: Nathan - Changed multiplayer mode to shared + fixed object sync for connecting players
```
- Changed multiplayer from auto host/client to shared mode
- Players are now able to spawn items and items are visible amongst all players
- Able to handle 4 player multiplayer
- Will implement to main scene

```
5/08/25: Nathan - Multiplayer Object Sync and added Main scene to Multiplayer scene
```

- Added main scene to multiplayer
- Players are spawning in correct places + objects are synced in transform amongst all players
- Currently facing issue where player 2 is unable to spawn items perhaps due to Host-Client behavior

```
5/09/25: Dallas - Finger Movement (VR Mode)
```
- Ability to move through VR immersive scene using hand tracking
- Point index finger of left hand straight forward to move through the scene
- To stop moving, lower the finger or raise it vertically
- To change direction, rotate head or body with headset and continue pointing forward
- Forward movement is in relation to camera/headset position
- NOTE: Objects in VR mode currently collide with player with movement - must be
made non-triggerable

<a href="https://youtu.be/oxrz8zAX6A8">
  <img src="https://img.youtube.com/vi/oxrz8zAX6A8/maxresdefault.jpg" width="500" 
  alt="Finger Movement"></a>
  <br>
  <em>Finger Movement in VR Mode</em>
</p>

```
5/08/25: Nathan - Multiplayer Movement Synced
```

- Successfully added multiplayer into the project
- Movement of hands and headset in sync with all player POV -- including transforms
- Incorporated a hardware rig and a network rig
- Hardware rig feeds data into network rig for network transform updates
- Will add implementation to main scene


```
5/08/25: Kyleigh - Audio Feedback for Perspective Switching
```

- Added audio feedback when switching between perspectives and pressing wrist buttons
- Added volume control for perspective switching audio
- Integrated audio source handling within the PerspectiveSwitcher component
- Enhanced user experience with auditory cues to reinforce visual perspective changes


```
5/06/25: Kyleigh - Perspective Switching System
```

- Implemented collaborative perspective switching to view the table from another player's viewpoint
- Players can now easily switch to see the table from any registered player position
- UI panel displays player perspective options with intuitive button controls
- Table objects rotate as a single unit to maintain spatial relationships
- Reset button allows immediate return to original perspective
- Automatic UI switching between perspective selection and reset buttons
- Proper restoration of all object positions and orientations when returning to original view

```
5/04/25: Nathan - Tab Switching + Request Feature
```

- Assets of other players available for request
- Local player can request the asset of another player
- UI pops up where a preview of the item they are looking to request is shown. Player can either send out the request or remove the request
- UI Queue is shown to the requested player of the items the local player is looking for
- Will continue to implement for all other players and items
- Will look to improve design of queue and request feature

<a href="https://youtu.be/_I5-_o1JqfkI">
  <img src="https://img.youtube.com/vi/_I5-_o1JqfkI/maxresdefault.jpg" width="500" 
  alt="Request Feature and Queue"></a>
  <br>
  <em>Request Feature and Item Queue</em>
</p>

```
5/04/25: Kyleigh - AR/VR Button Wrist Placement
```

- Moved AR/VR mode toggle button to the user's wrist for easy access
- Button now follows the user's wrist using world space UI
- Added smooth position and rotation tracking to ensure UI follows wrist naturally
- Button maintains proper positioning and visibility as user moves
- Button is continuously visible on the wrist for convenient mode switching (issues with raise-to-view functionality)
- issue with VR spawnpoint capsule: size is too large when returning to AR mode after VR

```
5/04/25: Dallas - Overhead View and Object Selection
```

- Overhead view mode activated with bell on side of table
- Gives top-down view of player's quadrant of the table and allows object
  manipulation
- Newly spawned items will automatically transition to overhead view for
  placement.
- Object selection by double tapping with index finger - object must be
  selected before translating/rotating
- Outline visible upon selection in normal tabletop view mode
- Overhead view currently only implemented for player position 1, remaining
  to come

 <a href="https://www.youtube.com/watch?v=4rBbh7R9izI">
  <img src="https://img.youtube.com/vi/4rBbh7R9izI/0.jpg" width="500" 
  alt="City Scene"></a>
  <br>
  <em>Overhead View, Object Selection</em>
</p>

```
5/04/25: Nathan - Tab Switching
```

- Implemented tab switching of assets
- Players can switch between pages to see the assets of other players
- Will improve UI of tab switching + integrate for all remaning players
- Queue to be created

```
5/01/25: Kyleigh - UI Tracking Improvements
```

- Implemented smooth movement tracking for UI elements
- Added configurable position and rotation interpolation
- UI elements now follow player with natural delay similar to Meta Horizon buttons
- Improved UI positioning stability when player moves quickly
- Added customizable smoothing parameters for position and rotation

```
4/29/25: Nathan - Multiplayer Integration
```

- Continued testing multiplayer object synchronization with Fusion
- Implemented network object for visibility across players
- Verified that objects can spawn and are visible to player
- Currently facing issues with object perspectives - objects appear in different locations based on user viewpoint

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
4/27/25: Nathan - Auto Hosting and Client Join
```

- Added automatic host/client
- First player becomes host, other players auto-join as clients
- Deleted GUI for smoother experience on headset

```
4/24/25: Nathan - Alternative Multiplayer Testing
```

- Tested other networking options (Netcode for GameObjects, Photon PUN)
- Integration of Netcode and PUN unsuccessful for current project
- PUN does not have headless hosting feature for VR integration

```
4/23/25: Nathan - Transform Bug Multiplayer Fix
```

- Fixed issue where moving one player leads to another moving
- Issue occured due to both players running the project in the same machine despite two Uniy editors

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
- [x] Overhead view switch to facilitate object placement in middle of table (p1) (5/4)
- [ ] Overhead view for remaining player positions
- [x] Finger movement in VR Mode (5/9)

Optional:
- [ ] Hand gestures to open/close object selection UI panels

```
Nathan
```

- [x] Photon Fusion Integration (4/21)
- [ ] Add authoritative interaction logic for scene objects
- [ ] Set up spawn points to support up to 4 players
- [ ] Ownership transfer of objects
- [x] Item request/queue feature

```
Kyleigh
```

- [x] AR/VR mode switching (4/28)
- [x] UI tracking and positioning improvements (5/1)
- [x] Perspective switching system (5/6)

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
Move Object

    To move/translate an object with the hand, place any of your fingers on
    the left hand on the object and then move your hand.  As long as your
    left hand remains on the object, the object will follow the hand.  Lift
    up to stop translating.
```

```
Move Player (*new*)

    To move the player in the immersive VR mode scene, point the left index
    finger straight forward.  The player will walk forward as long as the
    index finger is pointing straight ahead.  To stop moving, either raise
    or lower the index finger vertically.

    To rotate in VR Mode, simply turn your head/body in the direction you
    wish to move and then point the index finger forward to move in that
    direction.  All movement is in relation to the position of the camera/
    headset.
```

```
Rotate Object

  To rotate an object, place any of your fingers of the right hand on the
  object. The object will begin to automatically rotate at a constant rate
  (45 deg/sec).

  As long as any part of the right hand is touching the object, it will
  continue to rotate. Once the desired rotation has been achieved, simply
  remove the right hand from the object to stop rotating.
```

```
Select Object

  Tap twice on an object with the left index finger to select it for
  translation and rotation.  In normal view mode, selected objects will
  appear with a white outline around them.
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

```
Switch Perspective (*new*)

  To view the table from another player's perspective, press the perspective
  switch button to open a panel with player options. Select a player perspective
  button to rotate the table and all objects to match that player's viewpoint.

  To return to your original perspective, press the reset button that appears
  after switching. This will restore all objects to their original positions
  and orientations.
```

## Scripts

The following scripts are currently available:

### AR Tabletop Scripts

```
BellAnimation.cs

  A script for giving an animation to the bell prefab positioned both on the
  side of the tabletop at each player position aas well as in the center of the
  overhead view.  The bell when touched will animate with this script and play
  a sound to signal a transition between normal viewing mode and overhead
  mode.
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
OverheadSwap.cs *new*

  Used for cloning tabletop objects onto the overhead view tabletop.  All objects
  existing on the tabletop as well as newly instantiated objects will be cloned
  using a pool for optimization on the overhead view tabletop.  Any changes made
  to position and rotation will be mirrored/transferred back to the original items
  on the original tabletop upon transitioning back to normal viewing mode with the bell.
```

```
PanelOpenButton.cs *new*

  A script for buttons that show/hide panels when pressed with a finger in AR.
  Used for the perspective switching panel and other UI elements in the scene.
  Includes visual feedback when buttons are hovered and pressed, cooldown
  timing to prevent accidental double-presses, and integration with the
  PerspectiveUIManager for controlling panel visibility.
```

```
PerspectiveButton.cs *new*

  This script is used for buttons that switch to a specific player's perspective.
  When pressed, it tells the PerspectiveSwitcher which player's perspective to show.
  The button displays the player's name, highlights when touched, and ensures
  proper UI state changes after being pressed (hiding the panel and showing the
  reset button).
```

```
PerspectiveSwitcher.cs *new*

  The main controller for the perspective switching system. This script manages
  rotating the table and all its objects to show different player perspectives.
  It stores the original positions and rotations of all objects and can reset
  everything precisely to its original state. Also manages which UI buttons should
  be visible based on the current perspective state.

  The script includes audio feedback features:
  - Plays configurable sounds when switching perspectives
  - Plays distinct sounds when resetting to original view
  - Automatically manages an AudioSource component
  - Allows volume adjustment through the Inspector
```

```
PerspectiveUIManager.cs *new*

  Manages the UI panels related to perspective switching. Controls showing and
  hiding the panel containing player perspective buttons. Works alongside the
  PerspectiveSwitcher to ensure proper UI flow when switching perspectives.
```

```
PortToPosition.cs

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
ResetPerspectiveButton.cs *new*

  A script for the button that resets the table and objects to their original perspective.
  When pressed, it calls the PerspectiveSwitcher to restore everything to its original
  position and rotation. The button automatically appears after changing perspective and
  hides itself after being pressed, showing the perspective switch button again.
```

```
RotateObject.cs

  A simple rotator script.  Slowly rotates a given GameObject continuously
  at a constant rate.
```

```
ScreenFader.cs

  A simple script that uses a canvas to fade in/fade out transition between normal
  tabletop view and overhead view.
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
SelectRemotePortal.cs

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

### AR/VR Immersion

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
MovePlayer.cs (*new*)

  A basic script for detecting hand gestures in VR Mode.  If the index finger of the
  left hand is horizontal and pointed forward, the camera/XR origin will move forward
  in the current direction the player is facing.  If the index finger is raised or
  lowered vertically, movement will be halted.
```

```
UISetup.cs:

  This script handles the positioning and movement of UI elements that follow the player.
  It uses smooth interpolation techniques to create natural-feeling UI movement with a
  subtle delay that improves usability.

  The script features:
  - Configurable position and rotation offsets from the player's viewpoint
  - Smooth position tracking using Vector3.SmoothDamp for natural following behavior
  - Custom quaternion rotation interpolation for smooth rotational movement
  - Adjustable smoothing parameters to fine-tune the UI responsiveness
  - World space UI positioning that maintains optimal viewing angles
```

```
XRModeToggleButton.cs:

  This script enables hand interaction with the AR/VR mode toggle button.
  When an index finger collider touches the button, it triggers a mode switch
  via the ARVRModeManager. Includes a cooldown timer to prevent accidental
  double-presses.
```

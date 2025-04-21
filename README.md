# The ARchitects: Perspective Shifting
Final team project for 3D UI Design at Columbia University, Spring 2025

## Overview
The goal of this project is to investigate perspective shifting in a 3D VR
environment.  It consists of two parts:  A virtual tabletop at which participants
can join together and build structures from pre-defined models in miniature, and
an immersive 3D exploration of the completed tabletop scaled up to human size.
During the construction process, participants will have the opportunity to view
the scene in progress from the perspectives of other participants in order
to facilitate collaboration.

## Updates
```
4/21/25: Nathan - Photon Fusion Multiplayer and ParrelSync
```
- Set up initial Photon Fusion architecture for multiplayer.
- Players able to Host or Join a session using Fusion's network runner  
- Players now spawn as networked prefabs and can see each other.
- Players can be cloned and tested via ParrelSync
- Integrated Photon Fusion to the main scene


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
- Revised overall structure in Unity.  Now using Open XR alone rather than in 
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
- [ ] Prefab menu for building objects
- [ ] Prefab design
- [ ] Custom gesture for bringing up prefab menu
- [ ] Controller interaction for transformations
- [ ] Expand table, add more item portals w/position numbers

```
Nathan
```
- [x] Photon Fusion Integration (4/21)
- [ ] Add authoritative interaction logic for scene objects
- [ ] Set up spawn points to support up to 4 players
- [ ] Ownership transfer of objects

## Scripts
The following scripts are currently available:
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
SnapOnRelease.cs

  This script should also be placed along with GrabPushRotate.cs on any object that
  will be placed on the tabletop.  It snaps the object back down onto the table
  in an appropriate orientation after being removed from the tabletop and placed
  back down.
```

```
TransferItem.cs

  This script allows for the use of item portals to transfer items from one position
  of the table to another.  It should be placed on all ItemPort prefabs (already done)
  on the Opening child object (not on the port object itself).  

  Make sure you assign the following serialized variables in the inspector: 
    Portal 1 - the portal this script is being applied to
    Portal 2 - the portal the item is going to travel to
    Table Drop Point - an empty game object denoting the location on the table where
      the object will land after coming out of the portal.
```

```
PositionPlayer.cs

  This is placed on the XR Origin Hands rig to start the player's camera off in a
  particular location.  Assign the XR rig itself and an empty GameObject positioned
  at the desired start location to the serialized variables in the inspector.
```

```
BasicSpawner.cs

  Sets up multiplayer spawner with Fusion. Players can host or join a session and are spawned at the different markers in the scene. Script handles player spawning, despawning and handling of non-authoritative players
```

```
NetworkVR.cs

  Syncs position and rotation of player's head, left controller, and right controller across the network. Uses state authority and updates the network with transforms of headset and controller. If player doesn't have authority, it reads and applies to local scene player. This allows for live movement of users.
```

```
Player.cs

  Allows player to move character based on input data.
```

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

    To move/translate an object with the hand, press your index finger on any
    surface and keep pressing to move it.  The trigger for moving an object is
    specifically located on the distal interphalangeal joint of each hand
    (the first joint just below the fingertip).  Touching an object on this 
    joint will move it in whichever direction you push. 
    
    An object will not rotate while being translated.
```

```
Rotate Object

  To rotate an object, place the index finger of either your left or right
  hand and point it downward.  Place this downwardly directed finger 
  into the top of the object and hold it there.  Then take the index finger 
  of your other hand and touch anywhere on the object.  The object will begin 
  to automatically rotate at a constant rate (45 deg/sec).

  As long as an index finger is pointed downward into the object and the
  other index finger is touching the object, it will continue to rotate.
  Once the desired rotation has been achieved, simply move one of the
  fingers.

  If you are uncertain about the rotation, you can simply leave one
  index finger pointed downward into the object, remove the other finger
  and assess, then place the other finger back on the object to 
  continue rotating.

  An object will not translate while being rotated.
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
```
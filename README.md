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
 <a href="https://www.youtube.com/watch?v=lwsksg-yi-Y">
  <img src="https://img.youtube.com/vi/lwsksg-yi-Y/0.jpg" width="500" 
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

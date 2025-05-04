# AR Perspective Switcher Menu Setup Guide

This guide explains how to set up the expandable AR perspective switcher menu that enables players to switch between different perspectives using finger touches in AR.

## Creating the UI Menu

### 1. Create the Menu Prefab

1. Create a new UI Canvas in your scene:

   - Right-click in Hierarchy → UI → Canvas
   - Set Render Mode to "World Space"
   - Position it in front of the user in a comfortable position
   - Adjust the Canvas Scale to a small size appropriate for AR (e.g., 0.001)
   - Add a "Canvas Group" component for animation

2. Create the Menu Panel:

   - Add a Panel as a child of the Canvas
   - Name it "PerspectiveMenuPanel"
   - Adjust its size and appearance (e.g., 300x400)
   - Set its anchor to the top-left and position it accordingly

3. Create an Expand Button:

   - Add a Button as a child of the Canvas (not the panel)
   - Name it "ExpandButton"
   - Position it where you want the menu to be accessed from
   - Use an icon or text like "P" or "Perspectives"
   - This button will toggle the menu's expanded state

4. Create Player Buttons inside the Panel:
   - Add 4 Buttons as children of the PerspectiveMenuPanel
   - Name them "Player1Button", "Player2Button", etc.
   - Arrange them vertically or in a grid
   - Add appropriate text labels ("Player 1", "Player 2", etc.)
   - Style them with colors or icons as desired

### 2. Add the PerspectiveSwitcherMenu Script

1. Add the `PerspectiveSwitcherMenu.cs` script to the Canvas or Menu Panel.

2. Configure the script in the Inspector:
   - Assign the Menu Panel reference
   - Assign the Expand Button reference
   - Assign all Player Button references
   - Assign Player Button Text references (if applicable)
   - Assign the SimplePerspectiveSwitcher reference
   - Configure colors for active and normal button states
   - Set collapsed and expanded positions for animation
   - Adjust animation duration as desired

## Menu Positioning Tips for AR

1. **Comfortable Viewing Distance**:

   - Place the menu at a comfortable distance for finger interactions in AR (around 0.3-0.5 meters from the user's default position)

2. **Follow-Along or Fixed Position**:

   - Menu can be set to follow the user (child of the camera or XR Rig) or fixed in world space

3. **Positioning Example**:

   - Collapsed Position: (0, 0, 0) - Only showing the expand button
   - Expanded Position: (0, -200, 0) - Panel drops down when expanded

4. **Button Sizes**:
   - Make buttons large enough for finger interactions in AR (at least 30mm physical size)

## Integration with XR Interaction

1. **Button Interactions**:

   - The menu is designed to be interacted with direct touches or XR ray interaction
   - Make sure your Canvas has a "Graphic Raycaster" component
   - For XR interaction, add an "XR UI Canvas Input Module" to your Event System

2. **Finger Touch in AR**:
   - If using AR Foundation, ensure you have AR raycast support for UI elements
   - Your XR Interaction Manager should handle UI interactions

## Testing the Menu

1. Enter Play Mode and verify:
   - The menu starts in a collapsed state (only showing expand button)
   - Clicking the expand button shows the menu panel with player buttons
   - Player buttons show/hide correctly based on available perspective markers
   - The current perspective is highlighted with the active color
   - Clicking a player button switches to that perspective and updates colors
   - The menu collapses after selection (if enabled)

## Customization

You can customize the appearance and behavior of the menu through the Inspector:

- Change button colors, sizes, and layouts
- Adjust animation speed and behavior
- Configure whether the menu should collapse after selection
- Add additional visual elements or effects

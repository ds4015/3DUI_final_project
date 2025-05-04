# Simple Perspective Switching System

This system allows a player to switch between different perspectives around a tabletop AR experience by moving the camera to predefined positions.

## Setup Instructions

### 1. Player Position Markers

1. Make sure you have position markers in your scene named:

   - `StartMarkerP1` (for Player 1)
   - `StartMarkerP2` (for Player 2)
   - `StartMarkerP3` (for Player 3)
   - `StartMarkerP4` (for Player 4)

2. Ensure these markers are positioned around your table where you want each perspective to be.

3. Each marker should be oriented to face the center of the table.

### 2. Add SimplePerspectiveSwitcher Component

1. Create an empty GameObject called "PerspectiveSystem"
2. Add the `SimplePerspectiveSwitcher.cs` script to this GameObject
3. Configure the component in the Inspector:
   - Assign your Main Camera reference (optional, will find automatically if not set)
   - Assign the Table Center transform (the center point of your table)
   - Adjust Transition Speed if needed (higher = faster transitions)
   - Enable/disable Smooth Transition as preferred

### 3. Add UI (Optional)

1. If you want UI controls and perspective information:
   - Create a UI Canvas if you don't have one
   - Add a Text component for displaying the current perspective
   - Add two Buttons for Previous/Next perspective
   - Add the `PerspectiveUI.cs` script to the Canvas or a child GameObject
   - Configure the component in the Inspector:
     - Assign the perspective switcher reference
     - Assign the text component
     - Assign the button references

## How to Use

### Keyboard Controls

- Press `1`, `2`, `3`, or `4` to switch to the perspective of Player 1, 2, 3, or 4
- Press `Q` to switch to the previous valid player perspective
- Press `E` to switch to the next valid player perspective

### UI Controls (if implemented)

- Click the Previous button to switch to the previous player
- Click the Next button to switch to the next player
- The current perspective will be displayed in the assigned text component

## How It Works

The system simply moves the main camera to the position and rotation of the selected player marker. It transitions smoothly between positions if smooth transition is enabled.

Unlike the more complex implementations, this approach doesn't involve:

- Rotating objects instead of the camera
- Showing virtual representations of hands
- Complex calculations for object-relative perspectives

This simpler approach is easier to set up and works well for showing different perspectives around a table.

# ExtraSnapPointsMadeEasy
This is a quality of life building mod for Valheim.

## Acknowledgements
This mod was inspired by Snap Points Made Easy by MathiasDecrock and FenceSnap by MSchmoecker and parts of the code are based on those mods.

## Features
- Adds custom snap points to most vanilla build pieces in the game that did not have them.
- Adds extra snap points to wooden beams at the halfway points.
- Adds a snap point to the local center of all build pieces, even those added by other mods.
- Has configuration options to enable or disable extra snap points on a piece by piece basis.
- Allows for manual selection of snap points for both the piece you are placing and the piece you are snapping to.
- Allows for manual selection of snap points on only the piece being placed.
- Provides notifications of which snapping mode is active and the snap points selected.

### New Snap Points
New snap points have been added to most crafting and furniture items. Snap points on items like chests, torches, sconces, and banners. Snap points have been chosen to minimize clipping when snapping banners and sconces to walls of any material.

### Extra Snap Points
Extra snap points at the halfway position have been added to all wooden beams and the diagonal wood wall pieces.

### Local Center Snap Points
A snap point is added to center of nearly all build pieces (including those added by mods). The exception to this is pieces that will be unssupported if snapped to their local center during placement. For pieces with support issues an alternative custom snap point is added instead.

### Manual Snapping
Press a key to toggle Manual Snapping Mode, which allows you to cycle through snap points when targeting a piece in game - without needing to point at them directly with the mouse!

### Manual (Closest) Snapping
Press a key to toggle Manual (Closest) Snapping Mode, which allows you to cycle through the snap points of the piece you are placing while snapping to the closest snap-point on the piece being targeted with your mouse.

### Key Bind Configuration
The key-binds for manual snapping are configurable via editing of the configuration file at Valheim/BepInEx/config/searica.valheim.extrasnappointsmadeeasy.cfg or via an in-game configuration manager (live updates, no restart required).

The default key-bindings are:
- Toggle Manual Snapping Mode: Left-Alt
- Toggle Manual (Closest) Snapping Mode: Caps-Lock
- Cycle Snap Points on the piece you are placing (source piece): Left-Ctrl
- Cycle Snap Points ont he piece you are snapping to (target piece): Left-Shift

### Usage
Grab a hammer and select a piece to place. Hit Left-Alt to enable Manual Snapping Mode, a notification will then appear in the center of your screen indicating whether the snapping mode is set to Auto or Manual. Point your piece at the piece you want to snap to and hit Left-Control to cycle through the snap points on the piece you're holding. Use Left-Shift to cycle through destination snap points. For example you can snap a horizontal pillar to the middle of the vertical pillar by cycling both to their midpoint snap points. While cycling through snap points you will also receive notifications indicating which snap point is currently selected on each piece. Alternatively press Caps-Lock to enable Manual (Closest) snapping mode which will snap to the closest snap point on the target piece while still allowing you to manually select the snap point of the piece you are placing.

## Source Code
Github: https://github.com/searica/ExtraSnapPointsMadeEasy

## Donations/Tips
My mods will always be free to use but if you feel like saying thanks you can tip/donate here: https://ko-fi.com/searica

### Contributions
You are welcome to open issues on the Github repository to provide suggestions, feature requests (such as extra snap points for specific pieces), compatibility issues, and bug reports. I'm a grad student and have a lot of personal responsibilities on top of that so I can't promise I will respond quickly, but I do intend to maintain and improve the mod in my free time.

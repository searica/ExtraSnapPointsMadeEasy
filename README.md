# ExtraSnapPointsMadeEasy
This is a quality of life building mod for Valheim.

## Features
- Allows for manual selection of snap points for both the piece you are placing and the piece you are snapping to.
- Allows for manual selection of snap points on only the piece being placed.
- Provides notifications of which snapping mode is active and the snap points selected. The location of these notifications can be set in the mod configuration.
- Automatically determines the type of each build piece and adds extra snap points accordingly. 
- Dynamically updates to configuration changes while in-game.
- Extra snap points can be enabled/disabled:
    - For all pieces,
    - On a piece-by-piece basis,
    - Or by piece type.
- Built in config-file watcher.
- Works with other mods that add build pieces.

## Extra Snap Points
This mod adds extra snap points to increase precision when building. The type of each piece is automatically determined and extra snap points are added based on the type (also works with pieces added by other mods). 

**Custom Snap Points:** Many of the crafting and furniture items that were missing snap points have had custom ones added. For example chests, torches, sconces, and banners all have snap points added. The custom snap have been chosen to minimize clipping when snapping banners and sconces to walls of any material.

**Line Pieces:** If the piece has two snap points then an extra snap point is added midway between them. So all beams and poles have a snap point in the center of them.

**Triangle Pieces:** If the piece has three snap points that form a triangle then snap points are added midway between each pair of points and in the center of the triangle.

**Rect2D Pieces:** If the piece has four snap points that form a rectangle then snap points are added midway along each edge and in the center of the rectangle.

**Roof Top Pieces:** If the piece is tagged as a "roof" piece and has six snap points that form a V shape (roof top pieces) then snap points are added midway along the two bottom edges and midway along the top ridge.

**Cross Pieces:** If the piece has five snap points that from a cross with snap point in the center then no extra snap points are added.

**Torch Pieces:** If the piece is a torch and has no snap points then a snap point is added at the local center and another at the base of the torch.

**Ship Pieces:** If the piece is a ship then no snap points are added to it.

**Cart Pieces:** If the piece is a cart then no snap points are added to it. 

**Other Pieces:** If a piece has no snap points then a snap point is added at the local center of the piece. If a piece has only a single snap point then no extra snap points are added.

## Snapping Modes
This mod adds two different modes of manual snapping to provide greater precision when placing pieces.

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
- Cycle Snap Points on the piece you are snapping to (target piece): Left-Shift

### Usage
Grab a hammer and select a piece to place. Hit Left-Alt to enable Manual Snapping Mode, a notification will then appear in the center of your screen indicating whether the snapping mode is set to Auto or Manual. Point your piece at the piece you want to snap to and hit Left-Control to cycle through the snap points on the piece you're holding. Use Left-Shift to cycle through destination snap points. For example you can snap a horizontal pillar to the middle of the vertical pillar by cycling both to their midpoint snap points. While cycling through snap points you will also receive notifications indicating which snap point is currently selected on each piece. Alternatively press Caps-Lock to enable Manual (Closest) snapping mode which will snap to the closest snap point on the target piece while still allowing you to manually select the snap point of the piece you are placing.

## Source Code
Github: https://github.com/searica/ExtraSnapPointsMadeEasy

## Donations/Tips
My mods will always be free to use but if you feel like saying thanks you can tip/donate here: https://ko-fi.com/searica

### Contributions
If you would like to provide suggestions, make feature requests, or reports bugs and compatibility issues you can either open an issue on the Github repository or tag me (@searica) with a message on the [Jotunn discord](https://discord.gg/DdUt6g7gyA) or the [Odin Plus discord](https://discord.gg/mbkPcvu9ax).

I'm a grad student and have a lot of personal responsibilities on top of that so I can't promise I will respond quickly, but I do intend to maintain and improve the mod in my free time.

### Credits
This mod was inspired by Snap Points Made Easy which is originally by yardik and FenceSnap by MSchmoecker and parts of the code are based on those mods.

### Shameless Self Plug (Other Mods By Me)
If you like this mod you might like some of my other ones.

#### Building Mods
- [MoreVanillaBuildPrefabs](https://valheim.thunderstore.io/package/Searica/More_Vanilla_Build_Prefabs/)
<!--- [Extra Snap Points Made Easy](https://valheim.thunderstore.io/package/Searica/Extra_Snap_Points_Made_Easy/)-->
- [BuildRestrictionTweaksSync](https://valheim.thunderstore.io/package/Searica/BuildRestrictionTweaksSync/)

#### Gameplay Mods
- [FortifySkillsRedux](https://valheim.thunderstore.io/package/Searica/FortifySkillsRedux/)
- [DodgeShortcut](https://valheim.thunderstore.io/package/Searica/DodgeShortcut/)

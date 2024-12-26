# ExtraSnapPointsMadeEasy
This is a quality of life building mod for Valheim.

## Version 2.0.0 Notice
If you are updating from any version before 2.0.0 you may need to regenerate your config file!

## Features
- Allows for manual selection of snap points on only the piece being placed.
- Disables the vanilla manual snapping by default (you can re-enable it) as this mod already added manual snapping.
- Allows for manual selection of snap points for both the piece you are placing and the piece you are snapping to.
- Allows snapping pieces to the world grid.
- Shows key hints for all of the snapping functionality and key binds added by the mod.
- Provides notifications of which snapping mode is active and the snap points selected. The location of these notifications can be set in the mod configuration.
- Automatically determines the type of each build piece and adds extra snap points accordingly. 
- Dynamically updates from configuration setting changes while in-game.
- Extra snap points can be enabled/disabled:
    - For all pieces,
    - On a piece-by-piece basis,
    - Or by piece type.
- Built in config-file watcher.
- Works with other mods that add build pieces.

## Snapping Modes
This mod adds two different modes of manual snapping to provide greater precision when placing pieces.

### Manual Snapping
Press a key to toggle Manual Snapping Mode, which allows you to cycle through the snap points of the piece you are placing while snapping to the closest snap-point on the piece being targeted with your mouse.

### Manual+ Snapping
Press a key to toggle Manual+ Snapping Mode, which allows you to cycle through snap points when targeting a piece in game - without needing to point at them directly with the mouse!

### Grid Snapping
Press a key to toggle Grid Snapping Mode, which snaps pieces to the world grid. Pieces are only snapped to the world grid on the horizontal plane to avoid snapping pieces to thin air. Precision of the grid can be toggled between 1m and 0.5m.

### Key Bind Configuration
The key-binds for manual snapping are configurable via editing of the configuration file at Valheim/BepInEx/config/searica.valheim.extrasnappointsmadeeasy.cfg or via an in-game configuration manager (live updates, no restart required).

The default key-bindings are:
- Toggle Manual Snapping Mode: Caps-Lock
- Toggle Manual+ Snapping Mode: Left-Alt
- Cycle Snap Points on the piece you are placing: Q
- Cycle Snap Points on the piece you are snapping to: E
- Toggle Grid Snapping Mode: F3
- Cycle Grid precision: F4

### Usage
Grab a hammer and select a piece to place. Hit Left-Alt to enable Manual Snapping Mode, a notification will then appear in the center of your screen indicating whether the snapping mode is set to Auto or Manual. Point your piece at the piece you want to snap to and hit Left-Control to cycle through the snap points on the piece you're holding. Use Left-Shift to cycle through destination snap points. For example you can snap a horizontal pillar to the middle of the vertical pillar by cycling both to their midpoint snap points. While cycling through snap points you will also receive notifications indicating which snap point is currently selected on each piece. Alternatively press Caps-Lock to enable Manual (Closest) snapping mode which will snap to the closest snap point on the target piece while still allowing you to manually select the snap point of the piece you are placing.

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

## Donations/Tips
My mods will always be free to use but if you feel like saying thanks you can tip/donate.

| My Ko-fi: | [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/searica) |
|-----------|---------------|

## Source Code
Source code is available on Github.

| Github Repository: | <img height="18" src="https://github.githubassets.com/favicons/favicon-dark.svg"></img><a href="https://https://github.com/searica/ExtraSnapPointsMadeEasy"> Extra Snap Points Made Easy</a> |
|-----------|---------------|

### Contributions
If you would like to provide suggestions, make feature requests, or reports bugs and compatibility issues you can either open an issue on the Github repository or tag me (@searica) with a message on my discord [Searica's Mods](https://discord.gg/sFmGTBYN6n).

I'm a grad student and have a lot of personal responsibilities on top of that so I can't promise I will respond quickly, but I do intend to maintain and improve the mod in my free time.

### Credits
This mod was inspired by Snap Points Made Easy which is originally by yardik and FenceSnap by MSchmoecker and parts of the code are based on those mods. 
flo123333 contributed massively to version 2.0.0 and is the reason this mod now supports named snap points.

## Shameless Self Plug (Other Mods By Me)
If you like this mod you might like some of my other ones.

#### Building Mods
- [More Vanilla Build Prefabs](https://thunderstore.io/c/valheim/p/Searica/More_Vanilla_Build_Prefabs/)
- [AdvancedTerrainModifiers](https://thunderstore.io/c/valheim/p/Searica/AdvancedTerrainModifiers/)
- [BuildRestrictionTweaksSync](https://thunderstore.io/c/valheim/p/Searica/BuildRestrictionTweaksSync/)
- [ToolTweaks](https://thunderstore.io/c/valheim/p/Searica/ToolTweaks/)

#### Gameplay Mods
- [CameraTweaks](https://thunderstore.io/c/valheim/p/Searica/CameraTweaks/)
- [DodgeShortcut](https://thunderstore.io/c/valheim/p/Searica/DodgeShortcut/)
- [DiscoveryPins](https://thunderstore.io/c/valheim/p/Searica/DiscoveryPins/)
- [FortifySkillsRedux](https://thunderstore.io/c/valheim/p/Searica/FortifySkillsRedux/)
- [OpenSesame](https://thunderstore.io/c/valheim/p/Searica/OpenSesame/)
- [ProjectileTweaks](https://thunderstore.io/c/valheim/p/Searica/ProjectileTweaks/)
- [SkilledCarryWeight](https://thunderstore.io/c/valheim/p/Searica/SkilledCarryWeight/)
- [SafetyStatus](https://thunderstore.io/c/valheim/p/Searica/SafetyStatus/)
- [WatchWhereYouStab](https://thunderstore.io/c/valheim/p/Searica/WatchWhereYouStab/)

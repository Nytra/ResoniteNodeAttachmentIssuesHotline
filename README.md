# NodeAttachmentIssuesHotline

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that fixes the bug which causes ProtoFlux nodes to remain in the same group even when they have been disconnected.

Due to the way that ProtoFlux works, this is a local-only fix. This means that other users in the session who don't have this mod will still get the bug even if they select the same nodes as you.

It is unknown if this will have any side effects for the way that ProtoFlux is executed or compiled or if it could cause de-syncing.

This is a workaround for this issue: https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/907

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [NodeAttachmentIssuesHotline.dll](https://github.com/Nytra/ResoniteNodeAttachmentIssuesHotline/releases/download/v1.0.0/NodeAttachmentIssuesHotline.dll) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

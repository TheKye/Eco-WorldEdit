# Eco-WorldEdit
the WorldEdit Plugin By Mampf

you may not redistribute modified versions of this plugin. this plugin remains the property of the original creator and you may not without their consent release a modified version of this plugin.

This plugin remains the property of the original creator Mampf and all rights reserved. 

## Supported Eco Version
**9.3.4**

## Contributions
Contributions are Welcome! if you feel like you can improve something or have an idea Create an Issue or a Pull Request!

## Current Contributors 
Kye - Elixr Mods, Eco Modding Community - Code and Project Management

Tavren - Eco Russian Community - Code

StallEF - Eco Russian Community - Project Management Assitance

## Previous Contributors
Mampf - Original Creator

## Notes
I have updated it to 9.0 and made it standalone, The commands are the same but there is a few things to note:

/walls

Using the walls command has been changed up for 9.0, When using this command you can now use it like this:

/walls Ashlar Limestone Wall

In 9.0 the block call is AshlarLimestoneWallBlock - We have auto included the Block part and removed the spaces for you when you use the command to make life easier in the next update i will force both to lower to make it easier again

## Commands
```
/worldedit (/we) | Display available commands
/wand | Gives you the World Edit selection tool (left and right click to select an area).
/rmwand | Remove wand
/set <blocktype> | Set the selection to the desired <blocktype>.
/we delete (/del) | Clear selected area. Same as /set Empty
/replace <replaceBlock>, <desiredBlock> | Lets you replace a specific block of a selection with a desired block.
/replace <desiredBlock> | Replace everything except air with desired block
/walls <blocktype> | Creates a wall inside the selection.
/expand <amount> <direction> | Resizes the selection in the <direction> or where you are looking at
/reduce <amount> <direction> | opposite of expand
/shift <amount> <direction> | move selection
/move <amount> <direction> | move blocks in selection
/up <amount> | Teleports you up in the air.
/stack <direction> <amount>, <offset> | "Stacks" the current selection with given offset
/copy | Copies a selection.
/paste | Pastes a copied selection from clipboard.
/cut | Copies and clears selection.
/rotate <degree> | Rotate the clipboard
/undo <count> | Revert the last command. Can undo up to 10 commands. If count provided as number undo the number of commands
/import <filename> | Import a schematic file into clipboard. If file not found shows list of schematics in directory. If no <filename> provided shows list of schematic files
/export <filename> | Export the clipboard into a file
/distr <format>, <filename> | Show information about current selection. Optional <format> parameter can be brief or detail (short: b or d). Can save report to file in Schematic folder
/setpos1 | Set first position to where you are standing
/setpos2 | Set second position to where you are standing
/reset | Resets First and Second positions
/selclaim | Select current claim where player stands on ground level
/addclaim | Add current claim where player stands to the selection
/expclaim <amount> <direction> | Expands selection to include amount of claims in given direction or where player looking
/we version (/weversion) | for debugging gives you the current version when reporting bugs
```

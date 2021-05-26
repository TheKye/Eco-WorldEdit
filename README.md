# WorldEdit
WorldEdit Plugin for game Eco.

Original author: Mampf

This is fork from [TheKye/Eco-WorldEdit](https://github.com/TheKye/Eco-WorldEdit) and this version deleoped independently by the Russian Eco Community.
Some changes and bugfixes can be implimented from the original repository.

## Supported Eco Version
**9.3.4**

## Notes
This version where we rewrite the original code, do refactoring and review and keep working on it, adding new features and commands.

Our first goal is make exported files usable at anytime and enywhere so you not lost your creations. To achieve that we exporting creations into JSON format with own extension **.ecobp**

There currently no support for other extensions and formats.


@TheKye have updated it to 9.0 and made it standalone, The commands are the same but there is a few things to note:

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
/expand <amount> <direction> | Resizes the selection in the direction where you are looking at
/contract <amount> <direction> | opposite of expand
/shift <amount> <direction> | move selection
/move <amount> <direction> | move blocks in selection
/up <amount> | Teleports you up in the air.
/stack <amount> <direction> | "Stacks" the current selection
/copy | Copies a selection.
/paste | Pastes a copied selection from clipboard.
/rotate <degree> | Rotate the clipboard
/undo | Revert the last command. Can undo up to 10 commands
/import <name> | Import a schematic file into clipboard
/export <name> | Export the clipboard into a file
/distr | Show information about current selection
/setpos1 | Set first position to where you are standing
/setpos2 | Set second position to where you are standing
/reset | Resets First and Second positions
/we version (/weversion) | for debugging gives you the current version when reporting bugs
```

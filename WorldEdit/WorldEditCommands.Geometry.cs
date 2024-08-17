using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Eco.Shared.Logging;
using Eco.World.Blocks;
using System;
using static Eco.Mods.WorldEdit.Commands.PyramidCommand;

namespace Eco.Mods.WorldEdit
{
	public partial class WorldEditCommands
	{
		[ChatSubCommand("WorldEdit", "Sets the Area on the outside of the selection to selected wall type", "walls", ChatAuthorizationLevel.Admin)]
		public static void Walls(User user, string typeName)
		{
			try
			{
				typeName = typeName.Replace(" ", "");
				WallsCommand command = new WallsCommand(user, typeName);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"{command.BlocksChanged} blocks changed.");
				}
			}
			catch (WorldEditCommandException e)
			{
				user.Player.ErrorLocStr(e.Message);
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Create pyramid at user position with desired height", "pyramid", ChatAuthorizationLevel.Admin)]
		public static void Pyramid(User user, int height, string blockType = null, string styleStr = "full", bool clear = true)
		{
			try
			{
				height = height > 0 ? height : 1;
				blockType = blockType is not null ? blockType.Replace(" ", "") : typeof(DirtBlock).Name;
				PyramidStyle style = PyramidStyle.Full;
				if (Enum.TryParse(typeof(PyramidStyle), styleStr, true, out object result)) { style = (PyramidStyle)result; }
				PyramidCommand command = new PyramidCommand(user, height, blockType, style, clear);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"{command.BlocksChanged} blocks changed.");
				}
			}
			catch (WorldEditCommandException e)
			{
				user.Player.ErrorLocStr(e.Message);
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}
	}
}
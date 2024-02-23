﻿using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;

namespace Eco.Mods.WorldEdit
{
	public partial class WorldEditCommands
	{
		[ChatSubCommand("WorldEdit", "Drain water in Selected Area", "drain", ChatAuthorizationLevel.Admin)]
		public static void Drain(User user)
		{
			try
			{
				ReplaceCommand command = new ReplaceCommand(user, "Water", "Empty");
				if (command.Invoke())
				{
					user.Player.MsgLoc($"{command.BlocksChanged} water blocks drained in {command.ElapsedMilliseconds}ms.");
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

		[ChatSubCommand("WorldEdit", "Drain water in Selected Area", "fixwater", ChatAuthorizationLevel.Admin)]
		public static void Fixwater(User user, int height = 0)
		{
			try
			{
				FixwaterCommand command = new FixwaterCommand(user, height);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"{command.BlocksChanged} blocks changed in {command.ElapsedMilliseconds}ms.");
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
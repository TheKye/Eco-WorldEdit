using System;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Mods.WorldEdit.Commands;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit
{
	public class WorldEditCommands : IChatCommandHandler
	{
		[ChatCommand("Lists of world edit commands", "we", ChatAuthorizationLevel.Admin)] public static void WorldEdit(User user) { }

		[ChatSubCommand("WorldEdit", "Gives the player a Wand for using world edit", "wand", ChatAuthorizationLevel.Admin)]
		public static void Wand(User user)
		{
			try
			{
				user.Inventory.AddItems(WorldEditManager.getWandItemStack());
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Removes the wand from the players inventory", "rmwand", ChatAuthorizationLevel.Admin)]
		public static void RmWand(User user)
		{
			try
			{
				user.Inventory.TryRemoveItems(WorldEditManager.getWandItemStack());
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Sets The Selected Area to the desired Block", "set", ChatAuthorizationLevel.Admin)]
		public static void Set(User user, string pTypeName)
		{
			try
			{
				pTypeName = pTypeName.Replace(" ", "");
				SetCommand command = new SetCommand(user, pTypeName);
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

		[ChatSubCommand("WorldEdit", "Clears the Selected Area", "del", ChatAuthorizationLevel.Admin)]
		public static void Delete(User user)
		{
			Set(user, "Empty");
		}

		[ChatSubCommand("WorldEdit", "Replace a Specific Block Type with Another Block Example: replace sand, dirt, this will replace sand with dirt", "replace", ChatAuthorizationLevel.Admin)]
		public static void Replace(User user, string pTypeNames)
		{
			try
			{
				string[] splitted = pTypeNames.Trim().Split(',');
				string toFind = splitted[0].ToLower().Replace(" ", "");

				string toReplace = string.Empty;

				if (splitted.Length >= 2)
					toReplace = splitted[1].Trim().ToLower().Replace(" ", "");

				ReplaceCommand command = new ReplaceCommand(user, toFind, toReplace);
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

		[ChatSubCommand("WorldEdit", "/stack", "Stack ", ChatAuthorizationLevel.Admin)]
		public static void Stack(User user, string directionAndAmount = "1")
		{
			try
			{
				StackCommand command = new StackCommand(user, directionAndAmount);
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

		[ChatSubCommand("WorldEdit", "/move", "move", ChatAuthorizationLevel.Admin)]
		public static void Move(User user, string directionAndAmount = "1")
		{
			try
			{
				MoveCommand command = new MoveCommand(user, directionAndAmount);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"{command.BlocksChanged} blocks moved.");
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

		[ChatSubCommand("WorldEdit", "expand the selected area by the specified amount in the looking direction", "expands", ChatAuthorizationLevel.Admin)]
		public static void Expand(User user, string directionAndAmount = "1")
		{
			try
			{
				SelectionCommand command = SelectionCommand.ExpandCommand(user, directionAndAmount);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Expanded selection {command.amount} {command.direction}");
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

		[ChatSubCommand("WorldEdit", "contract will lessen the selected area by the specified amount in the looking direction", "contract", ChatAuthorizationLevel.Admin)]
		public static void Contract(User user, string directionAndAmount = "1")
		{
			try
			{
				SelectionCommand command = SelectionCommand.ContractCommand(user, directionAndAmount);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Contracted selection {command.amount} {command.direction}");
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

		[ChatSubCommand("WorldEdit", "shift will move the selected area by the specified amount in the looking direction", "shift", ChatAuthorizationLevel.Admin)]
		public static void Shift(User user, string directionAndAmount = "1")
		{
			try
			{
				SelectionCommand command = SelectionCommand.ShiftCommand(user, directionAndAmount);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Shifted selection {command.amount} {command.direction}");
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

		[ChatSubCommand("WorldEdit", "up will move the player upwards by the specified amount and place a block under the player if needed", "up", ChatAuthorizationLevel.Admin)]
		public static void Up(User user, int count = 1)
		{
			try
			{
				WorldEditCommand command = new UpCommand(user, count);
				if (command.Invoke())
				{
					//Silence?
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

		[ChatSubCommand("WorldEdit", "undo will revert the last action done using world edit, up too 10 times", "undo", ChatAuthorizationLevel.Admin)]
		public static void Undo(User user)
		{
			try
			{
				UserSession userSession = WorldEditManager.GetUserSession(user);
				if (userSession.ExecutedCommands.TryPop(out WorldEditCommand command))
				{
					if (command.Undo())
					{
						user.Player.MsgLoc($"Undo done.");
					}
				}
				else
				{
					user.Player.ErrorLocStr($"Nothing to undo");
					//user.Player.ErrorLocStr($"You can't use undo right now!");
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

		[ChatSubCommand("WorldEdit", "copy will copy the selected area ready for pasting or exporting", "copy", ChatAuthorizationLevel.Admin)]
		public static void Copy(User user)
		{
			try
			{
				WorldEditCommand command = new CopyCommand(user);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Copy done in {command.ElapsedMilliseconds}ms.");
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

		[ChatSubCommand("WorldEdit", "paste will paste the copied selection or imported schematic from where the player is standing", "paste", ChatAuthorizationLevel.Admin)]
		public static void Paste(User user)
		{
			try
			{
				WorldEditCommand command = new PasteCommand(user);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Paste done in {command.ElapsedMilliseconds}ms.");
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

		[ChatSubCommand("WorldEdit", "rotate will rotate all blocks and items in your clipboard, usable by degrees IE: 90, 180, 270", "rotate", ChatAuthorizationLevel.Admin)]
		public static void Rotate(User user, int degrees = 90)
		{
			try
			{
				WorldEditCommand command = new RotateCommand(user, degrees);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Rotation in clipboard done.");
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

		[ChatSubCommand("WorldEdit", "export will turn your copied selection into a schematic that you can share with friends or import into a fresh world!", "export", ChatAuthorizationLevel.Admin)]
		public static void Export(User user, string fileName)
		{
			try
			{
				WorldEditCommand command = new ExportCommand(user, fileName);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Export done in {command.ElapsedMilliseconds}ms.");
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

		[ChatSubCommand("WorldEdit", "import will import a schematic that you or someone else has exported", "import", ChatAuthorizationLevel.Admin)]
		public static void Import(User user, string fileName)
		{
			try
			{
				WorldEditCommand command = new ImportCommand(user, fileName);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Import done in {command.ElapsedMilliseconds}ms. Use /paste");
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

		[ChatSubCommand("WorldEdit", "distr will give you a detailed list of all items in your selected area", "distr", ChatAuthorizationLevel.Admin)]
		public static void Distr(User user)
		{
			try
			{
				WorldEditCommand command = new DistrCommand(user);
				if (command.Invoke())
				{
					//Output done in he command
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

		[ChatSubCommand("WorldEdit", "/grow", "grow", ChatAuthorizationLevel.Admin)]
		public static void Grow(User user)
		{
			try
			{
				WorldEditCommand command = new GrowCommand(user);
				if (command.Invoke())
				{
					user.Player.MsgLoc($"Grow done.");
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

		[ChatSubCommand("WorldEdit", "Set First Position To player position", "setpos1", ChatAuthorizationLevel.Admin)]
		public static void SetPos1(User user)
		{
			try
			{
				Vector3 pos = user.Position;
				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;
				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession session = WorldEditManager.GetUserSession(user);
				session.SetFirstPosition((Vector3i?)pos);

				user.Player.MsgLoc($"First Position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));

			}
		}

		[ChatSubCommand("WorldEdit", "Set Second Position To player position", "setpos2", ChatAuthorizationLevel.Admin)]
		public static void SetPos2(User user)
		{
			try
			{
				Vector3 pos = user.Position;
				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;
				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession session = WorldEditManager.GetUserSession(user);
				session.SetSecondPosition((Vector3i?)pos);

				user.Player.MsgLoc($"Second Position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Resets selection and both positions", "reset", ChatAuthorizationLevel.Admin)]
		public static void Reset(User user)
		{
			try
			{
				UserSession session = WorldEditManager.GetUserSession(user);
				session.SetFirstPosition(null);
				session.SetSecondPosition(null);

				user.Player.MsgLoc($"WorldEdit: Positions reset");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Show World Edit version", "WEversion", ChatAuthorizationLevel.Admin)]
		public static void Version(User user)
		{
			user.Player.MsgLocStr($"World Edit Version:" + EcoWorldEdit.Version);
		}
	}
}
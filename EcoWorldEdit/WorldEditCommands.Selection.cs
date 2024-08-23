using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Shared.Logging;
using Eco.Shared.Voxel;
using System;

namespace Eco.Mods.WorldEdit
{
	public partial class WorldEditCommands
	{
		[ChatSubCommand("WorldEdit", "expand the selected area by the specified amount in the looking direction", "expand", ChatAuthorizationLevel.Admin)]
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

		[ChatSubCommand("WorldEdit", "Reduce the selected area by the specified amount in the looking direction if not provided", "reduce", ChatAuthorizationLevel.Admin)]
		public static void Reduce(User user, string directionAndAmount = "1")
		{
			try
			{
				SelectionCommand command = SelectionCommand.ReduceCommand(user, directionAndAmount);
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

		[ChatSubCommand("WorldEdit", "Set First Position to given coordinate or player position", "setpos1", ChatAuthorizationLevel.Admin)]
		public static void SetPos1(User user, string coordinate = null)
		{
			try
			{
				Vector3i? pos = WorldEditUtils.GetPositionForUser(user, coordinate);
				if (pos is null) { return; }

				UserSession session = WorldEditManager.GetUserSession(user);
				session.SetFirstPosition(pos.Value);

				user.Player.MsgLoc($"First Position set to {pos}");
			}
			catch (Exception e)
			{
				Log.WriteErrorLineLocStr(e.ToString());

			}
		}

		[ChatSubCommand("WorldEdit", "Set Second Position to given coordinate or player position", "setpos2", ChatAuthorizationLevel.Admin)]
		public static void SetPos2(User user, string coordinate = null)
		{
			try
			{
				Vector3i? pos = WorldEditUtils.GetPositionForUser(user, coordinate);
				if (pos is null) { return; }

				UserSession session = WorldEditManager.GetUserSession(user);
				session.SetSecondPosition(pos.Value);

				user.Player.MsgLoc($"Second Position set to {pos}");
			}
			catch (Exception e)
			{
				Log.WriteErrorLineLocStr(e.ToString());
			}
		}

		[ChatSubCommand("WorldEdit", "Resets selection and both positions", "reset", ChatAuthorizationLevel.Admin)]
		public static void Reset(User user)
		{
			try
			{
				UserSession session = WorldEditManager.GetUserSession(user);
				session.ResetSelection();

				user.Player.MsgLoc($"WorldEdit: Positions reset");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		#region Claim related commands
		[ChatSubCommand("WorldEdit", "Select current claim where player stands on ground level", "selclaim", ChatAuthorizationLevel.Admin)]
		public static void Selclaim(User user)
		{
			try
			{
				Vector3i pos = user.Position.Round();
				Vector2i claimPos = PlotUtil.RawPlotPos(pos.XZ).RawXY;
				UserSession session = WorldEditManager.GetUserSession(user);

				session.SetFirstPosition(claimPos.X_Z(pos.Y - 1));
				session.SetSecondPosition(WorldEditUtils.SecondPlotPos(claimPos).X_Z(pos.Y - 1));

				user.Player.MsgLoc($"First Position set to {session.Selection.min}");
				user.Player.MsgLoc($"Second Position set to {session.Selection.max}");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Add current claim where player stands to the selection", "addclaim", ChatAuthorizationLevel.Admin)]
		public static void Addclaim(User user)
		{
			try
			{
				Vector3i pos = user.Position.Round();
				Vector2i claimPos = PlotUtil.RawPlotPos(pos.XZ).RawXY;
				UserSession session = WorldEditManager.GetUserSession(user);

				WorldRange range = session.Selection;
				range.ExtendToInclude(claimPos.X_Z(pos.Y - 1));
				range.ExtendToInclude(WorldEditUtils.SecondPlotPos(claimPos).X_Z(pos.Y - 1));
				session.SetSelection(range);

				user.Player.MsgLoc($"First Position now at {session.Selection.min}");
				user.Player.MsgLoc($"Second Position now at {session.Selection.max}");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[ChatSubCommand("WorldEdit", "Expands selection to include amount of claims in given direction or where player looking", "expclaim", ChatAuthorizationLevel.Admin)]
		public static void Expclaim(User user, string args = "1")
		{
			try
			{
				UserSession session = WorldEditManager.GetUserSession(user);
				if (!session.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
				Direction direction = WorldEditUtils.ParseDirectionAndAmountArgs(user, args, out int amount);
				if (direction == Direction.Unknown ||
					direction == Direction.None ||
					direction == Direction.Up ||
					direction == Direction.Down) { throw new WorldEditCommandException("Unable to determine direction"); }
				WorldRange range = session.Selection;
				Vector3i pos = default;
				if (range.min.y <= range.max.y) pos.y = range.min.y; else pos.y = range.max.y;
				switch (direction)
				{
					case Direction.Left:
					case Direction.Back:
						if (range.min.x <= range.max.x) pos.x = range.min.x; else pos.x = range.max.x;
						if (range.min.z <= range.max.z) pos.z = range.min.z; else pos.z = range.max.z;
						break;
					case Direction.Right:
					case Direction.Forward:
						if (range.min.x <= range.max.x) pos.x = range.max.x; else pos.x = range.min.x;
						if (range.min.z <= range.max.z) pos.z = range.max.z; else pos.z = range.min.z;
						break;
				}
				pos += direction.ToVec() * (PlotUtil.PropertyPlotLength - 1) * amount;
				Vector2i claimPos = PlotUtil.RawPlotPos(pos.XZ).RawXY;
				range.ExtendToInclude(claimPos.X_Z(pos.Y));
				range.ExtendToInclude(WorldEditUtils.SecondPlotPos(claimPos).X_Z(pos.Y));
				session.SetSelection(range);

				user.Player.MsgLoc($"First Position now at {session.Selection.min}");
				user.Player.MsgLoc($"Second Position now at {session.Selection.max}");
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
		#endregion Claim related commands

	}
}
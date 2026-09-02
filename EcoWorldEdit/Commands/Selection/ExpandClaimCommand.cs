using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.Shared.Voxel;

namespace Eco.Mods.WorldEdit.Commands.Selection
{
	[ChatCommandHandler]
	internal static class ExpandClaimCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Expands the selection by claim plots in a direction.", shortCut: "expclaim", level: ChatAuthorizationLevel.Admin)]
		public static void ExpandClaim(User user, string args = "1")
		{
			try
			{
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				if (!session.Selection.IsSet()) throw new WorldEditCommandException("Please set both points first!");
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, args);
				if (direction is Direction.Unknown or Direction.None or Direction.Up or Direction.Down)
					throw new WorldEditCommandException("Unable to determine horizontal direction.");

				WorldRange selection = session.Selection.FixToWorldSize();
				Vector3i edge = direction is Direction.Left or Direction.Back ? selection.min : selection.max;
				edge += direction.ToVec() * PlotUtil.PropertyPlotLength * amount;
				Vector2i plot = PlotUtil.RawPlotPos(edge.XZ).RawXY;
				selection.ExtendToInclude(plot.X_Z(edge.y));
				selection.ExtendToInclude(CommandParsing.SecondPlotPosition(plot).X_Z(edge.y));
				session.SetSelection(selection);
				Logging.Success($"First Position now at {selection.min}", user.Player);
				Logging.Success($"Second Position now at {selection.max}", user.Player);
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}
	}
}

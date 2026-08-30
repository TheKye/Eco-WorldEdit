using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Logging;
using Eco.Shared.Math;
using Eco.Shared.Voxel;

namespace Eco.Mods.WorldEdit.Commands.Selection
{
	[ChatCommandHandler]
	internal static class SelectClaimCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Select current claim where player stands on ground level", shortCut: "selclaim", level: ChatAuthorizationLevel.Admin)]
		public static void SelectClaim(User user)
		{
			try
			{
				Vector3i position = user.Position.Round();
				Vector2i plot = PlotUtil.RawPlotPos(position.XZ).RawXY;
				Vector2i oppositeCorner = CommandParsing.SecondPlotPosition(plot);
				WorldRange selection = new(plot.X_Z(position.y - 1), oppositeCorner.X_Z(position.y - 1));
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				session.SetSelection(selection);
				user.Player.MsgLoc($"First Position now at {selection.min}");
				user.Player.MsgLoc($"Second Position now at {selection.max}");
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}

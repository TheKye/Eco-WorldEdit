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
	internal static class AddClaimCommand
	{
		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Expands the selection to include the claim plot at your position.", shortCut: "addclaim", level: ChatAuthorizationLevel.Admin)]
		public static void AddClaim(User user)
		{
			try
			{
				Vector3i position = user.Position.Round();
				Vector2i plot = PlotUtil.RawPlotPos(position.XZ).RawXY;
				UserSession session = WorldEditManager.Obj.GetUserSession(user);
				WorldRange selection = session.Selection;
				selection.ExtendToInclude(plot.X_Z(position.y - 1));
				selection.ExtendToInclude(CommandParsing.SecondPlotPosition(plot).X_Z(position.y - 1));
				session.SetSelection(selection);
				user.Player.MsgLoc($"First Position now at {selection.min}");
				user.Player.MsgLoc($"Second Position now at {selection.max}");
			}
			catch (Exception exception) { Log.WriteException(exception); }
		}
	}
}

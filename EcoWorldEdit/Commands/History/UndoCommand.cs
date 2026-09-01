using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils;
using Eco.Shared.Logging;

namespace Eco.Mods.WorldEdit.Commands.History
{
	[ChatCommandHandler]
	internal sealed class UndoCommand(int ActionCount) : HistoryNavigationCommand(ActionCount)
	{
		protected override string ActionName => "Undo";
		protected override HistoryDirection Direction => HistoryDirection.Undo;
		protected override LimitedStack<HistoryEntry> GetSource(UserSession session) => session.UndoHistory;
		protected override LimitedStack<HistoryEntry> GetDestination(UserSession session) => session.RedoHistory;

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Undoes one or more WorldEdit actions.", shortCut: "undo", level: ChatAuthorizationLevel.Admin)]
		public static void Undo(User user, int count = 1)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new UndoCommand(count));
				if (result.Result.Success)
				{
					user.Player.MsgLoc($"Undo done in {result.Elapsed.TotalMilliseconds}ms.");
				}
				else
				{
					user.Player.Error(result.Result.Message);
				}
			}
			catch (Exception exception)
			{
				Log.WriteException(exception);
			}
		}
	}
}

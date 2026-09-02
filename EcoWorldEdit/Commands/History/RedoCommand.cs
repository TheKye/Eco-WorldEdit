using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Commands;

namespace Eco.Mods.WorldEdit.Commands.History
{
	[ChatCommandHandler]
	internal sealed class RedoCommand(int ActionCount) : HistoryNavigationCommand(ActionCount)
	{
		protected override string ActionName => "Redo";
		protected override HistoryDirection Direction => HistoryDirection.Redo;
		protected override LimitedStack<HistoryEntry> GetSource(UserSession session) => session.RedoHistory;
		protected override LimitedStack<HistoryEntry> GetDestination(UserSession session) => session.UndoHistory;

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Redoes one or more previously undone WorldEdit actions.", shortCut: "redo", level: ChatAuthorizationLevel.Admin)]
		public static void Redo(User user, int count = 1)
		{
			try
			{
				CommandResult result = CommandDispatcher.Obj.Execute(user, new RedoCommand(count));
				if (result.Result.Success)
				{
					Logging.Success($"Redo done in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				}
				else
				{
					Logging.Error(result.Result.Message, user.Player);
				}
			}
			catch (Exception exception)
			{
				Logging.Exception(exception, user.Player);
			}
		}
	}
}

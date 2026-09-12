using System.Collections.Concurrent;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Gameplay.Rooms;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.Shared.Utils;

namespace Eco.Mods.WorldEdit.Core.Commands
{
	internal sealed class CommandDispatcher : AutoSingleton<CommandDispatcher>
	{
		private readonly ConcurrentDictionary<User, CommandInvocation> _activeCommands = new();
		public IReadOnlyDictionary<User, CommandInvocation> ActiveCommands => this._activeCommands;

		public CommandResult Execute(User user, IWorldEditCommand command, WorldRange? selection = null)
		{
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(command);

			CommandInvocation invocation = this.CreateInvocation(user, command, selection);

			if (!this.TryBeginCommand(user, invocation))
			{
				invocation.Dispose();
				return new(Result.FailLocStr("Another command is still executing."), 0, TimeSpan.Zero);
			}

			try
			{
				return this.InvokeCommand(invocation);
			}
			finally
			{
				this.FinalizeCommand(user, invocation);
			}
		}

		private CommandInvocation CreateInvocation(User user, IWorldEditCommand command, WorldRange? selection)
		{
			UserSession userSession = WorldEditManager.Obj.GetUserSession(user);
			CommandContext context = new(userSession, selection ?? userSession.Selection);

			switch (command.HistoryPolicy)
			{
				case CommandHistoryPolicy.RecordWorldChanges: context.CreateScope(); break;
				case CommandHistoryPolicy.ManageHistory: break; // Left CommandContext without default CommandScope because Undo/Redo create own scopes in HistoryNavigationCommand
				default: throw new ArgumentOutOfRangeException(nameof(command.HistoryPolicy), command.HistoryPolicy, "Unsupported command history policy.");
			}

			return new CommandInvocation(command, context);
		}

		private CommandResult InvokeCommand(CommandInvocation invocation)
		{
			CommandContext context = invocation.Context;
			try
			{
				invocation.Invoke();
				if (context.Scopes.Any(x => x.BlockManager.HasPendingBatch)) throw new InvalidOperationException("A command completed with an uncommitted block batch.");
				if (!invocation.Command.PreserveClipboardAnchor) context.UserSession.DisableClipboardAnchor();
				return new(Result.Succeeded, context.ChangedBlocks, invocation.Elapsed);
			}
			catch (OperationCanceledException) when (invocation.IsCancellationRequested)
			{
				return new(Result.FailLocStr("Command was cancelled."), context.ChangedBlocks, invocation.Elapsed);
			}
			catch (WorldEditCommandException exception)
			{
				return new(Result.FailLocStr(exception.Message), context.ChangedBlocks, invocation.Elapsed);
			}
			catch (Exception exception)
			{
				Logging.Exception(exception, context.Player);
				Result result = Result.FailLocStr("An unexpected error occurred.");
				result.AppendDebug(exception.Message);
				return new(result, context.ChangedBlocks, invocation.Elapsed);
			}
		}

		private void FinalizeCommand(User user, CommandInvocation invocation)
		{
			try
			{
				if (invocation.Command.HistoryPolicy == CommandHistoryPolicy.RecordWorldChanges)
				{
					this.CommitChanges(invocation.Context.UserSession, invocation.Context.Changes);
				}
			}
			catch (Exception exception)
			{
				Logging.Exception(exception, invocation.Context.Player);
			}

			try
			{
				this.ScheduleRoomRecalculation(invocation.Context.Scopes);
			}
			catch (Exception exception)
			{
				Logging.Exception(exception, invocation.Context.Player);
			}
			finally
			{
				this.EndCommand(user, invocation);
			}
		}

		private void CommitChanges(UserSession userSession, CommandChangeSet changeSet)
		{
			if (changeSet.ChangedBlocks <= 0) return;
			bool discardedRecovery = userSession.DiscardPendingRecoveries();
			userSession.UndoHistory.Push(new HistoryEntry(changeSet));
			userSession.RedoHistory.Clear();
			if (discardedRecovery) Logging.Success($"A pending history recovery was discarded because another world-changing command was executed.", userSession.Player);
		}

		private void ScheduleRoomRecalculation(IEnumerable<CommandScope> scopes)
		{
			foreach (CommandScope scope in scopes)
			{
				CommandChangeSet changeSet = scope.Changes;
				if (changeSet.ChangedBlocks <= 0) continue;
				RoomData.QueuePositionsTest(changeSet.AffectedPositions);
			}
		}

		public bool TryCancelCommand(User user)
		{
			ArgumentNullException.ThrowIfNull(user);
			return this._activeCommands.TryGetValue(user, out CommandInvocation? invocation) && invocation.RequestCancellation();
		}

		private bool TryBeginCommand(User user, CommandInvocation invocation)
		{
			ArgumentNullException.ThrowIfNull(user);
			ArgumentNullException.ThrowIfNull(invocation);
			return this._activeCommands.TryAdd(user, invocation);
		}

		private void EndCommand(User user, CommandInvocation invocation)
		{
			ArgumentNullException.ThrowIfNull(invocation);
			if (this._activeCommands.TryGetValue(user, out CommandInvocation? current) && ReferenceEquals(current, invocation) && this._activeCommands.TryRemove(new KeyValuePair<User, CommandInvocation>(user, invocation)))
			{
				invocation.Dispose();
			}
		}
	}
}

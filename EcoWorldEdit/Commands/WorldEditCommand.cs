using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Eco.Mods.WorldEdit.Commands
{
	using World = Eco.World.World;

	internal abstract class WorldEditCommand : IWorldEditCommand
	{
		protected UserSession UserSession { get; private set; }
		protected WorldRange Selection { get; private set; }
		public long BlocksChanged { get; protected set; }
		protected Stack<WorldEditBlock> AffectedBlocks
		{
			get
			{
				this.affectedBlocks ??= new Stack<WorldEditBlock>();
				return this.affectedBlocks;
			}
		}
		private Stack<WorldEditBlock> affectedBlocks = null;
		private readonly Stopwatch timer = new Stopwatch();
		public TimeSpan Elapsed => this.timer.Elapsed;
		public long ElapsedMilliseconds => this.timer.ElapsedMilliseconds;
		public long ElapsedTicks => this.timer.ElapsedTicks;
		public bool IsRunning => this.timer.IsRunning;
		public bool PerformingUndo { get; internal set; } = false;

		public WorldEditCommand(User user)
		{
			ArgumentNullException.ThrowIfNull(user);
			this.UserSession = WorldEditManager.GetUserSession(user);
			if (this.UserSession.ExecutingCommand != null && this.UserSession.ExecutingCommand.IsRunning) throw new WorldEditCommandException("Another command still executing"); //TODO: Probably need to rework that and impliment aborting
		}

		public bool Invoke() => this.Invoke(this.UserSession.Selection);
		public bool Invoke(WorldRange selection)
		{
			this.Selection = selection;
			bool result = false;
			try
			{
				this.UserSession.ExecutingCommand = this;
				this.timer.Start();
				this.Execute();
				this.timer.Stop();
				if (this.affectedBlocks != null)
				{
					this.UserSession.ExecutedCommands.Push(this);
				}
				result = true;
			}
			catch (WorldEditCommandException e)
			{
				this.UserSession.Player.ErrorLocStr(e.Message);
			}
			finally
			{
				if (this.timer.IsRunning) this.timer.Stop();
				this.UserSession.ExecutingCommand = null;
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddBlockChangedEntry(Vector3i position)
		{
			Block block = World.GetBlock(position);
			IEnumerable<WorldEditBlock> worldEditBlocks = WorldEditBlock.Create(block, position);
			foreach (WorldEditBlock worldEditBlock in worldEditBlocks)
			{
				this.AffectedBlocks.Push(worldEditBlock);
			}
		}

		public bool Undo()
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);
			bool result = false;
			try
			{
				this.UserSession.ExecutingCommand = this;
				this.PerformingUndo = true;
				this.timer.Restart();
				if (this.affectedBlocks != null)
				{
					this.AffectedBlocks.Where(b => !b.IsPlantBlock() || !b.IsWorldObjectBlock()).ForEach(b =>
					{
						blockManager.RestoreBlock(b, b.Position);
					});
					this.AffectedBlocks.Where(b => b.IsPlantBlock() || b.IsWorldObjectBlock()).ForEach(b =>
					{
						blockManager.RestoreBlock(b, b.Position);
					});
					result = true;
				}
				this.timer.Stop();
				this.UserSession.UndoneCommands.Push(this);
			}
			catch (WorldEditCommandException e)
			{
				this.UserSession.Player.ErrorLocStr(e.Message);
			}
			finally
			{
				if (this.timer.IsRunning) this.timer.Stop();
				this.PerformingUndo = false;
				this.UserSession.ExecutingCommand = null;
			}
			return result;
		}

		public bool Redo() => this.Invoke(this.Selection);

		protected abstract void Execute(WorldRange selection);
		protected void Execute() => this.Execute(this.Selection);
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
	using World = Eco.World.World;

	internal abstract class WorldEditCommand : IWorldEditCommand
	{
		protected UserSession UserSession { get; private set; }
		public long BlocksChanged { get; protected set; }
		protected Stack<WorldEditBlock> AffectedBlocks
		{
			get
			{
				if (this.affectedBlocks == null) { this.affectedBlocks = new Stack<WorldEditBlock>(); }
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
			if (user == null) throw new ArgumentNullException(nameof(user));
			this.UserSession = WorldEditManager.GetUserSession(user);
			if (this.UserSession.ExecutingCommand != null && this.UserSession.ExecutingCommand.IsRunning) throw new WorldEditCommandException("Another command still executing"); //TODO: Probably need to rework that and impliment aborting
		}

		public bool Invoke() => this.Invoke(this.UserSession.Selection);
		public bool Invoke(WorldRange selection)
		{
			bool result = false;
			try
			{
				this.UserSession.ExecutingCommand = this;
				this.timer.Start();
				this.Execute(selection);
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
			WorldEditBlock worldEditBlock = WorldEditBlock.Create(block, position);
			this.AffectedBlocks.Push(worldEditBlock);
		}

		public bool Undo()
		{
			WorldEditBlockManager blockManager = new WorldEditBlockManager(this.UserSession);
			bool result = false;
			try
			{
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
			}
			catch (WorldEditCommandException e)
			{
				this.UserSession.Player.ErrorLocStr(e.Message);
			}
			finally
			{
				if (this.timer.IsRunning) this.timer.Stop();
				this.PerformingUndo = false;
			}
			return result;
		}

		protected abstract void Execute(WorldRange selection);
	}
}

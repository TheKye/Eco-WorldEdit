using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Model;
using Eco.Shared.Math;
using Eco.World;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands
{
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

		public WorldEditCommand(User user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			this.UserSession = WorldEditManager.GetUserSession(user);
		}

		public bool Invoke()
		{
			bool result = false;
			try
			{
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
				if (this.timer.IsRunning)
				{
					this.timer.Stop();
				}
			}
			return result;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddBlockChangedEntry(Vector3i position)
		{
			Block block = Eco.World.World.GetBlock(position);
			this.AffectedBlocks.Push(WorldEditBlock.Create(block, position));
		}

		public bool Undo()
		{
			bool result = false;
			try
			{
				this.timer.Restart();
				if (this.affectedBlocks != null)
				{
					foreach (WorldEditBlock entry in this.AffectedBlocks)
					{
						WorldEditBlockManager.RestoreBlock(entry, entry.Position, this.UserSession.Player);
					}
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
				if (this.timer.IsRunning)
				{
					this.timer.Stop();
				}
			}
			return result;
		}

		protected abstract void Execute();
	}
}

using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.WorldEdit.Commands.General;
using Eco.Mods.WorldEdit.Core.Commands;
using Eco.Mods.WorldEdit.Utils.Eco;
using Eco.Mods.WorldEdit.Utils.Exceptions;
using Eco.Shared.Math;
using Eco.World.Blocks;

namespace Eco.Mods.WorldEdit.Commands.Blocks
{
	[ChatCommandHandler]
	internal sealed class SphereCommand(int Radius, Type BlockType, SphereCommand.SphereStyle Style, bool Hollow, bool Clear, Vector3i Origin) : IWorldEditCommand
	{
		public enum SphereStyle { Full, HalfTop, HalfBottom }

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Builds a sphere at your position.", shortCut: "sphere", level: ChatAuthorizationLevel.Admin)]
		public static void Sphere(User user, int radius, string? blockType = null, string styleStr = "full", bool hollow = false, bool clear = true)
		{
			try
			{
				string requestedType = blockType ?? nameof(DirtBlock);
				Type type = BlockUtils.GetBlockType(requestedType) ?? throw new WorldEditCommandException($"No BlockType with name {requestedType} found!");
				if (!Enum.TryParse(styleStr, true, out SphereStyle style)) style = SphereStyle.Full;
				SphereCommand command = new(Math.Max(1, radius), type, style, hollow, clear, CommandParsing.GetPosition(user));
				CommandResult result = CommandDispatcher.Obj.Execute(user, command);
				if (result.Result.Success) Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			WorldRange area = WorldRange.SurroundingSpace(Origin, Radius);
			if (Style == SphereStyle.HalfTop) area.min.y = Origin.y;
			else if (Style == SphereStyle.HalfBottom) area.max.y = Origin.y;
			area = area.FixToWorldSize();

			long outerRadiusSquared = (long)Radius * Radius;
			long innerRadiusSquared = (long)(Radius - 1) * (Radius - 1);
			Vector3i worldSize = Shared.Voxel.World.VoxelSize;

			foreach (Vector3i position in area.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				if (Clear) context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);

				long x = WrappedDistance(position.x, Origin.x, worldSize.x);
				long y = position.y - Origin.y;
				long z = WrappedDistance(position.z, Origin.z, worldSize.z);
				long distanceSquared = (x * x) + (y * y) + (z * z);
				if (distanceSquared > outerRadiusSquared || (Hollow && distanceSquared <= innerRadiusSquared)) continue;

				context.BlockManager.TryScheduleBlock(BlockType, position, ct);
			}

			context.BlockManager.CommitBatch(ct);
		}

		private static int WrappedDistance(int position, int origin, int worldSize)
		{
			int distance = Math.Abs(position - origin);
			return worldSize > 0 ? Math.Min(distance, worldSize - distance) : distance;
		}
	}
}

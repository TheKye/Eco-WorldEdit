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
	internal sealed class CylinderCommand(int Radius, int Height, Type BlockType, CylinderCommand.CylinderStyle Style, bool Hollow, bool Clear, Vector3i Origin) : IWorldEditCommand
	{
		public enum CylinderStyle { Cylinder, Square }

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Builds a cylinder at your position.", shortCut: "cylinder", level: ChatAuthorizationLevel.Admin)]
		public static void Cylinder(User user, int radius, int height, string? blockType = null, string styleStr = "cylinder", bool hollow = false, bool clear = true)
		{
			try
			{
				string requestedType = blockType ?? nameof(DirtBlock);
				Type type = BlockUtils.GetBlockType(requestedType) ?? throw new WorldEditCommandException($"No BlockType with name {requestedType} found!");
				if (!Enum.TryParse(styleStr, true, out CylinderStyle style)) style = CylinderStyle.Cylinder;
				CylinderCommand command = new(Math.Max(1, radius), Math.Max(1, height), type, style, hollow, clear, CommandParsing.GetPosition(user));
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
			area.min.y = Origin.y;
			area.max.y = Origin.y + Height - 1;
			area = area.FixToWorldSize();

			long outerRadiusSquared = (long)Radius * Radius;
			long innerRadiusSquared = (long)(Radius - 1) * (Radius - 1);
			Vector3i worldSize = Shared.Voxel.World.VoxelSize;

			foreach (Vector3i position in area.XYZIterInc())
			{
				ct.ThrowIfCancellationRequested();
				if (Clear) context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);

				long x = WrappedDistance(position.x, Origin.x, worldSize.x);
				long z = WrappedDistance(position.z, Origin.z, worldSize.z);
				long distanceSquared = (x * x) + (z * z);
				bool insideFootprint = Style == CylinderStyle.Square ? x <= Radius && z <= Radius : distanceSquared <= outerRadiusSquared;
				if (!insideFootprint) continue;

				bool isCap = position.y == area.min.y || position.y == area.max.y;
				bool isWall = Style == CylinderStyle.Square ? x == Radius || z == Radius : distanceSquared > innerRadiusSquared;
				if (Hollow && !isCap && !isWall) continue;

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

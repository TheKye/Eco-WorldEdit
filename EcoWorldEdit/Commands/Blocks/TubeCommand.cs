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
	internal sealed class TubeCommand(int Radius, int Amount, Direction Direction, Type BlockType, TubeCommand.TubeStyle Style, bool Hollow, bool Clear, Vector3i Origin) : IWorldEditCommand
	{
		public enum TubeStyle { Cylinder, Square }

		[ChatSubCommand(nameof(WorldEditCommand.WorldEdit), helpText: "Builds a horizontal tube from your position in the specified direction.", shortCut: "tube", level: ChatAuthorizationLevel.Admin)]
		public static void Tube(User user, int radius, string directionAndAmount = "1", string? blockType = null, string styleStr = "cylinder", bool hollow = false, bool clear = true)
		{
			try
			{
				string requestedType = blockType ?? nameof(DirtBlock);
				Type type = BlockUtils.GetBlockType(requestedType) ?? throw new WorldEditCommandException($"No BlockType with name {requestedType} found!");
				(Direction direction, int amount) = CommandParsing.ParseDirectionAndAmount(user, directionAndAmount);
				if (direction is Direction.Unknown or Direction.None) throw new WorldEditCommandException("Unable to determine direction.");
				if (direction is Direction.Up or Direction.Down) throw new WorldEditCommandException("Tube only supports horizontal directions. Use cylinder command for vertical structures.");
				if (!Enum.TryParse(styleStr, true, out TubeStyle style)) style = TubeStyle.Cylinder;
				TubeCommand command = new(Math.Max(1, radius), Math.Max(1, amount), direction, type, style, hollow, clear, CommandParsing.GetPosition(user));
				CommandResult result = CommandDispatcher.Obj.Execute(user, command);
				if (result.Result.Success) Logging.Success($"{result.BlocksChanged} blocks changed in {result.Elapsed.TotalMilliseconds}ms.", user.Player);
				else Logging.Error(result.Result.Message, user.Player);
			}
			catch (WorldEditCommandException exception) { Logging.ErrorLocStr(exception.Message, user.Player); }
			catch (Exception exception) { Logging.Exception(exception, user.Player); }
		}

		public void Execute(CommandContext context, CancellationToken ct)
		{
			Vector3i axis = Direction.ToVec();
			(Vector3i firstCrossAxis, Vector3i secondCrossAxis) = GetCrossAxes(axis);
			long outerRadiusSquared = (long)Radius * Radius;
			long innerRadiusSquared = (long)(Radius - 1) * (Radius - 1);

			for (int offset = 0; offset < Amount; offset++)
			{
				bool isCap = offset == 0 || offset == Amount - 1;
				for (int first = -Radius; first <= Radius; first++)
				{
					for (int second = -Radius; second <= Radius; second++)
					{
						ct.ThrowIfCancellationRequested();
						Vector3i position = Origin + (axis * offset) + (firstCrossAxis * first) + (secondCrossAxis * second);
						position = WrapHorizontalPosition(position);
						if (!WorldHeight.IsValid(position.y)) continue;

						if (Clear) context.BlockManager.TryScheduleBlock(typeof(EmptyBlock), position, ct);

						long distanceSquared = ((long)first * first) + ((long)second * second);
						bool insideCrossSection = Style == TubeStyle.Square || distanceSquared <= outerRadiusSquared;
						if (!insideCrossSection) continue;

						bool isWall = Style == TubeStyle.Square ? Math.Abs(first) == Radius || Math.Abs(second) == Radius : distanceSquared > innerRadiusSquared;
						if (Hollow && !isCap && !isWall) continue;

						context.BlockManager.TryScheduleBlock(BlockType, position, ct);
					}
				}
			}

			context.BlockManager.CommitBatch(ct);
		}

		private static (Vector3i First, Vector3i Second) GetCrossAxes(Vector3i axis)
		{
			if (axis.x != 0) return (new Vector3i(0, 1, 0), new Vector3i(0, 0, 1));
			return (new Vector3i(1, 0, 0), new Vector3i(0, 1, 0));
		}

		private static Vector3i WrapHorizontalPosition(Vector3i position)
		{
			Vector3i worldSize = Shared.Voxel.World.VoxelSize;
			position.x = WrapCoordinate(position.x, worldSize.x);
			position.z = WrapCoordinate(position.z, worldSize.z);
			return position;
		}

		private static int WrapCoordinate(int coordinate, int worldSize) => worldSize > 0 ? ((coordinate % worldSize) + worldSize) % worldSize : coordinate;
	}
}

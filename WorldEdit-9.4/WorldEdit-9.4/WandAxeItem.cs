namespace Eco.Mods.WorldEdit
{
	using System;
	using System.ComponentModel;
	using Eco.Core.Utils;
	using Eco.Core.Utils.AtomicAction;
	using Eco.Gameplay.DynamicValues;
	using Eco.Gameplay.Interactions;
	using Eco.Gameplay.Items;
	using Eco.Gameplay.Players;
	using Eco.Shared.Items;
	using Eco.Shared.Localization;
	using Eco.Shared.Math;
	using Eco.Shared.Serialization;
	using Eco.Shared.Utils;

	[Serialized]
	[LocDisplayName("Wand Tool")]
	[Category("Hidden")]
	public partial class WandAxeItem : ToolItem
	{
		private static IDynamicValue skilledRepairCost = new ConstantValue(1);

		public override LocString DisplayDescription { get { return Localizer.DoStr("Does magical World Edit things"); } }

		public override LocString LeftActionDescription { get { return Localizer.DoStr(""); } }

		public override ClientPredictedBlockAction LeftAction { get { return ClientPredictedBlockAction.None; } }

		public override IDynamicValue SkilledRepairCost => skilledRepairCost;

		public override InteractResult OnActLeft(InteractionContext context)
		{
			try
			{
				if (context.BlockPosition == null || !context.BlockPosition.HasValue)
					return InteractResult.Success;

				var pos = context.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(context.Player.User);
				userSession.SetFirstPosition(pos);

				context.Player.MsgLoc($"First Position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));

			}
			return InteractResult.Success;
		}

		public override InteractResult OnActRight(InteractionContext context)
		{
			try
			{
				if (context.BlockPosition == null || !context.BlockPosition.HasValue)
					return InteractResult.Success;

				var pos = context.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(context.Player.User);
				userSession.SetSecondPosition(pos);

				context.Player.MsgLoc($"Second Position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
			return InteractResult.NoOp;
		}

		public override bool ShouldHighlight(Type block)
		{
			return true;
		}

		protected Result PlayerPlaceBlock(Type blockType, Vector3i blockPosition, Player player, bool replaceBlock, float calorieMultiplier = 1, params IAtomicAction[] additionalActions)
		{
			return Result.Succeeded;
		}

		protected Result PlayerPlaceBlock<T>(Vector3i blockPosition, Player player, bool replaceBlock, float calorieMultiplier = 1, params IAtomicAction[] additionalActions)
		{
			return Result.Succeeded;
		}

		protected Result PlayerDeleteBlock(Vector3i blockPosition, Player player, bool addToInventory, float calorieMultiplier = 1, BlockItem fallbackGiveItem = null, params IAtomicAction[] additionalActions)
		{
			return Result.Succeeded;
		}

		protected void BurnCalories(Player player, float calorieMultiplier = 1)
		{
		}
	}
}

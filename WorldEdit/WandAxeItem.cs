namespace Eco.Mods.WorldEdit
{
	using System;
	using System.ComponentModel;
	using Eco.Core.Utils;
	using Eco.Core.Utils.AtomicAction;
	using Eco.Gameplay.DynamicValues;
	using Eco.Gameplay.Interactions.Interactors;
	using Eco.Gameplay.Items;
	using Eco.Gameplay.Players;
	using Eco.Shared.Localization;
	using Eco.Shared.Math;
	using Eco.Shared.Serialization;
    using Eco.Shared.SharedTypes;
    using Eco.Shared.Utils;

    [Serialized]
	[LocDisplayName("Wand Tool")]
	[Category("Hidden")]
	public class WandAxeItem : ToolItem, IInteractor
	{
		public override LocString DisplayDescription { get { return Localizer.DoStr("Does magical World Edit things"); } }

		public override IDynamicValue SkilledRepairCost => skilledRepairCost;
		private static IDynamicValue skilledRepairCost = new ConstantValue(1);

        [Interaction(InteractionTrigger.LeftClick, overrideDescription: "Set First Position", tags: BlockTags.Block)]
        public bool OnActLeft(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
		{
			try
			{
				if (!target.IsBlock) { return false; }
				if (target.BlockPosition is null || !target.BlockPosition.HasValue) { return false; }

				Vector3i pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(player.User);
				userSession.SetFirstPosition(pos);

                player.MsgLoc($"First Position set to ({pos.x}, {pos.y}, {pos.z})");
				return true;
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
			return false;
		}

        [Interaction(InteractionTrigger.RightClick, overrideDescription: "Set Second Position", tags: BlockTags.Block)]
        public bool OnActRight(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
		{
			try
			{
                if (!target.IsBlock) { return false; }
                if (target.BlockPosition is null || !target.BlockPosition.HasValue) { return false; }

                Vector3i pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(player.User);
				userSession.SetSecondPosition(pos);

                player.MsgLoc($"Second Position set to ({pos.x}, {pos.y}, {pos.z})");
                return true;
            }
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
			return false;
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

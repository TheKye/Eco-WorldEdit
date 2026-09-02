using System.ComponentModel;
using Eco.Core.Items;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.WorldEdit.Core;
using Eco.Mods.WorldEdit.Core.Managers;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Serialization;
using Eco.Shared.SharedTypes;

namespace Eco.Mods.WorldEdit
{
	[Serialized]
	[LocDisplayName("Wand Tool")]
	[LocDescription("Does magical World Edit things")]
	[Category("Hidden")]
	[Tag(nameof(SurfaceTags.CanBeOnSurface), Unset = true)]
	// ToolItem adds CanBeOnSurface through an inherited tag.
	// It must be explicitly unset because the wand has no placeable 3D model.
	// Item + IInteractor cannot currently be used because the Eco client does not activate its block interactions.
	public class WandToolItem : ToolItem, IInteractor
	{
		public override bool Decays => false; //Don't use durability for this tool.

		private static IDynamicValue skilledRepairCost = new ConstantValue(4);
		public override IDynamicValue SkilledRepairCost { get { return skilledRepairCost; } }

		//public override ItemCategory ItemCategory => ItemCategory.Devtool;

		[Interaction(InteractionTrigger.LeftClick, overrideDescription: "Set First Position", animationDriven: false, interactionDistance: 15, authRequired: AccessType.None,
			tags: new[] { BlockTags.Constructed, BlockTags.CanBeRoad, BlockTags.Road, BlockTags.Minable, BlockTags.Tillable, BlockTags.Diggable, BlockTags.Excavatable, BlockTags.Tilled, BlockTags.Liquid })]
		public void SetFirstPos(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
		{
			try
			{
				if (!target.IsBlock) return;
				if (target.BlockPosition is null || !target.BlockPosition.HasValue) return;

				Vector3i pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.Obj.GetUserSession(player.User);
				userSession.SetFirstPosition(pos);

				Logging.Success($"First position set to ({pos.x}, {pos.y}, {pos.z})", player);
			}
			catch (Exception e)
			{
				Logging.Exception(e, player);
			}
		}

		[Interaction(InteractionTrigger.RightClick, overrideDescription: "Set Second Position", animationDriven: false, interactionDistance: 15, authRequired: AccessType.None,
			tags: new[] { BlockTags.Constructed, BlockTags.CanBeRoad, BlockTags.Road, BlockTags.Minable, BlockTags.Tillable, BlockTags.Diggable, BlockTags.Excavatable, BlockTags.Tilled, BlockTags.Liquid })]
		public void SetSecondPos(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
		{
			try
			{
				if (!target.IsBlock) return;
				if (target.BlockPosition is null || !target.BlockPosition.HasValue) return;

				Vector3i pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.Obj.GetUserSession(player.User);
				userSession.SetSecondPosition(pos);

				Logging.Success($"Second position set to ({pos.x}, {pos.y}, {pos.z})", player);
			}
			catch (Exception e)
			{
				Logging.Exception(e, player);
			}
		}

		public static ItemStack GetWandItemStack()
		{
			Item item = Item.Get(nameof(WandToolItem));
			return new ItemStack(item, 1);
		}
	}
}

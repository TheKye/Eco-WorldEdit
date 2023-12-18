namespace Eco.Mods.WorldEdit
{
	using System;
	using System.ComponentModel;
	using Eco.Gameplay.DynamicValues;
	using Eco.Gameplay.Interactions.Interactors;
	using Eco.Gameplay.Items;
	using Eco.Gameplay.Players;
	using Eco.Mods.TechTree;
	using Eco.Shared.Items;
	using Eco.Shared.Localization;
	using Eco.Shared.Math;
	using Eco.Shared.Serialization;
	using Eco.Shared.SharedTypes;
	using Eco.Shared.Utils;

	[Serialized]
	[LocDisplayName("Wand Tool")]
	[LocDescription("Does magical World Edit things")]
	[Category("Hidden")]
	public class WandAxeItem : HammerItem, IInteractor
	{
		public override float DurabilityRate { get { return 0; } }
		public override IDynamicValue SkilledRepairCost => skilledRepairCost;
		private static IDynamicValue skilledRepairCost = new ConstantValue(1);
		public override ItemCategory ItemCategory => ItemCategory.Devtool;
		public override LocString GetNoSuitablePickupTargetFailureMessage(Inventory inventory) => Localizer.DoStr("");

		public override bool IsValidForInteraction(Item item)
		{
			BlockItem blockItem = item as BlockItem;
			return blockItem.BlockTypes.Count() >= 1;
		}

		[Interaction(InteractionTrigger.LeftClick, overrideDescription: "Set First Position", flags: InteractionFlags.BlocksOtherInteraction, authRequired: AccessType.None)]
		public void SetFirstPos(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
		{
			try
			{
				if (!target.IsBlock) { return; }
				if (target.BlockPosition is null || !target.BlockPosition.HasValue) { return; }
				var pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(player.User);
				userSession.SetFirstPosition(pos);

				player.MsgLoc($"First Position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

		[Interaction(InteractionTrigger.RightClick, overrideDescription: "Set Second Position", flags: InteractionFlags.BlocksOtherInteraction, authRequired: AccessType.None)]
		public void SetSecondPos(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
		{
			try
			{
				if (!target.IsBlock) { return; }
				if (target.BlockPosition is null || !target.BlockPosition.HasValue) { return; }
				var pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(player.User);
				userSession.SetSecondPosition(pos);

				player.MsgLoc($"Second Position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}
	}
}

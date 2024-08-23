namespace Eco.Mods.WorldEdit
{
	using System;
	using System.ComponentModel;
	using Eco.Gameplay.DynamicValues;
	using Eco.Gameplay.Interactions.Interactors;
	using Eco.Gameplay.Items;
	using Eco.Gameplay.Players;
	using Eco.Shared.Items;
	using Eco.Shared.Localization;
	using Eco.Shared.Math;
	using Eco.Shared.Serialization;
	using Eco.Shared.SharedTypes;
	using Eco.Shared.Logging;

	[Serialized]
	[LocDisplayName("Wand Tool")]
	[LocDescription("Does magical World Edit things")]
	[Category("Hidden")]
	public class WandToolItem : ToolItem, IInteractor
    {
        //public override float DurabilityRate { get { return 0; } }
        public override bool Decays => false; //Don't use durability for this tool.

        //public override IDynamicValue SkilledRepairCost => skilledRepairCost;
		//private static IDynamicValue skilledRepairCost = new ConstantValue(1);

        private static IDynamicValue skilledRepairCost = new ConstantValue(4);
        public override IDynamicValue SkilledRepairCost { get { return skilledRepairCost; } }

        //public override ItemCategory ItemCategory => ItemCategory.Devtool;

        [Interaction(InteractionTrigger.LeftClick, overrideDescription: "Set First Position", animationDriven: false, interactionDistance: 15, authRequired: AccessType.None)]
        public void SetFirstPos(Player player, InteractionTriggerInfo trigger, InteractionTarget target)
		{
			try
			{
				if (!target.IsBlock) { return; }
				if (target.BlockPosition is null || !target.BlockPosition.HasValue) { return; }

				Vector3i pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(player.User);
				userSession.SetFirstPosition(pos);

                player.MsgLoc($"First position set to ({pos.x}, {pos.y}, {pos.z})");
			}
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}

        [Interaction(InteractionTrigger.RightClick, overrideDescription: "Set Second Position", animationDriven: false, interactionDistance: 15, authRequired: AccessType.None)]
        public void SetSecondPos(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
		{
            try
			{
                if (!target.IsBlock) { return; }
                if (target.BlockPosition is null || !target.BlockPosition.HasValue) { return; }

                Vector3i pos = target.BlockPosition.Value;

				pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
				pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

				pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
				pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

				UserSession userSession = WorldEditManager.GetUserSession(player.User);
				userSession.SetSecondPosition(pos);

                player.MsgLoc($"Second position set to ({pos.x}, {pos.y}, {pos.z})");
            }
			catch (Exception e)
			{
				Log.WriteError(Localizer.Do($"{e}"));
			}
		}
	}
}

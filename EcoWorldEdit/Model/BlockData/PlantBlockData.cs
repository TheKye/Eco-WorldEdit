using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.BlockData
{
	[BlockDataContract(BlockDataType.Plant)]
	internal sealed record PlantBlockData : IBlockData
	{
		public Type PlantType { get; private set; }
		public float YieldPercent { get; private set; }
		public bool Dead { get; private set; }
		public int DeadType { get; private set; }
		public double DeathTime { get; private set; }
		public float GrowthPercent { get; private set; }
		public bool Tended { get; private set; }

		[JsonConstructor]
		public PlantBlockData(Type plantType, float yieldPercent, bool dead, int deadType, double deathTime, float growthPercent, bool tended)
		{
			this.PlantType = plantType ?? throw new ArgumentNullException(nameof(plantType));
			this.YieldPercent = yieldPercent;
			this.Dead = dead;
			this.DeadType = deadType;
			this.DeathTime = deathTime;
			this.GrowthPercent = growthPercent;
			this.Tended = tended;
		}
	}
}

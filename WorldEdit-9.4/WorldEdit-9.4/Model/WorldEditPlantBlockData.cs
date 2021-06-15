using System;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model
{
	internal struct WorldEditPlantBlockData : IWorldEditBlockData
	{
		public Type PlantType { get; private set; }
		public float YieldPercent { get; private set; }
		public bool Dead { get; private set; }
		public DeathType DeadType { get; private set; }
		public double DeathTime { get; private set; }
		public float GrowthPercent { get; private set; }
		public bool Tended { get; private set; }

		public static WorldEditPlantBlockData From(Plant plant)
		{
			WorldEditPlantBlockData plantBlockData = new WorldEditPlantBlockData
			{
				PlantType = plant.GetType(),
				YieldPercent = plant.YieldPercent,
				Dead = plant.Dead,
				DeadType = plant.DeadType,
				DeathTime = plant.DeathTime,
				GrowthPercent = plant.GrowthPercent,
				Tended = plant.Tended
			};
			return plantBlockData;
		}

		[JsonConstructor]
		public WorldEditPlantBlockData(Type plantType, float yieldPercent, bool dead, DeathType deadType, double deathTime, float growthPercent, bool tended)
		{
			this.PlantType = plantType;
			this.YieldPercent = yieldPercent;
			this.Dead = dead;
			this.DeadType = deadType;
			this.DeathTime = deathTime;
			this.GrowthPercent = growthPercent;
			this.Tended = tended;
		}

		public IWorldEditBlockData Clone()
		{
			return new WorldEditPlantBlockData(this.PlantType, this.YieldPercent, this.Dead, this.DeadType, this.DeathTime, this.GrowthPercent, this.Tended);
		}
	}
}

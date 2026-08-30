using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Model.Components
{
	[WorldObjectComponentContract(WorldObjectComponentType.CustomText)]
	internal sealed record CustomTextComponentData : IWorldObjectComponentData
	{
		public string Text { get; init; }

		[JsonConstructor]
		public CustomTextComponentData(string text)
		{
			this.Text = text;
		}
	}
}

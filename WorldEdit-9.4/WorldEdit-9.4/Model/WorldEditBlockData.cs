namespace Eco.Mods.WorldEdit.Model
{
	internal class WorldEditBlockData<T>
	{
		public string DataType { get { return typeof(T).FullName; } }
		public T Data { get; protected set; }
	}

	internal class WorldEditBlockData
	{
		public string DataType { get; protected set; }
		public object Data { get; protected set; }
	}
}

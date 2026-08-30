using System.Reflection;
using Eco.Mods.WorldEdit.Model;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit.Core.Managers
{
	internal sealed class DataTypeRegistry<TDiscriminator, TBase> where TDiscriminator : struct, Enum
	{
		private readonly Dictionary<TDiscriminator, Type> _types;
		private readonly Dictionary<Type, TDiscriminator> _discriminators;

		public DataTypeRegistry()
		{
			this._types = [];
			this._discriminators = [];

			Assembly assembly = typeof(TBase).Assembly;
			IEnumerable<Type> dataTypes = assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface && typeof(TBase).IsAssignableFrom(type));

			foreach (Type dataType in dataTypes)
			{
				IDataTypeContractAttribute<TDiscriminator>[] contracts = dataType.GetCustomAttributes(inherit: false).OfType<IDataTypeContractAttribute<TDiscriminator>>().ToArray();

				if (contracts.Length == 0) throw new InvalidOperationException($"{dataType.FullName} implements {typeof(TBase).FullName}, but has no data contract attribute.");
				if (contracts.Length > 1) throw new InvalidOperationException($"{dataType.FullName} has multiple data contract attributes.");

				TDiscriminator discriminator = contracts[0].Discriminator;

				if (!this._types.TryAdd(discriminator, dataType))
				{
					Type registeredType = this._types[discriminator];
					throw new InvalidOperationException($"Discriminator {discriminator} is already assigned to {registeredType.FullName}; unable to assign it to {dataType.FullName}.");
				}

				if (!this._discriminators.TryAdd(dataType, discriminator)) throw new InvalidOperationException($"{dataType.FullName} is already registered.");
			}
		}

		public Type GetType(TDiscriminator discriminator)
		{
			return this._types.TryGetValue(discriminator, out Type? type) ? type : throw new JsonSerializationException($"Unknown {typeof(TBase).Name} discriminator: {discriminator}.");
		}

		public TDiscriminator GetDiscriminator(Type type)
		{
			ArgumentNullException.ThrowIfNull(type);

			return this._discriminators.TryGetValue(type, out TDiscriminator discriminator) ? discriminator : throw new JsonSerializationException($"Type {type.FullName} is not registered as {typeof(TBase).Name}.");
		}
	}
}

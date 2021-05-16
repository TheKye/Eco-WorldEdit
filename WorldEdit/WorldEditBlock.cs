using Eco.Core.Serialization;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Shared.Math;
using Eco.Shared.Serialization;
using Eco.World;
using System;

namespace Eco.Mods.WorldEdit
{
    [Serialized]
    public struct WorldEditBlock
    {
        public static WorldEditBlock API { get; set; }

        public WorldEditBlock CreateNew(Block pBlock, Vector3i pPosition, Vector3i? pSourcePosition)
        {
            WorldEditBlock web = new WorldEditBlock();

            if (pBlock is WorldObjectBlock)
            {
                WorldObject wob = (WorldObjectBlock)pBlock;

                CreateNew(wob, pPosition, pSourcePosition);
            }
            else
            {
                web.Type = pBlock.GetType();
                web.Position = pPosition;
            }

            return web;
        }

        public WorldEditBlock CreateNew(WorldObject obj, Vector3i pPosition, Vector3i? pSourcePosition)
        {
            WorldEditBlock web = new WorldEditBlock
            {
                Type = obj.GetType(),
                Position = pPosition,
            };

            return web;
        }

        [Serialized] public Vector3i Position { get; set; }
        [Serialized] public byte[] Data { get; set; }
        [Serialized] public Type Type;

        public WorldEditBlock(Type pType, Vector3i pPosition, byte[] data) : this()
        {
            Type = pType;
            Position = pPosition;
            Data = data;
        }

        public WorldEditBlock Clone()
        {
            return new WorldEditBlock(Type, Position, Data);
        }
    }
}

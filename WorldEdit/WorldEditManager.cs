using Eco.Core.Serialization;
using Eco.EM.Framework.Utils;
using Eco.Gameplay.Components;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using Eco.World;
using Eco.World.Blocks;
using EcoWorldEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Eco.Mods.WorldEdit
{
    public class WorldEditManager
    {
        protected static int mDefaultWorldSaveFrequency;
        protected static bool mWorldSaveStopped = false;
        protected static object mLocker = new object();
        //  internal static SimpleSerializer mSimpleSerializer = new SimpleSerializer();

        protected static Dictionary<string, WorldEditUserData> mUserData = new Dictionary<string, WorldEditUserData>();


        public static ItemStack getWandItemStack()
        {
            Item item = Item.Get("WandAxeItem");
            return new ItemStack(item, 1);
        }

        public static WorldEditUserData GetUserData(string pUsername)
        {
            if (mUserData.Keys.Contains(pUsername))
                return mUserData[pUsername];

            WorldEditUserData weud = new WorldEditUserData();
            mUserData.Add(pUsername, weud);
            return weud;
        }

        public static void SetBlock(WorldEditBlock pBlock, UserSession pSession)
        {
            SetBlock(pBlock.Type, pBlock.Position, pSession, null, null, pBlock.Data);
        }

        public static void SetBlock(Type pType, Vector3i pPosition, UserSession pSession = null, Vector3i? pSourcePosition = null, Block pSourceBlock = null, byte[] pData = null)
        {


            if (pType == null || pType.DerivesFrom<PickupableBlock>())
                pType = typeof(EmptyBlock);

            if (pType.FullName == "Eco.Gameplay.Objects.WorldObjectBlock" || pType.FullName == "Eco.Gameplay.Objects.BuildingWorldObjectBlock")
                pType = typeof(EmptyBlock);

            if (World.World.GetBlock(pPosition) is WorldObjectBlock worldObjectBlock)
            {
                if (worldObjectBlock.WorldObjectHandle.Object.Position3i == pPosition)
                {
                    worldObjectBlock.WorldObjectHandle.Object.Destroy();
                }
            }

            if (pType == typeof(EmptyBlock))
            {
                World.World.DeleteBlock(pPosition);
                return;
            }

            var constuctor = pType.GetConstructor(Type.EmptyTypes);

            if (constuctor != null)
            {
                Eco.World.World.SetBlock(pType, pPosition);
                return;
            }

            Type[] types = new Type[1];
            types[0] = typeof(WorldPosition3i);

            constuctor = pType.GetConstructor(types);

            if (constuctor != null)
            {
                object obj = null;

                if (pData != null)
                {
                    MemoryStream ms = new MemoryStream(pData);
                    obj = EcoSerializer.Deserialize(ms);
                }

                if (pType.DerivesFrom<PlantBlock>())
                {
                    PlantSpecies ps = null;
                    Plant pb = null;

                    if (obj != null)
                    {
                        pb = (Plant)obj;
                        ps = pb.Species;
                    }
                    else
                    {
                        ps = WorldUtils.GetPlantSpecies(pType);

                        if (pSourceBlock != null)
                            pb = PlantBlock.GetPlant(pPosition);
                    }

                    Plant newplant = EcoSim.PlantSim.SpawnPlant(ps, pPosition);

                    if (pb != null)
                    {
                        newplant.YieldPercent = pb.YieldPercent;
                        newplant.Dead = pb.Dead;
                        newplant.DeadType = pb.DeadType;
                        newplant.GrowthPercent = pb.GrowthPercent;
                        newplant.DeathTime = pb.DeathTime;
                        newplant.Tended = pb.Tended;
                    }

                    return;
                }

                Log.WriteLine(Localizer.DoStr("Unknown Type: " + pType));
            }

            types[0] = typeof(WorldObject);
            constuctor = pType.GetConstructor(types);

            if (constuctor != null)
            {
                WorldObject wObject = null;

                if (pSourceBlock != null)
                {
                    wObject = ((WorldObjectBlock)pSourceBlock).WorldObjectHandle.Object;

                    //if this is not the "main block" of an object do nothing
                    if (wObject.Position3i != pSourcePosition.Value)
                        return;

                }
                else if (pData != null)
                {
                    MemoryStream ms = new MemoryStream(pData);
                    var obj = EcoSerializer.Deserialize(ms);

                    if (obj is WorldObject)
                    {
                        wObject = obj as WorldObject;
                    }
                    else
                        throw new InvalidOperationException("obj is not WorldObjectBlock");
                }

                if (wObject == null)
                    return;


                //    WorldObject newObject = WorldObjectUtil.Spawn(wObject.GetType().Name, wObject.Creator.User, pPosition);
                //    newObject.Rotation = wObject.Rotation;


                WorldObject newObject = (WorldObject)Activator.CreateInstance(wObject.GetType(), true);
                newObject = WorldObjectManager.TryPlaceWorldObject(wObject.Creator.Player, newObject.CreatingItem, pPosition, wObject.Rotation);


                {
                    StorageComponent newSC = newObject.GetComponent<StorageComponent>();

                    if (newSC != null)
                    {
                        StorageComponent oldPSC = wObject.GetComponent<StorageComponent>();
                        newSC.Inventory.AddItems(oldPSC.Inventory.Stacks);
                        //    newSC.Inventory.OnChanged.Invoke(null);
                    }

                }

                {
                    CustomTextComponent newTC = newObject.GetComponent<CustomTextComponent>();

                    if (newTC != null)
                    {
                        CustomTextComponent oldTC = wObject.GetComponent<CustomTextComponent>();

                        typeof(CustomTextComponent).GetProperty("Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newTC, oldTC.TextData);
                    }
                }

                return;
            }

            Log.WriteLine(Localizer.DoStr("Unknown Type: " + pType));
        }

        public static Direction GetDirectionAndAmount(User pUser, string pDirectionAndAmount, out int pAmount)
        {
            pAmount = 1;
            string[] splitted = pDirectionAndAmount.Split(' ', ',');

            if (!int.TryParse(splitted[0], out pAmount))
            {
                pUser.Player.ErrorLocStr($"Please provide an amount first");
                return Direction.Unknown;
            }

            return DirectionUtils.GetDirectionOrLooking(pUser, splitted.Length >= 2 ? splitted[1] : null);
        }

        public static class DirectionUtils
        {
            /// <summary>
            /// Get Looking Direction of User 
            /// <para>Can be Left, Right, Forward, Backward, Up, Down</para>
            /// </summary>
            public static Direction GetLookingDirection(User pUser)
            {
                float yDirection = pUser.Rotation.Forward.Y;

                if (yDirection > 0.85)
                    return Direction.Up;
                else if (yDirection < -0.85)
                    return Direction.Down;

                return pUser.FacingDir;
            }

            /// <summary>
            /// For example "Up" or "u" to  <see cref="Direction.Up"/>. <see cref="Direction.Unknown"/> if direction is unkown
            /// </summary>
            public static Direction GetDirection(string pDirection)
            {
                if (string.IsNullOrWhiteSpace(pDirection))
                    return Direction.Unknown;

                switch (pDirection.Trim().ToLower())
                {
                    case "up":
                    case "u":
                        return Direction.Up;

                    case "down":
                    case "d":
                        return Direction.Down;

                    case "left":
                    case "l":
                        return Direction.Left;

                    case "right":
                    case "r":
                        return Direction.Right;

                    case "back":
                    case "b":
                        return Direction.Back;

                    case "forward":
                    case "f":
                        return Direction.Forward;

                    default:
                        return Direction.Unknown;
                }
            }

            public static Direction GetDirectionOrLooking(User pUser, string pDirection)
            {
                if (string.IsNullOrWhiteSpace(pDirection))
                    return GetLookingDirection(pUser);

                return GetDirection(pDirection);
            }
        }
    }
}

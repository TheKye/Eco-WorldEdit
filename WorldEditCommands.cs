
using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World;
using Eco.World.Blocks;
using EcoWorldEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Eco.Mods.WorldEdit
{
    public class WorldEditCommands : IChatCommandHandler
    {
        const string version = "1.0.1";
        #region

        public static class BlockUtils
        {

            public static Type GetBlockType(string pBlockName)
            {
                pBlockName = pBlockName.ToLower();

                if (pBlockName == "air")
                    return typeof(EmptyBlock);

                Type blockType = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == pBlockName + "floorblock");

                if (blockType != null)
                    return blockType;

                blockType = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == pBlockName + "block");

                if (blockType != null)
                    return blockType;

                return BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == pBlockName + "block");
            }
        }
        #endregion

        [ChatCommand("/wand", "", ChatAuthorizationLevel.Admin)]
        public static void Wand(User user)
        {
            try
            {
                user.Inventory.AddItems(WorldEditManager.getWandItemStack());
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("/rmwand", "", ChatAuthorizationLevel.Admin)]
        public static void RmWand(User user)
        {
            try
            {
                user.Inventory.TryRemoveItems(WorldEditManager.getWandItemStack());
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("/set", "", ChatAuthorizationLevel.Admin)]
        public static void Set(User user, string pTypeName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both Points with the Wand Tool first!");
                    return;
                }

                Type blockType = BlockUtils.GetBlockType(pTypeName);

                if (blockType == null)
                {
                    user.Player.MsgLoc($"No BlockType with name {pTypeName} found!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                weud.StartEditingBlocks();

                long changedBlocks = 0;

                for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                        for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                        {
                            var pos = new Vector3i(x, y, z);
                            weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos), pos);
                            WorldEditManager.SetBlock(blockType, pos);
                            changedBlocks++;
                        }

                user.Player.MsgLoc($"{changedBlocks} blocks changed.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("Replace a block with another block", ChatAuthorizationLevel.Admin)]
        public static void Replace(User user, string pTypeNames, string toReplace)
        {
            try
            {
                toReplace = toReplace.Trim();
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
                    return;
                }

                Type blockTypeToFind = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == (pTypeNames + "block").ToLower());
                if (blockTypeToFind == null)
                {
                    user.Player.MsgLoc($"No BlockType with name {pTypeNames} found!");
                    return;
                }

                Type blockTypeToReplace = null;

                if (toReplace != string.Empty)
                {
                    blockTypeToReplace = BlockManager.BlockTypes.FirstOrDefault(t => t.Name.ToLower() == (toReplace + "block").ToLower());
                    if (blockTypeToReplace == null)
                    {
                        user.Player.MsgLoc($"No BlockType with name { toReplace } found!");
                        return;
                    }
                }

                var vectors = weud.GetSortedVectors();

                long changedBlocks = 0;


                //if toReplace is string empty we will replace everything except empty with toFind type

                weud.StartEditingBlocks();

                if (toReplace != string.Empty)
                    for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                        for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                            for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                            {
                                var pos = new Vector3i(x, y, z);
                                var block = Eco.World.World.GetBlock(pos);

                                if (block != null && block.GetType() == blockTypeToFind)
                                {
                                    weud.AddBlockChangedEntry(block, pos);
                                    WorldEditManager.SetBlock(blockTypeToReplace, pos);
                                    changedBlocks++;
                                }
                            }
                else
                    for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                        for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                            for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                            {
                                var pos = new Vector3i(x, y, z);
                                var block = Eco.World.World.GetBlock(pos);

                                if (block != null && block.GetType() != typeof(EmptyBlock))
                                {
                                    weud.AddBlockChangedEntry(block, pos);
                                    WorldEditManager.SetBlock(blockTypeToFind, pos);
                                    changedBlocks++;
                                }
                            }

                user.Player.MsgLoc($"{changedBlocks} blocks changed.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("Set the outside area as a specific wall", ChatAuthorizationLevel.Admin)]
        public static void Walls(User user, string pTypeName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both Points with the Wand Tool first!");
                    return;
                }
                pTypeName = pTypeName.Replace(" ", "");
                
                Type blockType = BlockManager.BlockTypes.FirstOrDefault(t => t.Name == (pTypeName + "block").ToLower()); 

                if (blockType == null)
                {
                    user.Player.MsgLoc($"No BlockType with name {pTypeName} found!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                weud.StartEditingBlocks();
                long changedBlocks = 0;

                for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                    {
                        var pos = new Vector3i(x, y, vectors.Lower.Z);
                        weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos), pos);
                        WorldEditManager.SetBlock(blockType, pos);
                        changedBlocks++;
                    }

                for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                    {
                        var pos = new Vector3i(x, y, vectors.Higher.Z - 1);
                        weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos), pos);
                        WorldEditManager.SetBlock(blockType, pos);
                        changedBlocks++;
                    }

                for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                    {
                        var pos = new Vector3i(vectors.Lower.X, y, z);
                        weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos), pos);
                        WorldEditManager.SetBlock(blockType, pos);
                        changedBlocks++;
                    }

                for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                    {
                        var pos = new Vector3i(vectors.Higher.X - 1, y, z);
                        weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos), pos);
                        WorldEditManager.SetBlock(blockType, pos);
                        changedBlocks++;
                    }

                //   int changedBlocks = (((vectors.Higher.X - vectors.Lower.X) * 2 + (vectors.Higher.Z - vectors.Lower.Z) * 2) - 4) * (vectors.Higher.Y - vectors.Lower.Y);

                if (changedBlocks == 0) //maybe better math?
                    changedBlocks = 1;

                user.Player.MsgLoc($"{ changedBlocks } blocks changed.");

            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Stack(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                Direction dir = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);

                weud.StartEditingBlocks();
                UserSession session = weud.GetNewSession();

                long changedBlocks = 0;

                for (int i = 1; i <= amount; i++)
                {
                    Vector3i offset = dir.ToVec() * (vectors.Higher - vectors.Lower) * i;

                    for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                        for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                            for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                            {
                                var pos = new Vector3i(x, y, z);

                                weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos + offset), pos + offset);
                                var sourceBlock = Eco.World.World.GetBlock(pos);
                                WorldEditManager.SetBlock(sourceBlock.GetType(), pos + offset, session, pos, sourceBlock);
                                changedBlocks++;
                            }
                }

                //   int changedBlocks = (int)((vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z)) * amount;

                user.Player.MsgLoc($"{changedBlocks} blocks changed.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Move(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                Direction dir = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);

                weud.StartEditingBlocks();

                UserSession session = weud.GetNewSession();

                Vector3i offset = dir.ToVec() * amount;

                //     if (dir == Direction.Up)
                //          offset *= vectors.Higher.Y - vectors.Lower.Y;

                long changedBlocks = 0;

                Action<int, int, int> action = (int x, int y, int z) =>
                  {
                      var pos = new Vector3i(x, y, z);

                      weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos), pos);
                      weud.AddBlockChangedEntry(Eco.World.World.GetBlock(pos + offset), pos + offset);

                      var sourceBlock = Eco.World.World.GetBlock(pos);
                      WorldEditManager.SetBlock(sourceBlock.GetType(), pos + offset, session, pos, sourceBlock);
                      WorldEditManager.SetBlock(typeof(EmptyBlock), pos, session);
                      changedBlocks++;
                  };


                if (dir == Direction.Left || dir == Direction.Back || dir == Direction.Down)
                {
                    for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                        for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                            for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                                action.Invoke(x, y, z);
                }
                else
                {
                    /*                for (int x = vectors.Higher.X - 1; x >= vectors.Lower.X; x--)
                                        for (int y = vectors.Higher.Y - 1; y >= vectors.Lower.Y; y--)
                                            for (int z = vectors.Higher.Z - 1; z >= vectors.Lower.Z; z--)*/

                    int x = vectors.Higher.X - 1;
                    if (x < 0)
                        x = x + Shared.Voxel.World.VoxelSize.X;

                    int startZ = vectors.Higher.Z - 1;
                    if (startZ < 0)
                        startZ = startZ + Shared.Voxel.World.VoxelSize.Z;

                    //           Console.WriteLine("--------------");
                    //           Console.WriteLine(vectors.Lower);
                    //            Console.WriteLine(vectors.Higher);

                    for (; x != (vectors.Lower.X - 1); x--)
                    {
                        if (x < 0)
                            x = x + Shared.Voxel.World.VoxelSize.X;
                        for (int y = vectors.Higher.Y - 1; y >= vectors.Lower.Y; y--)
                            for (int z = startZ; z != (vectors.Lower.Z - 1); z--)
                            {
                                if (z < 0)
                                    z = z + Shared.Voxel.World.VoxelSize.Z;

                                //               Console.WriteLine($"{x} {y} {z}");
                                action.Invoke(x, y, z);
                            }
                    }
                }

                // int changedBlocks = (int)((vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z)) * amount;

                user.Player.MsgLoc($"{changedBlocks} blocks moved.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Expand(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                Direction direction = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);
                if (direction == Direction.Unknown)
                    return;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.ExpandSelection(direction.ToVec() * amount))
                    user.Player.MsgLoc($"Expanded selection {amount} {direction}");
                else
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Contract(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                Direction direction = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);
                if (direction == Direction.Unknown)
                    return;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.ExpandSelection(direction.ToVec() * -amount, true))
                    user.Player.MsgLoc($"Contracted selection {amount} {direction}");
                else
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Shift(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                Direction direction = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);
                if (direction == Direction.Unknown)
                    return;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.ShiftSelection(direction.ToVec() * amount))
                    user.Player.MsgLoc($"Shifted selection {amount} {direction}");
                else
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Up(User user, int pCount = 1)
        {
            try
            {
                Vector3 pos = user.Player.Position;
                var newpos = new Vector3i((int)pos.X, (int)pos.Y + pCount, (int)pos.Z);
                WorldEditManager.SetBlock(typeof(StoneBlock), newpos);
                newpos.Y += 2;
                user.Player.SetPosition(newpos);
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Undo(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.Undo())
                    user.Player.MsgLoc($"Undo done.");
                else
                    user.Player.MsgLoc($"You can't use undo right now!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Copy(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.SaveSelectionToClipboard(user))
                    user.Player.MsgLoc($"Copy done.");
                else
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Paste(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.LoadSelectionFromClipboard(user, weud))
                    user.Player.MsgLoc($"Paste done.");
                else
                    user.Player.MsgLoc($"Please copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Rotate(User user, int pDegree = 90)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.RotateClipboard(pDegree))
                    user.Player.MsgLoc($"Rotation in clipboard done.");
                else
                    user.Player.MsgLoc($"Please copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Export(User user, string pFileName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.SaveClipboard(pFileName))
                    user.Player.MsgLoc($"Export done.");
                else
                    user.Player.MsgLoc($"Please //copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Import(User user, string pFileName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.LoadClipboard(pFileName))
                    user.Player.MsgLoc($"Import done. Use //paste");
                else
                    user.Player.MsgLoc($"Schematic file not found!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("", ChatAuthorizationLevel.Admin)]
        public static void Distr(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both points with the Wand Tool first!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                Dictionary<string, long> mBlocks = new Dictionary<string, long>();

                for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                        for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                        {
                            //                 Console.WriteLine($"{x} {y} {z}");
                            var pos = new Vector3i(x, y, z);
                            var block = Eco.World.World.GetBlock(pos).GetType().ToString();

                            long count;
                            mBlocks.TryGetValue(block, out count);
                            mBlocks[block] = count + 1;
                        }

                double amountBlocks = mBlocks.Values.Sum(); // (vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z);

                user.Player.MsgLoc($"total blocks: {amountBlocks}");

                foreach (var entry in mBlocks)
                {
                    string percent = (Math.Round((entry.Value / amountBlocks) * 100, 2)).ToString() + "%";
                    string nameOfBlock = entry.Key.Substring(entry.Key.LastIndexOf(".") + 1);
                    user.Player.MsgLoc($"{entry.Value.ToString().PadRight(6)} {percent.PadRight(6)} {nameOfBlock}");
                }
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("Set First Position To players position", ChatAuthorizationLevel.Admin)]
        public static void SetPos1(User user)
        {
            try
            {
                var pos = user.Position;

                pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
                pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

                pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
                pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Player.User.Name);
                weud.FirstPos = (Vector3i?)pos;

                user.Player.MsgLoc($"First Position set to ({pos.x}, {pos.y}, {pos.z})");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));

            }
        }

        [ChatCommand("Set Second Position To players position", ChatAuthorizationLevel.Admin)]
        public static void SetPos2(User user)
        {
            try
            {
                var pos = user.Position;

                pos.X = pos.X < 0 ? pos.X + Shared.Voxel.World.VoxelSize.X : pos.X;
                pos.Z = pos.Z < 0 ? pos.Z + Shared.Voxel.World.VoxelSize.Z : pos.Z;

                pos.X = pos.X % Shared.Voxel.World.VoxelSize.X;
                pos.Z = pos.Z % Shared.Voxel.World.VoxelSize.Z;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Player.User.Name);
                weud.SecondPos = (Vector3i?)pos;

                user.Player.MsgLoc($"Second Position set to ({pos.x}, {pos.y}, {pos.z})");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }

        [ChatCommand("World Edit Version - For Debugging", ChatAuthorizationLevel.Admin)]
        public static void WEVersion(User user)
        {
            user.Player.MsgLocStr($"World Edit - Beta Test Version:" + version);
        }

      /*  [ChatCommand("/grow", "", ChatAuthorizationLevel.Admin)]
        public static void Grow(User user, string pFileName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.MsgLoc($"Please set both Points with the Wand Tool first!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                for (int x = vectors.Lower.X; x < vectors.Higher.X; x++)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                        for (int z = vectors.Lower.Z; z < vectors.Higher.Z; z++)
                        {
                            var pos = new Vector3i(x, y, z);
                            var block = Eco.World.World.GetBlock(pos);

                            if (block is PlantBlock)
                            {
                                var pb = block as PlantBlock;
                                pb.GrowthPercent = 1;
                                pb.TryGet() = 1;
                                pb.Tick(pos);
                            }

                        }

            }
            catch (Exception e)
            {
               Log.WriteError(Localizer.Do($"{e}"));
            }
        }
      */
    }
}
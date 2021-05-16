using Eco.Gameplay.Objects;
using Eco.Gameplay.Plants;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Chat;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.Simulation.Agents;
using Eco.World;
using Eco.World.Blocks;
using EcoWorldEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Eco.EM.Framework.Utils;

namespace Eco.Mods.WorldEdit
{
    public class WorldEditCommands : IChatCommandHandler
    {
        [ChatCommand("Gives the player a Wand for using world edit", ChatAuthorizationLevel.Admin)]
        public static void Wand(User user)
        {
            try
            {
                user.Inventory.AddItems(WorldEditManager.getWandItemStack());
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Removes the wand from the players inventory", ChatAuthorizationLevel.Admin)]
        public static void RmWand(User user)
        {
            try
            {
                user.Inventory.TryRemoveItems(WorldEditManager.getWandItemStack());
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Sets The Selected Area to the desired Block", ChatAuthorizationLevel.Admin)]
        public static void Set(User user, string pTypeName)
        {
            try
            {
                pTypeName = pTypeName.Replace(" ", "");
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.ErrorLocStr($"Please set both Points with the Wand Tool first!");
                    return;
                }

                Type blockType = BlockUtils.GetBlockType(pTypeName);

                if (blockType == null)
                {
                    user.Player.ErrorLocStr($"No BlockType with name {pTypeName} found!");
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

                user.Player.ErrorLocStr($"{changedBlocks} blocks changed.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Replace a Specific Block Type with Another Block", ChatAuthorizationLevel.Admin)]
        public static void Replace(User user, string pTypeNames = "")
        {
            try
            {
                string[] splitted = pTypeNames.Trim().Split(',');
                string toFind = splitted[0].ToLower().Replace(" ", "");

                string toReplace = string.Empty;

                if (splitted.Length >= 2)
                    toReplace = splitted[1].Trim().ToLower().Replace(" ", "");

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
                    return;
                }

                Type blockTypeToFind = BlockUtils.GetBlockType(toFind);
                if (blockTypeToFind == null)
                {
                    user.Player.ErrorLocStr($"No BlockType with name {toFind} found!");
                    return;
                }

                Type blockTypeToReplace = null;

                if (toReplace != string.Empty)
                {
                    blockTypeToReplace = BlockUtils.GetBlockType(toReplace);
                    if (blockTypeToReplace == null)
                    {
                        user.Player.ErrorLocStr($"No BlockType with name { toReplace } found!");
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

                user.Player.ErrorLocStr($"{changedBlocks} blocks changed.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Sets the Area on the outside of the selection to selected wall type", ChatAuthorizationLevel.Admin)]
        public static void Walls(User user, string pTypeName)
        {
            try
            {
                pTypeName = pTypeName.Replace(" ", "");
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.ErrorLocStr($"Please set both Points with the Wand Tool first!");
                    return;
                }

                Type blockType = BlockUtils.GetBlockType(pTypeName);

                if (blockType == null)
                {
                    user.Player.ErrorLocStr($"No BlockType with name {pTypeName} found!");
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

                //    int changedBlocks = (((vectors.Higher.X - vectors.Lower.X) * 2 + (vectors.Higher.Z - vectors.Lower.Z) * 2) - 4) * (vectors.Higher.Y - vectors.Lower.Y);

                if (changedBlocks == 0) //maybe better math?
                    changedBlocks = 1;

                user.Player.ErrorLocStr($"{ changedBlocks } blocks changed.");

            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/stack", "", ChatAuthorizationLevel.Admin)]
        public static void Stack(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
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

                user.Player.ErrorLocStr($"{changedBlocks} blocks changed.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/move", "", ChatAuthorizationLevel.Admin)]
        public static void Move(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
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

                user.Player.ErrorLocStr($"{changedBlocks} blocks moved.");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/expand", "", ChatAuthorizationLevel.Admin)]
        public static void Expand(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                Direction direction = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);
                if (direction == Direction.Unknown)
                    return;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.ExpandSelection(direction.ToVec() * amount))
                    user.Player.ErrorLocStr($"Expanded selection {amount} {direction}");
                else
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/contract", "", ChatAuthorizationLevel.Admin)]
        public static void Contract(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                Direction direction = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);
                if (direction == Direction.Unknown)
                    return;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.ExpandSelection(direction.ToVec() * -amount, true))
                    user.Player.ErrorLocStr($"Contracted selection {amount} {direction}");
                else
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/shift", "", ChatAuthorizationLevel.Admin)]
        public static void Shift(User user, string pDirectionAndAmount = "1")
        {
            try
            {
                Direction direction = WorldEditManager.GetDirectionAndAmount(user, pDirectionAndAmount, out int amount);
                if (direction == Direction.Unknown)
                    return;

                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.ShiftSelection(direction.ToVec() * amount))
                    user.Player.ErrorLocStr($"Shifted selection {amount} {direction}");
                else
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/up", "", ChatAuthorizationLevel.Admin)]
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
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }


        [ChatCommand("/undo", "", ChatAuthorizationLevel.Admin)]
        public static void Undo(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.Undo())
                    user.Player.ErrorLocStr($"Undo done.");
                else
                    user.Player.ErrorLocStr($"You can't use undo right now!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }


        [ChatCommand("/copy", "", ChatAuthorizationLevel.Admin)]
        public static void Copy(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.SaveSelectionToClipboard(user))
                    user.Player.ErrorLocStr($"Copy done.");
                else
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Pastes the current clipboard selection", "", ChatAuthorizationLevel.Admin)]
        public static void Paste(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.LoadSelectionFromClipboard(user, weud))
                    user.Player.ErrorLocStr($"Paste done.");
                else
                    user.Player.ErrorLocStr($"Please copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Paste Clipboard Selection and specify y offset", "paste-offset", ChatAuthorizationLevel.Admin)]
        public static void PasteOffset(User user, int offset)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.LoadSelectionFromClipboard(user, weud, offset))
                    user.Player.ErrorLocStr($"Paste done.");
                else
                    user.Player.ErrorLocStr($"Please copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/rotate", "", ChatAuthorizationLevel.Admin)]
        public static void Rotate(User user, int pDegree = 90)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.RotateClipboard(pDegree))
                    user.Player.ErrorLocStr($"Rotation in clipboard done.");
                else
                    user.Player.ErrorLocStr($"Please copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/export", "", ChatAuthorizationLevel.Admin)]
        public static void Export(User user, string pFileName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.SaveClipboard(pFileName))
                    user.Player.ErrorLocStr($"Export done.");
                else
                    user.Player.ErrorLocStr($"Please /copy a selection first!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("Import Legacy", ChatAuthorizationLevel.Admin)]
        public static void ImportLegacy(User user, string pFileName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.LoadClipboardLegacy(pFileName))
                    user.Player.ErrorLocStr($"Import done. Use //paste");
                else
                    user.Player.ErrorLocStr($"Schematic file not found!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/import", "", ChatAuthorizationLevel.Admin)]
        public static void Import(User user, string pFileName)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.LoadClipboard(pFileName))
                    user.Player.ErrorLocStr($"Import done. Use //paste");
                else
                    user.Player.ErrorLocStr($"Schematic file not found!");
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
            }
        }

        [ChatCommand("/distr", "", ChatAuthorizationLevel.Admin)]
        public static void Distr(User user)
        {
            try
            {
                WorldEditUserData weud = WorldEditManager.GetUserData(user.Name);

                if (weud.FirstPos == null || weud.SecondPos == null)
                {
                    user.Player.ErrorLocStr($"Please set both points with the Wand Tool first!");
                    return;
                }

                var vectors = weud.GetSortedVectors();

                Dictionary<string, long> mBlocks = new Dictionary<string, long>();

                for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                    for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                        for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                        {
                            //                 Console.WriteLine($"{x} {y} {z}");
                            var block = "";
                            var pos = new Vector3i(x, y, z);
                            var ablock = Eco.World.World.GetBlock(pos);
                            
                            if (ablock.GetType() == typeof(WorldObjectBlock))
                            {
                                var worldObject = ablock as WorldObjectBlock;
                                block = worldObject.WorldObjectHandle.Object.GetType().Name;
                            }
                            else if (ablock.GetType() == typeof(TreeBlock))
                            {
                                var worldObject = ablock as TreeBlock;
                                block = worldObject.GetType().Name;
                            }
                            else if (ablock.GetType() == typeof(BuildingWorldObjectBlock))
                            {
                                var worldObject = ablock as BuildingWorldObjectBlock;
                                block = worldObject.WorldObjectHandle.Object.GetType().Name;
                            }
                            else
                                block = World.World.GetBlock(pos).GetType().ToString();

                            long count;
                            mBlocks.TryGetValue(block, out count);
                            mBlocks[block] = count + 1;
                        }

                double amountBlocks = mBlocks.Values.Sum(); // (vectors.Higher.X - vectors.Lower.X) * (vectors.Higher.Y - vectors.Lower.Y) * (vectors.Higher.Z - vectors.Lower.Z);

                string msg = $"total blocks: {amountBlocks}\n";
                foreach (var entry in mBlocks)
                {
                    string percent = (Math.Round((entry.Value / amountBlocks) * 100, 2)).ToString() + "%";
                    string nameOfBlock = entry.Key.Substring(entry.Key.LastIndexOf(".") + 1);
                    msg += $"{entry.Value.ToString().PadRight(6)} {percent.PadRight(6)} {nameOfBlock} \n";
                }
                user.Player.OpenInfoPanel("", msg, null);
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.DoStr($"{e}"));
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
                weud.FirstPos = (Vector3i)pos;

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
                weud.SecondPos = (Vector3i)pos;

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
            user.Player.MsgLocStr($"World Edit - Beta Test Version: Experimental version B-145");
        }

        [ChatCommand("/grow", "", ChatAuthorizationLevel.Admin)]
        public static void Grow(User user)
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

                            if (block.GetType() == typeof(PlantBlock))
                            {
                                var pb = PlantBlock.GetPlant(pos);
                                pb.GrowthPercent = 1;
                                pb.Tended = true;
                                pb.Tick();
                            }

                        }

            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"{e}"));
            }
        }
    }
}
using Eco.Core.Serialization;
using Eco.EM.Framework.Utils;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World;
using EcoWorldEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Eco.EM.Framework.FileManager;
using Newtonsoft.Json;

namespace Eco.Mods.WorldEdit
{
    public class WorldEditUserData
    {
        public const string mSchematicPath = "./Configs/Mods/WorldEdit/Schematics/";
        public const string schemtaic = ".Schematic";
        public Vector3i? FirstPos;
        public Vector3i? SecondPos;

        //multiple undos with circular buffer?
        private Stack<WorldEditBlock> mLastCommandBlocks = null;

        private List<WorldEditBlock> mClipboard = new List<WorldEditBlock>();

        private Vector3i mUserClipboardPosition;

        public UserSession GetNewSession()
        {
            return new UserSession();
        }

        public SortedVectorPair GetSortedVectors()
        {
            if (!FirstPos.HasValue || !SecondPos.HasValue)
                return null;

            Vector3i pos1 = FirstPos.Value;
            Vector3i pos2 = SecondPos.Value;

            int typeOfVolume = FindTypeOfVolume(ref pos1, ref pos2, out Vector3i lower, out Vector3i higher);

            if (typeOfVolume == 1)
            {
                //swap
                Vector3i tmp = higher;
                higher = lower;
                lower = tmp;

                lower.Y = Math.Min(pos1.Y, pos2.Y);
                higher.Y = Math.Max(pos1.Y, pos2.Y) + 1;
            }
            if (typeOfVolume == 3)
            {
                int tmp = higher.X;
                higher.X = lower.X;
                lower.X = tmp;
            }
            else if (typeOfVolume == 2)
            {
                int tmp = higher.Z;
                higher.Z = lower.Z;
                lower.Z = tmp;
            }

            higher.X = (higher.X + 1) % Shared.Voxel.World.VoxelSize.X;
            higher.Z = (higher.Z + 1) % Shared.Voxel.World.VoxelSize.Z;

            return new SortedVectorPair(lower, higher);
        }

        public int FindTypeOfVolume(ref Vector3i pos1, ref Vector3i pos2, out Vector3i lower, out Vector3i higher)
        {
            lower = new Vector3i();
            higher = new Vector3i();

            pos1.X = pos1.X % Shared.Voxel.World.VoxelSize.X;
            pos1.Z = pos1.Z % Shared.Voxel.World.VoxelSize.Z;

            pos2.X = pos2.X % Shared.Voxel.World.VoxelSize.X;
            pos2.Z = pos2.Z % Shared.Voxel.World.VoxelSize.Z;

            lower.X = Math.Min(pos1.X, pos2.X);
            lower.Y = Math.Min(pos1.Y, pos2.Y);
            lower.Z = Math.Min(pos1.Z, pos2.Z);

            higher.X = Math.Max(pos1.X, pos2.X);
            higher.Y = Math.Max(pos1.Y, pos2.Y) + 1;
            higher.Z = Math.Max(pos1.Z, pos2.Z);

            KeyValuePair<int, int>[] volumes = new KeyValuePair<int, int>[4];

            //sometimes a delta is 0 - we add one so the volume is not 0. The value does not matter here!
            volumes[0] = new KeyValuePair<int, int>(0, (((higher.X - lower.X) + 1) * ((higher.Z - lower.Z) + 1)));
            volumes[1] = new KeyValuePair<int, int>(1, (lower.X + (Shared.Voxel.World.VoxelSize.X - higher.X) + 1) * (lower.Z + (Shared.Voxel.World.VoxelSize.Z - higher.Z) + 1));
            volumes[2] = new KeyValuePair<int, int>(2, ((higher.X - lower.X) + 1) * (lower.Z + (Shared.Voxel.World.VoxelSize.Z - higher.Z) + 1));
            volumes[3] = new KeyValuePair<int, int>(3, (lower.X + (Shared.Voxel.World.VoxelSize.X - higher.X) + 1) * ((higher.Z - lower.Z) + 1));

            KeyValuePair<int, int> min = volumes.MinObj(kv => kv.Value);

            return min.Key;
        }

        public bool ExpandSelection(Vector3i pDirection, bool pContract = false)
        {
            SortedVectorPair svp = GetSortedVectors();
            if (svp == null)
                return false;

            Vector3i pos1 = FirstPos.Value;
            Vector3i pos2 = SecondPos.Value;

            int typeOfVolume = FindTypeOfVolume(ref pos1, ref pos2, out Vector3i lower, out Vector3i higher);

            var firstResult = SumAllAxis(pDirection * FirstPos.Value);
            var secondResult = SumAllAxis(pDirection * SecondPos.Value);

            bool useFirst = firstResult > secondResult;

            if (typeOfVolume == 1)
                useFirst = !useFirst;

            if (pContract)
                useFirst = !useFirst;

            if (useFirst)
                FirstPos = FirstPos + pDirection;
            else
                SecondPos = SecondPos + pDirection;

            return true;
        }

        public bool ShiftSelection(Vector3i pDirection)
        {
            if (!FirstPos.HasValue || !SecondPos.HasValue)
                return false;

            FirstPos = FirstPos + pDirection;
            SecondPos = SecondPos + pDirection;
            return true;
        }

        // Is there a correct name for this operation?
        public int SumAllAxis(Vector3i pVector)
        {
            return pVector.X + pVector.Y + pVector.Z;
        }

        public void StartEditingBlocks()
        {
            mLastCommandBlocks = new Stack<WorldEditBlock>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBlockChangedEntry(Block pBlock, Vector3i pPosition, Vector3i? pSourcePosition = null)
        {
            pSourcePosition = pSourcePosition ?? pPosition;
            mLastCommandBlocks.Push(WorldEditBlock.API.CreateNew(pBlock, pPosition, pSourcePosition));
        }

        public bool Undo()
        {
            if (mLastCommandBlocks == null)
                return false;

            UserSession session = GetNewSession();

            foreach (var entry in mLastCommandBlocks)
            {
                WorldEditManager.SetBlock(entry, session);
            }
            return true;
        }

        public bool SaveSelectionToClipboard(User pUser)
        {
            var vectors = GetSortedVectors();

            if (vectors == null)
                return false;

            mUserClipboardPosition = pUser.Player.Position.Round;

            mClipboard.Clear();

            for (int x = vectors.Lower.X; x != vectors.Higher.X; x = (x + 1) % Shared.Voxel.World.VoxelSize.X)
                for (int y = vectors.Lower.Y; y < vectors.Higher.Y; y++)
                    for (int z = vectors.Lower.Z; z != vectors.Higher.Z; z = (z + 1) % Shared.Voxel.World.VoxelSize.Z)
                    {
                        var pos = new Vector3i(x, y, z);
                        
                        mClipboard.Add(WorldEditBlock.API.CreateNew(World.World.GetBlock(pos), pos - mUserClipboardPosition, pos));
                    }
            return true;
        }

        public bool LoadSelectionFromClipboard(User pUser, WorldEditUserData pWeud, int offset)
        {
            try
            {
                if (mClipboard == null)
                    return false;

                StartEditingBlocks();
                var currentPos = pUser.Player.Position.Round;
                UserSession session = pWeud.GetNewSession();
                Vector3i newOffset = new Vector3i(0, offset, 0);

                foreach (var entry in mClipboard)
                {
                    var web = entry.Clone();
                    web.Position += currentPos;
                    AddBlockChangedEntry(World.World.GetBlock(web.Position + newOffset), web.Position + newOffset);
                    WorldEditManager.SetBlock(web.Type, web.Position + newOffset, session, null, null, web.Data);
                }
                return true;
            }
            catch (Exception e)
            {
                Log.WriteErrorLineLocStr(Localizer.DoStr($"{e}"));
                return false;
            }

        }

        public bool LoadSelectionFromClipboard(User pUser, WorldEditUserData pWeud)
        {
            try
            {
                if (mClipboard == null)
                    return false;

                StartEditingBlocks();
                var currentPos = pUser.Player.Position.Round;
                UserSession session = pWeud.GetNewSession();

                foreach (var entry in mClipboard)
                {
                    var web = entry.Clone();
                    web.Position += currentPos;
                    AddBlockChangedEntry(World.World.GetBlock(web.Position), web.Position);
                    WorldEditManager.SetBlock(web.Type, web.Position, session, null, null, web.Data);
                }
                return true;
            }
            catch( Exception e)
            {
                Log.WriteErrorLineLocStr(Localizer.DoStr($"{e}"));
                return false;
            }
            
        }

        public bool RotateClipboard(float pAngle)
        {
            if (mClipboard == null)
                return false;

            AffineTransform transform = new AffineTransform();

            pAngle = (float)(Math.PI * MathUtil.NormalizeAngle0to360(pAngle) / 180.0);
            transform = transform.RotateY(pAngle);

            for (int i = 0; i < mClipboard.Count; i++)
            {
                var block = mClipboard[i].Clone();
                block.Position = transform.Apply(block.Position);
                mClipboard[i] = block;
            }

            return true;
        }

        public bool SaveClipboard(string pFileName)
        {
            if (mClipboard == null || mClipboard.Count <= 0)
                return false;

            /*
            var stream = EcoSerializer.Serialize<List<WorldEditBlock>>(mClipboard);
            Directory.CreateDirectory(mSchematicPath);
            pFileName = new string(pFileName.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());
            File.WriteAllBytes(Path.Combine(mSchematicPath, pFileName + ".ecoschematic"), stream.ToArray());
            */

            FileManager<List<WorldEditBlock>>.WriteTypeHandledToFile(mClipboard, mSchematicPath, pFileName, schemtaic);

            return true;
        }

        public bool LoadClipboard(string pFileName)
        {
            /*
            pFileName = new string(pFileName.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());

            pFileName = Path.Combine(mSchematicPath, pFileName + ".ecoschematic");

            if (!File.Exists(pFileName))
                return false;

            mClipboard = EcoSerializer.Deserialize<List<WorldEditBlock>>(File.OpenRead(pFileName)).ToList();
            */

            if (!File.Exists(Path.Combine(mSchematicPath + pFileName + schemtaic)))
                return false;

            mClipboard = FileManager<List<WorldEditBlock>>.ReadTypeHandledFromFile(mSchematicPath, pFileName, schemtaic);

            return true;
        }

        public bool LoadClipboardLegacy(string pFileName)
        {
            
            pFileName = new string(pFileName.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());

            pFileName = Path.Combine(mSchematicPath, pFileName + ".ecoschematic");

            if (!File.Exists(pFileName))
                return false;

            mClipboard = EcoSerializer.Deserialize<List<WorldEditBlock>>(File.OpenRead(pFileName));

            return true;
        }
    }
}


using Eco.Core.Serialization;
using Eco.Gameplay.Players;
using Eco.Shared;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using Eco.World;
using Eco.World.Blocks;
using EcoWorldEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Eco.Mods.WorldEdit
{
    public class WorldEditUserData
    {
        #region
        public class AffineTransform
        {

            /**
             * coefficients for x coordinate.
             */
            private float m00, m01, m02, m03;

            /**
             * coefficients for y coordinate.
             */
            private float m10, m11, m12, m13;

            /**
             * coefficients for z coordinate.
             */
            private float m20, m21, m22, m23;

            // ===================================================================
            // constructors

            /**
             * Creates a new affine transform3D set to identity
             */
            public AffineTransform()
            {
                // init to identity matrix
                m00 = m11 = m22 = 1;
                m01 = m02 = m03 = 0;
                m10 = m12 = m13 = 0;
                m20 = m21 = m23 = 0;
            }

            public AffineTransform(float[] coefs)
            {
                if (coefs.Length == 9)
                {
                    m00 = coefs[0];
                    m01 = coefs[1];
                    m02 = coefs[2];
                    m10 = coefs[3];
                    m11 = coefs[4];
                    m12 = coefs[5];
                    m20 = coefs[6];
                    m21 = coefs[7];
                    m22 = coefs[8];
                }
                else if (coefs.Length == 12)
                {
                    m00 = coefs[0];
                    m01 = coefs[1];
                    m02 = coefs[2];
                    m03 = coefs[3];
                    m10 = coefs[4];
                    m11 = coefs[5];
                    m12 = coefs[6];
                    m13 = coefs[7];
                    m20 = coefs[8];
                    m21 = coefs[9];
                    m22 = coefs[10];
                    m23 = coefs[11];
                }
                else
                {
                    throw new ArgumentException("Input array must have 9 or 12 elements");
                }
            }

            public AffineTransform(float xx, float yx, float zx, float tx,
                                   float xy, float yy, float zy, float ty, float xz, float yz,
                                   float zz, float tz)
            {
                m00 = xx;
                m01 = yx;
                m02 = zx;
                m03 = tx;
                m10 = xy;
                m11 = yy;
                m12 = zy;
                m13 = ty;
                m20 = xz;
                m21 = yz;
                m22 = zz;
                m23 = tz;
            }

            // ===================================================================
            // accessors


            public bool IsIdentity()
            {
                if (m00 != 1)
                    return false;
                if (m11 != 1)
                    return false;
                if (m22 != 1)
                    return false;
                if (m01 != 0)
                    return false;
                if (m02 != 0)
                    return false;
                if (m03 != 0)
                    return false;
                if (m10 != 0)
                    return false;
                if (m12 != 0)
                    return false;
                if (m13 != 0)
                    return false;
                if (m20 != 0)
                    return false;
                if (m21 != 0)
                    return false;
                if (m23 != 0)
                    return false;
                return true;
            }

            /**
             * Returns the affine coefficients of the transform. Result is an array of
             * 12 float.
             */

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float[] Coefficients()
            {
                return new float[] { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23 };
            }

            /**
             * Computes the determinant of this transform. Can be zero.
             *
             * @return the determinant of the transform.
             */
            private float Determinant()
            {
                return m00 * (m11 * m22 - m12 * m21) - m01 * (m10 * m22 - m20 * m12)
                        + m02 * (m10 * m21 - m20 * m11);
            }

            /**
             * Computes the inverse affine transform.
             */

            public AffineTransform Inverse()
            {
                float det = this.Determinant();
                return new AffineTransform(
                        (m11 * m22 - m21 * m12) / det,
                        (m21 * m01 - m01 * m22) / det,
                        (m01 * m12 - m11 * m02) / det,
                        (m01 * (m22 * m13 - m12 * m23) + m02 * (m11 * m23 - m21 * m13)
                                - m03 * (m11 * m22 - m21 * m12)) / det,
                        (m20 * m12 - m10 * m22) / det,
                        (m00 * m22 - m20 * m02) / det,
                        (m10 * m02 - m00 * m12) / det,
                        (m00 * (m12 * m23 - m22 * m13) - m02 * (m10 * m23 - m20 * m13)
                                + m03 * (m10 * m22 - m20 * m12)) / det,
                        (m10 * m21 - m20 * m11) / det,
                        (m20 * m01 - m00 * m21) / det,
                        (m00 * m11 - m10 * m01) / det,
                        (m00 * (m21 * m13 - m11 * m23) + m01 * (m10 * m23 - m20 * m13)
                                - m03 * (m10 * m21 - m20 * m11)) / det);
            }

            // ===================================================================
            // general methods

            /**
             * Returns the affine transform created by applying first the affine
             * transform given by {@code that}, then this affine transform.
             *
             * @param that the transform to apply first
             * @return the composition this * that
             */
            public AffineTransform Concatenate(AffineTransform that)
            {
                float n00 = m00 * that.m00 + m01 * that.m10 + m02 * that.m20;
                float n01 = m00 * that.m01 + m01 * that.m11 + m02 * that.m21;
                float n02 = m00 * that.m02 + m01 * that.m12 + m02 * that.m22;
                float n03 = m00 * that.m03 + m01 * that.m13 + m02 * that.m23 + m03;
                float n10 = m10 * that.m00 + m11 * that.m10 + m12 * that.m20;
                float n11 = m10 * that.m01 + m11 * that.m11 + m12 * that.m21;
                float n12 = m10 * that.m02 + m11 * that.m12 + m12 * that.m22;
                float n13 = m10 * that.m03 + m11 * that.m13 + m12 * that.m23 + m13;
                float n20 = m20 * that.m00 + m21 * that.m10 + m22 * that.m20;
                float n21 = m20 * that.m01 + m21 * that.m11 + m22 * that.m21;
                float n22 = m20 * that.m02 + m21 * that.m12 + m22 * that.m22;
                float n23 = m20 * that.m03 + m21 * that.m13 + m22 * that.m23 + m23;
                return new AffineTransform(
                        n00, n01, n02, n03,
                        n10, n11, n12, n13,
                        n20, n21, n22, n23);
            }

            /**
             * Return the affine transform created by applying first this affine
             * transform, then the affine transform given by {@code that}.
             *
             * @param that the transform to apply in a second step
             * @return the composition that * this
             */
            public AffineTransform PreConcatenate(AffineTransform that)
            {
                float n00 = that.m00 * m00 + that.m01 * m10 + that.m02 * m20;
                float n01 = that.m00 * m01 + that.m01 * m11 + that.m02 * m21;
                float n02 = that.m00 * m02 + that.m01 * m12 + that.m02 * m22;
                float n03 = that.m00 * m03 + that.m01 * m13 + that.m02 * m23 + that.m03;
                float n10 = that.m10 * m00 + that.m11 * m10 + that.m12 * m20;
                float n11 = that.m10 * m01 + that.m11 * m11 + that.m12 * m21;
                float n12 = that.m10 * m02 + that.m11 * m12 + that.m12 * m22;
                float n13 = that.m10 * m03 + that.m11 * m13 + that.m12 * m23 + that.m13;
                float n20 = that.m20 * m00 + that.m21 * m10 + that.m22 * m20;
                float n21 = that.m20 * m01 + that.m21 * m11 + that.m22 * m21;
                float n22 = that.m20 * m02 + that.m21 * m12 + that.m22 * m22;
                float n23 = that.m20 * m03 + that.m21 * m13 + that.m22 * m23 + that.m23;
                return new AffineTransform(
                        n00, n01, n02, n03,
                        n10, n11, n12, n13,
                        n20, n21, n22, n23);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform Translate(Vector3i vec)
            {
                return Translate(vec.x, vec.y, vec.z);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform Translate(float x, float y, float z)
            {
                return Concatenate(new AffineTransform(1, 0, 0, x, 0, 1, 0, y, 0, 0, 1, z));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform RotateXDeg(float pAngleDeg)
            {
                return RotateX(Mathf.DegToRad(pAngleDeg));
            }

            public AffineTransform RotateX(float theta)
            {
                float cot = Mathf.Cos(theta);
                float sit = Mathf.Sin(theta);
                return Concatenate(
                        new AffineTransform(
                                1, 0, 0, 0,
                                0, cot, -sit, 0,
                                0, sit, cot, 0));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform RotateYDeg(float pAngleDeg)
            {
                return RotateY(Mathf.DegToRad(pAngleDeg));
            }

            public AffineTransform RotateY(float theta)
            {
                float cot = Mathf.Cos(theta);
                float sit = Mathf.Sin(theta);
                return Concatenate(
                        new AffineTransform(
                                cot, 0, sit, 0,
                                0, 1, 0, 0,
                                -sit, 0, cot, 0));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform RotateZDeg(float pAngleDeg)
            {
                return RotateZ(Mathf.DegToRad(pAngleDeg));
            }

            public AffineTransform RotateZ(float theta)
            {
                float cot = Mathf.Cos(theta);
                float sit = Mathf.Sin(theta);
                return Concatenate(
                        new AffineTransform(
                                cot, -sit, 0, 0,
                                sit, cot, 0, 0,
                                0, 0, 1, 0));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform Scale(float s)
            {
                return Scale(s, s, s);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform Scale(float sx, float sy, float sz)
            {
                return Concatenate(new AffineTransform(sx, 0, 0, 0, 0, sy, 0, 0, 0, 0, sz, 0));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform Scale(Vector3i vec)
            {
                return Scale(vec.x, vec.y, vec.z);
            }

            public Vector3i Apply(Vector3i vector)
            {
                return new Vector3i(
                       (int)Math.Round(vector.x * m00 + vector.y * m01 + vector.Z * m02 + m03),
                          (int)Math.Round(vector.x * m10 + vector.y * m11 + vector.Z * m12 + m13),
                         (int)Math.Round(vector.x * m20 + vector.y * m21 + vector.Z * m22 + m23));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AffineTransform Combine(AffineTransform other)
            {
                return Concatenate(other);
            }

            public override string ToString()
            {
                return String.Format("Affine[%g %g %g %g, %g %g %g %g, %g %g %g %g]}", m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23);
            }
        }
        #endregion

        public const string mSchematicPath = "./Schematics/";
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
            mLastCommandBlocks.Push(WorldEditBlock.CreateNew(pBlock, pPosition, pSourcePosition));
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

                        //pos - mUserClipboardPosition: "Spitze minus Anfang"
                        mClipboard.Add(WorldEditBlock.CreateNew(Eco.World.World.GetBlock(pos), pos - mUserClipboardPosition, pos));
                    }
            return true;
        }

        public bool LoadSelectionFromClipboard(User pUser, WorldEditUserData pWeud)
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
                WorldEditManager.SetBlock(web.Type, web.Position, session, null, Block.Empty, web.Data);
                AddBlockChangedEntry(Eco.World.World.GetBlock(web.Position), web.Position);
                WorldEditManager.SetBlock(web.Type, web.Position, session, null, null, web.Data);
            }
            return true;
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

            var stream = EcoSerializer.Serialize<List<WorldEditBlock>>(mClipboard.ToList());

            Directory.CreateDirectory(mSchematicPath);
            pFileName = new string(pFileName.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());

            File.WriteAllBytes(Path.Combine(mSchematicPath, pFileName + ".ecoschematic"), stream.ToArray());

            return true;
        }

        public bool LoadClipboard(string pFileName)
        {
            pFileName = new string(pFileName.Where(x => !Path.GetInvalidFileNameChars().Contains(x)).ToArray());

            pFileName = Path.Combine(mSchematicPath, pFileName + ".ecoschematic");

            if (!File.Exists(pFileName))
                return false;

            mClipboard = EcoSerializer.Deserialize<List<WorldEditBlock>>(File.OpenRead(pFileName)).ToList();

            return true;
        }
    }
}

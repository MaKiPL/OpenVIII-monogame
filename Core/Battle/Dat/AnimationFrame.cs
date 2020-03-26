using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 3c: Model animation frames
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Animation"/>
    public struct AnimationFrame
    {
        #region Fields

        public readonly IReadOnlyList<Matrix> BoneMatrix;
        public readonly IReadOnlyList<Vector3> BonesVectorRotations;
        private const float Degrees = 360f;

        #endregion Fields

        #region Constructors

        private AnimationFrame(ExtapathyExtended.BitReader bitReader, Skeleton skeleton, AnimationFrame? previous = null)
        {
            /// <summary>
            /// Some enemies use additional information that is saved for bone AFTER rotation types. We
            /// are still not sure what it does as enemy works without it
            /// </summary>
            // ReSharper disable once UnusedLocalFunctionReturnValue
            (short unk1V, short unk2V, short unk3V) GetAdditionalRotationInformation()
            {
                short calc()
                {
                    var unk1 = bitReader.ReadBits(1);
                    const int special = 1024;
                    if ((byte)unk1 <= 0) return special; // if checked here it'll crash
                    var unk1V = bitReader.ReadBits(16);
                    return checked((short)(unk1V + special));
                }

                return (calc(), calc(), calc());
            }
            //Step 1. It starts with bone0.position. Let's read that into AnimationFrames[animId]- it's only one position per currentFrame

            var x = bitReader.ReadPositionType() * .01f;
            var y = bitReader.ReadPositionType() * .01f;
            var z = bitReader.ReadPositionType() * .01f;
            Position = !previous.HasValue
                ? new Vector3(x, y, z)
                : new Vector3(
                    previous.Value.Position.X + x,
                    previous.Value.Position.Y + y,
                    previous.Value.Position.Z + z);

            var modeTest = (byte)bitReader.ReadBits(1); //used to determine if additional info is required

            var boneMatrix = new Matrix[skeleton.CBones];
            var bonesVectorRotations = new Vector3[skeleton.CBones];

            //Step 2. We read the position and we need to store the bones rotations or save base rotation if currentFrame==0

            foreach (var k in Enumerable.Range(0, skeleton.CBones)) //bones iterator
            {
                if (previous.HasValue) //just like position the data for next frames are added to previous
                {
                    bonesVectorRotations[k] = new Vector3
                    {
                        X = bitReader.ReadRotationType(),
                        Y = bitReader.ReadRotationType(),
                        Z = bitReader.ReadRotationType()
                    };
                    if (modeTest > 0)
                        GetAdditionalRotationInformation();
                    var previousFrame = previous.Value.BonesVectorRotations[k];
                    var currentFrame = bonesVectorRotations[k];
                    bonesVectorRotations[k] =
                        previousFrame + currentFrame;
                }
                else //if this is zero currentFrame, then we need to set the base rotations for bones
                {
                    bonesVectorRotations[k] = new Vector3
                    {
                        X = bitReader.ReadRotationType(),
                        Y = bitReader.ReadRotationType(),
                        Z = bitReader.ReadRotationType()
                    };
                    if (modeTest > 0)
                        GetAdditionalRotationInformation();
                }

                //Step 3. We now have all bone rotations stored into short. We need to convert that into Matrix and 360/4096

                var boneRotation = bonesVectorRotations[k];
                boneRotation =
                    Extended.S16VectorToFloat(
                        boneRotation); //we had vector3 containing direct copy of short to float, now we need them in real floating point values
                boneRotation *= Degrees; //bone rotations are in 360 scope
                                         //maki way
                var xRot = Extended.GetRotationMatrixX(-boneRotation.X);
                var yRot = Extended.GetRotationMatrixY(-boneRotation.Y);
                var zRot = Extended.GetRotationMatrixZ(-boneRotation.Z);

                //this is the monogame way and gives same results as above.
                //Matrix xRot = Matrix.CreateRotationX(MathHelper.ToRadians(boneRotation.X));
                //Matrix yRot = Matrix.CreateRotationY(MathHelper.ToRadians(boneRotation.Y));
                //Matrix zRot = Matrix.CreateRotationZ(MathHelper.ToRadians(boneRotation.Z));

                var matrixZ = Extended.MatrixMultiply_transpose(yRot, xRot);
                matrixZ = Extended.MatrixMultiply_transpose(zRot, matrixZ);

                // ReSharper disable once CommentTypo
                if (skeleton.Bones[k].ParentId == 0xFFFF
                ) //if parentId is 0xFFFF then the current bone is core aka bone0
                {
                    matrixZ.M41 = -Position.X;
                    matrixZ.M42 = -Position.Y; //up/down
                    matrixZ.M43 = Position.Z;
                    matrixZ.M44 = 1;
                }
                else
                {
                    var parentBone = boneMatrix[skeleton.Bones[k].ParentId]; //gets the parent bone
                    matrixZ.M43 = skeleton.Bones[skeleton.Bones[k].ParentId].Size;
                    var rMatrix = Matrix.Multiply(parentBone, matrixZ);
                    rMatrix.M41 = parentBone.M11 * matrixZ.M41 + parentBone.M12 * matrixZ.M42 +
                                  parentBone.M13 * matrixZ.M43 + parentBone.M41;
                    rMatrix.M42 = parentBone.M21 * matrixZ.M41 + parentBone.M22 * matrixZ.M42 +
                                  parentBone.M23 * matrixZ.M43 + parentBone.M42;
                    rMatrix.M43 = parentBone.M31 * matrixZ.M41 + parentBone.M32 * matrixZ.M42 +
                                  parentBone.M33 * matrixZ.M43 + parentBone.M43;
                    rMatrix.M44 = 1;
                    matrixZ = rMatrix;
                }

                boneMatrix[k] = matrixZ;
            }

            BonesVectorRotations = bonesVectorRotations;
            BoneMatrix = boneMatrix;
        }

        #endregion Constructors

        #region Properties

        public Vector3 Position { get; }

        #endregion Properties

        #region Methods

        public static IReadOnlyList<AnimationFrame> CreateInstances(BinaryReader br, byte cFrames, Skeleton skeleton)
        {
            var bitReader = new ExtapathyExtended.BitReader(br.BaseStream);
            var animationFrames = new AnimationFrame[cFrames];
            foreach (var n in Enumerable.Range(0, cFrames)) //frames
                animationFrames[n] = n == 0
                    ? new AnimationFrame(bitReader, skeleton)
                    : new AnimationFrame(bitReader, skeleton, animationFrames[n - 1]);

            return animationFrames;
        }

        #endregion Methods
    }
}
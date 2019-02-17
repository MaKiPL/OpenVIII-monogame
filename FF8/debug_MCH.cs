using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class debug_MCH
    {
        private uint pBase;
        private MemoryStream ms;
        private BinaryReader br;

        public uint cSkeletonBones;
        public uint cVertices;
        public uint cTexAnimations;
        public uint cFaces;
        public uint cUnk;
        public uint cSkinObjects;
        public uint Unk;
        public ushort cTris;
        public ushort cQuads;
        public uint pBones;
        public uint pVertices;
        public uint pTexAnimations;
        public uint pFaces;
        public uint pUnk;
        public uint pSkinObjects;
        public uint pAnimation;
        public uint Unk2;

        private struct AnimationKeypoint
        {
            public short X;
            public short Y;
            public short Z;
            public Vector3[] rot;
        }

        private AnimationKeypoint[] animationKeypoints;

        public debug_MCH(MemoryStream ms, BinaryReader br)
        {
            this.ms = ms;
            this.br = br;
            pBase = (uint)ms.Position;
            cSkeletonBones = br.ReadUInt32();
            cVertices = br.ReadUInt32();
            cTexAnimations = br.ReadUInt32();
            cFaces = br.ReadUInt32();
            cUnk = br.ReadUInt32();
            cSkinObjects = br.ReadUInt32();
            Unk = br.ReadUInt32();
            cTris = br.ReadUInt16();
            cQuads = br.ReadUInt16();
            pBones = br.ReadUInt32();
            pVertices = br.ReadUInt32();
            pTexAnimations = br.ReadUInt32();
            pFaces = br.ReadUInt32();
            pUnk = br.ReadUInt32();
            pSkinObjects = br.ReadUInt32();
            pAnimation = br.ReadUInt32();
            Unk2 = br.ReadUInt32();

            ReadSkeleton(cSkeletonBones, pBones);
            ReadGeometry(cVertices, pVertices, cFaces, pFaces, cTris, cQuads);
            ReadAnimation(pAnimation);
        }

        private void ReadSkeleton(uint cSkeletonBones, uint pBones)
        {
            return;
            throw new NotImplementedException();
        }
        private void ReadGeometry(uint cVertices, uint pVertices, uint cFaces, uint pFaces, ushort cTris, ushort cQuads)
        {
            return;
            throw new NotImplementedException();
        }
        private void ReadAnimation(uint pAnimation)
        {
            ms.Seek(pBase + pAnimation, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler
            ushort animationCount = br.ReadUInt16();
            List<AnimationKeypoint> animKeypoints = new List<AnimationKeypoint>();

            while (animationCount != 0/*(cEntries = br.ReadUInt16()) != 0*/)
            {
                ushort animationFramesCount = br.ReadUInt16();
                ushort cBones = br.ReadUInt16();
                while (animationFramesCount != 0)
                {
                    AnimationKeypoint keyPoint = new AnimationKeypoint() { X = br.ReadInt16(), Y = br.ReadInt16(), Z = br.ReadInt16() };
                    Vector3[] vetRot = new Vector3[cBones];
                    for (int i = 0; i < cBones; i++)
                        vetRot[i] = new Vector3() { X = br.ReadInt16(), Y = br.ReadUInt16(), Z = br.ReadUInt16() };
                    animationFramesCount--;
                    keyPoint.rot = vetRot;
                    animKeypoints.Add(keyPoint);
                }
                animationCount--;
            }
            animationKeypoints = animKeypoints.ToArray();
            return;
        }
    }
}

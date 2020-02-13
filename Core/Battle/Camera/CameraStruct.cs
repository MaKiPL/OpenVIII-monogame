using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #region Structs

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1092)]
        public struct CameraStruct
        {
            public byte animationId; //000
            public byte keyframeCount;
            public ushort control_word;
            public ushort startingFOV; //usually ~280
            public ushort endingFOV; //006
            public ushort startingCameraRoll; //usually 0 unless you're aiming for some wicked animation
            public ushort endingCameraRoll; //
            private ushort startingTime; //usually 0, that's pretty logical

            /// <summary>
            /// Time is calculated from number of frames. You basically set starting position
            /// World+lookat and ending position, then mark number of frames to interpolate between
            /// them. Every frame is one drawcall and it costs 16.
            /// </summary>
            public ushort time; //starting time needs to be equal or higher for next animation frame to be read; If next frame==0xFFFF then it's all done

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] unk; //010

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public ushort[] startFramesOffsets; //024 - start frames for each key frame?

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_Z_s16; //064

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_X_s16; //0A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_Y_s16; //0E4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] is_FrameDurationsShot; //124

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_Z_s16; //144

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_X_s16; //184

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_Y_s16; //1C4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] is_FrameEndingShots; //204

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte224; //224

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte2A4; //2A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte324; //324

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte3A4; //3A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte424; //424

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte4A4; //4A4

            private float V => Memory.CameraScale;

            private Vector3 offset => new Vector3(30, +40, 0);

            public Vector3 Camera_World(int i) => new Vector3(
                Camera_World_X_s16[i],
                -(Camera_World_Y_s16[i]),
                -(Camera_World_Z_s16[i]))/V + offset;

            public Vector3 Camera_Lookat(int i) => new Vector3(
                Camera_Lookat_X_s16[i],
                -(Camera_Lookat_Y_s16[i]),
                -(Camera_Lookat_Z_s16[i]))/V + offset;

            public void UpdateTime() => CurrentTime += Memory.ElapsedGameTime;

            public TimeSpan CurrentTime;
            public TimeSpan TotalTime => TimeSpan.FromTicks(TotalTimePerFrame.Ticks * time);

            /// <summary>
            /// (1000) milliseconds / frames per second
            /// </summary>
            public TimeSpan TotalTimePerFrame => TimeSpan.FromMilliseconds(1000d / 240d);

            public void ResetTime() => CurrentTime = TimeSpan.Zero;
        };

        #endregion Structs
    }
}
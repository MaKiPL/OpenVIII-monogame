using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable 169
#pragma warning disable 649

namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #region Structs

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1092)]
        [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public struct CameraStruct
        {
            public byte AnimationId { get; set; } //000
            private byte KeyframeCount;
            public ushort ControlWord { get; private set; }
            public ushort StartingFov { get; private set; } //usually ~280
            public ushort EndingFov { get; private set; } //006
            private ushort StartingCameraRoll; //usually 0 unless you're aiming for some wicked animation
            private ushort EndingCameraRoll; //
            private readonly ushort _startingTime; //usually 0, that's pretty logical

            /// <summary>
            /// Time is calculated from number of frames. You basically set starting position
            /// World+lookat and ending position, then mark number of frames to interpolate between
            /// them. Every frame is one draw call and it costs 16.
            /// </summary>
            /// ReSharper disable once CommentTypo
            /// <remarks>starting time needs to be equal or higher for next animation frame to be read; If next frame==0xFFFF then it's all done</remarks>
            public ushort Time { get; private set; }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            private readonly byte[] _unkBytes; //010

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly ushort[] StartFramesOffsets; //024 - start frames for each key frame?

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly short[] _cameraWorldZ; //064

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly short[] _cameraWorldX; //0A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly short[] _cameraWorldY; //0E4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly byte[] IsFrameDurationsShot; //124

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly short[] _cameraLookAtZ; //144

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly short[] _cameraLookAtX; //184

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly short[] _cameraLookAtY; //1C4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            private readonly byte[] IsFrameEndingShots; //204

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            private readonly byte[] _unkByte224; //224

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            private readonly byte[] _unkByte2A4; //2A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            private readonly byte[] _unkByte324; //324

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            private readonly byte[] _unkByte3A4; //3A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            private readonly byte[] _unkByte424; //424

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            private readonly byte[] _unkByte4A4; //4A4

            private static Vector3 Offset => new Vector3(40, 40, -40);
            //private (Vector3 World, Vector3 LookAt) this[int i] => (CameraWorld(i), CameraLookAt(i));

            private Vector3 CameraWorld(int i) =>
                new Vector3(
                    _cameraWorldX[i],
                    -(_cameraWorldY[i]),
                    -(_cameraWorldZ[i])) / Memory.CameraScale + Offset;

            private Vector3 CameraLookAt(int i) =>
                new Vector3(
                    _cameraLookAtX[i],
                    -(_cameraLookAtY[i]),
                    -(_cameraLookAtZ[i])) / Memory.CameraScale + Offset;

            public void UpdateTime() => CurrentTime += Memory.ElapsedGameTime;

            public TimeSpan CurrentTime { get; private set; }

            public TimeSpan TotalTime => TimeSpan.FromTicks(TotalTimePerFrame.Ticks * Time);

            /// <summary>
            /// (1000) milliseconds / frames per second, maybe not quite fps.
            /// </summary>
            private static TimeSpan TotalTimePerFrame => TimeSpan.FromMilliseconds(1000d / 240d);

            public void ResetTime() => CurrentTime = TimeSpan.Zero;

            public void ReadAnimation(BinaryReader br)
            {
                ControlWord = br.ReadUInt16();
                if (ControlWord == 0xFFFF) return;

                var currentPosition = br.ReadUInt16(); //getter for *current_position
                br.BaseStream.Seek(-2, SeekOrigin.Current); //roll back one WORD because no increment

                switch ((ControlWord >> 6) & 3)
                {
                    case 1:
                        StartingFov = 0x200;
                        EndingFov = 0x200;
                        break;

                    case 2:
                        StartingFov = currentPosition;
                        EndingFov = currentPosition;
                        br.ReadUInt16(); //current_position++
                        break;

                    case 3:
                        StartingFov = currentPosition;
                        br.BaseStream.Seek(2,
                            SeekOrigin
                                .Current); //skipping WORD, because we already rolled back one WORD above this switch
                        currentPosition = br.ReadUInt16();
                        EndingFov = currentPosition;
                        break;
                }

                switch ((ControlWord >> 8) & 3)
                {
                    case 0: //TODO!!
                        StartingCameraRoll = 00000000; //TODO, what's ff8vars.unkWord1D977A2?
                        EndingCameraRoll = 00000000; //same as above; cam->unkWord00A = ff8vars.unkWord1D977A2;
                        break;

                    case 1:
                        StartingCameraRoll = 0;
                        EndingCameraRoll = 0;
                        break;

                    case 2:
                        currentPosition = br.ReadUInt16(); //* + current_position++;
                        StartingCameraRoll = currentPosition;
                        EndingCameraRoll = currentPosition;
                        break;

                    case 3:
                        currentPosition = br.ReadUInt16(); //* + current_position++;
                        StartingCameraRoll = currentPosition;
                        currentPosition = br.ReadUInt16(); //* + current_position++;
                        EndingCameraRoll = currentPosition;
                        break;
                }

                byte keyFrameCount = 0;
                ushort totalFrameCount = 0;
                switch (ControlWord & 1)
                {
                    case 0:
                        while (true
                        ) //I'm setting this to true and breaking in code as this works on peeking on next variable via pointer and that's not possible here without unsafe block
                        {
                            StartFramesOffsets[keyFrameCount] = totalFrameCount; //looks like this is the camera index
                            currentPosition = br.ReadUInt16();
                            if ((short)currentPosition < 0
                            ) //reverse of *current_position >= 0, also cast to signed is important here
                                break;
                            totalFrameCount +=
                                (ushort)(currentPosition *
                                          16); //here is increment of short*, but I already did that above
                            IsFrameDurationsShot[keyFrameCount] =
                                (byte)(br
                                    .ReadUInt16()
                                ); //cam->unkByte124[keyFrameCount] = *current_position++; - looks like we are wasting one byte due to integer sizes
                            _cameraWorldX[keyFrameCount] = br.ReadInt16();
                            _cameraWorldY[keyFrameCount] = br.ReadInt16();
                            _cameraWorldZ[keyFrameCount] = br.ReadInt16();
                            IsFrameEndingShots[keyFrameCount] =
                                (byte)(br.ReadUInt16()); //m->unkByte204[keyFrameCount] = *current_position++;
                            _cameraLookAtX[keyFrameCount] = br.ReadInt16();
                            _cameraLookAtY[keyFrameCount] = br.ReadInt16();
                            _cameraLookAtZ[keyFrameCount] = br.ReadInt16();
                            keyFrameCount++;
                        }

                        if (keyFrameCount > 2)
                        {
                            //ff8Functions.Sub50D010(cam->unkWord024, cam->unkWord064, cam->unkWord0A4, cam->unkWord0E4, keyFrameCount, cam->unkByte224, cam->unkByte2A4, cam->unkByte324);
                            //ff8Functions.Sub50D010(cam->unkWord024, cam->unkWord144, cam->unkWord184, cam->unkWord1C4, keyFrameCount, cam->unkByte3A4, cam->unkByte424, cam->unkByte4A4);
                        }

                        break;

                    case 1:
                        {
                            goto case 0;
                            //if (currentPosition >= 0)
                            //{
                            //    var local14 = (short)(br.BaseStream.Position + 5 * 2);
                            //    var local10 = (short)(br.BaseStream.Position + 6 * 2);
                            //    var local2C = (short)(br.BaseStream.Position + 7 * 2);
                            //    var local18 = (short)(br.BaseStream.Position + 1 * 2);
                            //    var local1C = (short)(br.BaseStream.Position + 2 * 2);
                            //    while (true)
                            //    {
                            //        StartFramesOffsets[keyFrameCount] = totalFrameCount;
                            //        currentPosition = br.ReadUInt16();
                            //        if ((short)currentPosition < 0) //reverse of *current_position >= 0, also cast to signed is important here
                            //            break;
                            //        totalFrameCount += (ushort)(currentPosition * 16);
                            //        //ff8Functions.Sub503AE0(++local18, ++local1C, ++ebx, *(BYTE*)current_position, &cam->unkWord064[keyFrameCount], &cam->unkWord0A4[keyFrameCount], &cam->unkWord0E4[keyFrameCount]);
                            //        //ff8Functions.Sub503AE0(++local14, ++local10, ++local2C, *(BYTE*)(current_position + 4), &cam->unkWord144[keyFrameCount], &cam->unkWord184[keyFrameCount], &cam->unkWord1C4[keyFrameCount]);
                            //        IsFrameEndingShots[keyFrameCount] = 0xFB;
                            //        IsFrameDurationsShot[keyFrameCount] = 0xFB;
                            //        local1C += 8;
                            //        local18 += 8;
                            //        currentPosition += 8;
                            //        local2C += 8;
                            //        //ebx += 8;
                            //        local10 += 8;
                            //        local14 += 8;
                            //        keyFrameCount++;
                            //    }
                            //}
                            //break;
                        }
                }

                if ((ControlWord & 0x3E) == 0x1E)
                {
                    //ff8Functions.Sub503300();
                }

                KeyframeCount = keyFrameCount;
                Time = totalFrameCount;
                //cam.startingTime = 0;
                CurrentTime = TimeSpan.Zero;
            }

            public bool Done => CurrentTime >= TotalTime || (_cameraLookAtX[0], _cameraLookAtY[0], _cameraLookAtZ[0],
                _cameraWorldX[0], _cameraWorldY[0], _cameraWorldZ[0]) == (0, 0, 0, 0, 0, 0);

            public (Vector3 CamTarget, Vector3 CamPosition, Matrix View, Matrix Projection) UpdatePosition()
            {
                var step = CurrentTime.Ticks / (float)TotalTime.Ticks;
                if (step > 1f) step = 1f;
                var camTarget = Vector3.SmoothStep(CameraLookAt(0), CameraLookAt(1), step);
                var camPosition = Vector3.SmoothStep(CameraWorld(0), CameraWorld(1), step);
                var fov =
                    MathHelper.ToRadians(
                        (float)(2f * Math.Atan(240f / (2f *
                                 MathHelper.SmoothStep(StartingFov, EndingFov, step))) *
                                 57.29577951f));
                var viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);

                var projectionMatrix = (Memory.Graphics != null)
                    ? Matrix.CreatePerspectiveFieldOfView(fov,
                        Memory.Graphics.GraphicsDevice.Viewport.AspectRatio, 1f, 1000f)
                    : Matrix.Identity;

                return (camTarget, camPosition, viewMatrix, projectionMatrix);
            }
        }

        #endregion Structs
    }
}
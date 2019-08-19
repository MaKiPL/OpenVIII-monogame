using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;

namespace OpenVIII
{
    public class FPS_Camera
    {
        #region Fields

        private static readonly float camDistance = 10.0f;
        private static readonly float defaultmaxMoveSpeed = 1f;
        private static readonly float maxLookSpeed = 0.25f;
        private static readonly float MoveSpeedChange = 1f;
        //private static Vector3 camPosition, camTarget;
        //private float degrees = 90;
        private float Yshift;
        private Vector2 left;
        private float maxMoveSpeed = defaultmaxMoveSpeed;
        private Vector2 shift;
        private Vector2 leftdist;

        #endregion Fields

        #region Methods

        private void Inputs_Speed()
        {
            //speedcontrols
            //+ to increase
            //- to decrease
            //* to reset
            if (Input2.Button(Keys.OemPlus) || Input2.Button(Keys.Add))
            {
                maxMoveSpeed += MoveSpeedChange;
            }
            if (Input2.Button(Keys.OemMinus) || Input2.Button(Keys.Subtract))
            {
                maxMoveSpeed -= MoveSpeedChange;
                if (maxMoveSpeed < defaultmaxMoveSpeed) maxMoveSpeed = defaultmaxMoveSpeed;
            }
            if (Input2.Button(Keys.Multiply)) maxMoveSpeed = defaultmaxMoveSpeed;

            //speed is effected by the milliseconds between frames. so alittle goes a long way. :P
        }
        private void Inputs_Sticks(ref float degrees)
        {
            //require mouselock to center of screen for mouse joystick mode.
            InputMouse.Mode = MouseLockMode.Center;
            // check mouse to move camera
            shift = InputMouse.Distance(MouseButtons.MouseToStick, maxLookSpeed);
            // check right stick to adjust camera
            shift += InputGamePad.Distance(GamePadButtons.RightStick, maxMoveSpeed);
            //convert stick readings to degrees
            degrees = (degrees + (int)shift.X) % 360;
            Yshift -= shift.Y;
            Yshift = MathHelper.Clamp(Yshift, -80, 80);
            // grab left stick reading for moving camera position
            // storing signed value to detect direction of movement for left stick.
            left = InputGamePad.Distance(GamePadButtons.LeftStick, maxMoveSpeed);
            // convert to positive value to get distance traveled
            leftdist = left.Abs();

            if (leftdist == Vector2.Zero)
            {
                leftdist.Y = leftdist.X = (float)Input2.Distance(maxMoveSpeed);
            }
        }
        private void Inputs_D_Pad( ref Vector3 camPosition, ref float degrees)
        { 
            // using the calcuated direction and distance to move camera position
            // also fall back to arrow keys to move when not using a left stick
            if (Input2.Button(FF8TextTagKey.Up) || left.Y > 0)
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdist.Y / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdist.Y / 10;
                camPosition.Y -= Yshift / 50;
            }
            if (Input2.Button(FF8TextTagKey.Down) || left.Y < 0)
            {
                camPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdist.Y / 10;
                camPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdist.Y / 10;
                camPosition.Y += Yshift / 50;
            }
            if (Input2.Button(FF8TextTagKey.Left) || left.X > 0)
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees - 90)) * leftdist.X / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees - 90)) * leftdist.X / 10;
            }
            if (Input2.Button(FF8TextTagKey.Right) || left.X < 0)
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees + 90)) * leftdist.X / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees + 90)) * leftdist.X / 10;
            }
        }
        public Matrix Update(ref Vector3 camPosition,ref Vector3 camTarget, ref float degrees)
        {
            InputMouse.Mode = MouseLockMode.Center;
            Inputs_Speed();
            Inputs_Sticks(ref degrees);
            Inputs_D_Pad(ref camPosition, ref degrees);

            // adjust the camera target
            camTarget.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;

            // return the matrix of camera posistion and camera target to adjust the camera.
            return Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
        }

        #endregion Methods
    }
}
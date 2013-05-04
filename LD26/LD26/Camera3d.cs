using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD26
{
    class Camera3d
    {
        public static Camera3d c;

        public Vector3 position = new Vector3(0, 96, 0);
        public float leftRightRot = MathHelper.Pi;
        public float upDownRot = 0;
        public int cameraZoom = 80;


        public float upDownRotHead = 0;
        public float leftRightRotHead = 0;

        public float EyeOffset = 1f;

        public Camera3d()
        {
            c = this;
        }

        public void Update()
        {

            if (upDownRot > MathHelper.TwoPi)
                upDownRot = MathHelper.TwoPi;
            else if (upDownRot < MathHelper.TwoPi * -1)
                upDownRot += MathHelper.TwoPi;

            if (leftRightRot > MathHelper.TwoPi)
                leftRightRot -= MathHelper.TwoPi;
            else if (leftRightRot < MathHelper.TwoPi * -1)
                leftRightRot += MathHelper.TwoPi;

            if (RM.IsDown(InputAction.Up))
            {
                position.Z += 2;
            }
            if (RM.IsDown(InputAction.Down))
            {
                position.Z -= 2;
            }

            if (RM.IsDown(InputAction.Right))
            {
                position.X -= 2;
            }
            if (RM.IsDown(InputAction.Left))
            {
                position.X += 2;
            }
        }

        public void Apply(Effect e)
        {
            e.Parameters["World"].SetValue(Matrix.Identity);
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            cameraFinalTarget = position + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            e.Parameters["View"].SetValue(View);
            e.Parameters["Projection"].SetValue(Projection);
            e.Parameters["CameraPosition"].SetValue(position);
        }

        public void ApplyCustom(Entity ent)
        {
            var entPos = ent.Position.ToVector3();
            entPos.Y += ent.eyeYHeight;
            camPos = entPos;

            //Vector3 eye = new Vector3(RightEye ? EyeOffset : -EyeOffset, 0, 0);
            //var rotatedEye = Vector3.Transform(eye, Matrix.CreateRotationY(Camera3d.c.leftRightRot));


            Vector3 cameraRotatedTarget = ent.CamDirection;
            cameraFinalTarget = entPos + cameraRotatedTarget;// + rotatedEye;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            //e.Parameters["View"].SetValue(Matrix.CreateLookAt(entPos + rotatedEye, cameraFinalTarget, cameraOriginalUpVector));
            //e.Parameters["Projection"].SetValue(Projection);
            //e.Parameters["CameraPosition"].SetValue(entPos);
            
            Matrix viewCenterMatrix = Matrix.CreateLookAt(entPos, cameraFinalTarget, cameraOriginalUpVector);

            // Compute Aspect Ratio. Stereo mode cuts width in half.
            float aspectRatio = (RiftSettings.HResolution * 0.5f) / RiftSettings.VResolution;

            // Compute Vertical FOV based on distance.
            float yfov = 2.0f * (float)Math.Atan(RiftSettings.VScreenCenter/RiftSettings.EyeToScreenDistance);

            // Post-projection viewport coordinates range from (-1.0, 1.0), with the
            // center of the left viewport falling at (1/4) of horizontal screen size.
            // We need to shift this projection center to match with the lens center.
            // We compute this shift in physical units (meters) to correct
            // for different screen sizes and then rescale to viewport coordinates.
            float viewCenter = RiftSettings.HScreenSize * 0.25f;
            float eyeProjectionShift = viewCenter - RiftSettings.LensSeparationDistance*0.5f;
            float projectionCenterOffset = 4.0f * eyeProjectionShift / RiftSettings.HScreenSize;
            // Projection matrix for the "center eye", which the left/right matrices are based on.
            Matrix projCenter = Matrix.CreatePerspectiveFieldOfView(yfov, aspectRatio, 1f, 1000.0f);
            projLeft = Matrix.CreateTranslation(projectionCenterOffset, 0, 0) * projCenter;
            projRight = Matrix.CreateTranslation(-projectionCenterOffset, 0, 0) * projCenter;

            // View transformation translation in world units.
            float halfIPD = RiftSettings.InterpupillaryDistance * 0.5f;
            float halfIPDToScale = halfIPD*10f;
            viewLeft = Matrix.CreateTranslation(halfIPDToScale, 0, 0) * viewCenterMatrix;
            viewRight = Matrix.CreateTranslation(-halfIPDToScale, 0, 0) * viewCenterMatrix;
        }

        private Matrix projLeft;
        private Matrix projRight;
        private Matrix viewLeft;
        private Matrix viewRight;
        private Vector3 camPos;

        public void SetLeftEye(Effect e)
        {
            e.Parameters["World"].SetValue(Matrix.Identity);
            e.Parameters["View"].SetValue(viewLeft);
            e.Parameters["Projection"].SetValue(projLeft);
            e.Parameters["CameraPosition"].SetValue(camPos);
            
        }

        public void SetRightEye(Effect e)
        {
            e.Parameters["World"].SetValue(Matrix.Identity);
            e.Parameters["View"].SetValue(viewRight);
            e.Parameters["Projection"].SetValue(projRight);
            e.Parameters["CameraPosition"].SetValue(camPos);
            
        }

        public Vector3 GetCameraDirection()
        {
            return new Vector3(-(float)Math.Sin(leftRightRot), (float)upDownRot, -(float)Math.Cos(leftRightRot));
        }

        private Vector3 cameraFinalTarget;
        private Vector3 cameraRotatedUpVector;

        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(position, cameraFinalTarget, cameraRotatedUpVector);
            }
        }

        public Matrix Projection
        {
            get { return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(110), (float)(G.g.Window.ClientBounds.Width / 2f) / (float)G.g.Window.ClientBounds.Height, 1f, 10000f); }
        }

        public Vector3 MousePos
        {
            get
            {
                Viewport vp = G.g.GraphicsDevice.Viewport;
                //  Note the order of the parameters! Projection first.
                Vector3 pos1 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 0), Projection, View, Matrix.Identity);
                Vector3 pos2 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 1), Projection, View, Matrix.Identity);
                Vector3 dir = Vector3.Normalize(pos2 - pos1);

                var r = new Ray(position, dir);

                Vector3 n = new Vector3(0f, 1f, 0f);
                Plane p = new Plane(n, 0f);

                float denominator = Vector3.Dot(p.Normal, r.Direction);
                float numerator = Vector3.Dot(p.Normal, r.Position) + p.D;
                float t = -(numerator / denominator);

                // calculate the picked position on the y = 0 plane
                Vector3 pickedPosition = pos1 + dir * t;

                return pickedPosition;
            }
        }

        public Ray MouseRay
        {
            get
            {
                Viewport vp = G.g.GraphicsDevice.Viewport;
                //  Note the order of the parameters! Projection first.
                Vector3 pos1 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 0), Projection, View, Matrix.Identity);
                Vector3 pos2 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 1), Projection, View, Matrix.Identity);
                Vector3 dir = Vector3.Normalize(pos2 - pos1);

                var r = new Ray(position, dir);
                return r;
            }
        }

        public Ray MouseRayCustom(Entity ent)
        {
            var entPos = ent.Position.ToVector3();
            entPos.Y += ent.eyeYHeight;

            Vector3 cameraRotatedTarget = ent.CamDirection;
            cameraFinalTarget = entPos + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            var view = Matrix.CreateLookAt(entPos, cameraFinalTarget, cameraOriginalUpVector);

            Viewport vp = G.g.GraphicsDevice.Viewport;
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 0), Projection, view, Matrix.Identity);
            Vector3 pos2 = vp.Unproject(new Vector3(IM.MousePos.X, IM.MousePos.Y, 1), Projection, view, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            var r = new Ray(entPos, dir);
            return r;            
        }
    }
}

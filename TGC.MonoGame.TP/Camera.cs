using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Zero
{
    public class Camera
    {
        private float distanceBack;   // Distancia detrás del objeto seguido
        private float heightOffset;   // Altura por encima del objeto
        private float lookAhead;      // Cuánto mira por delante del objeto
        private float cameraYaw; // Actual angle (degrees)
        private const float MaxAngleDiff = 15f; // Angle threshold for semi-locked camera

        public Camera(float aspectRatio, float distanceBack, float heightOffset, float lookAhead)
        {
            this.distanceBack = distanceBack;
            this.heightOffset = heightOffset;
            this.lookAhead = lookAhead;

            // Proyección en perspectiva (FOV 60°, near/far plane amplios)
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60f),
                aspectRatio,
                1f,
                5000f);
            View = Matrix.Identity;
            cameraYaw = 0f;
        }

        public Matrix Projection { get; set; }
        public Matrix View { get; set; }

        public void Update(Matrix carWorld, float carYaw)
        {
            float angleDiff = MathHelper.ToDegrees(MathHelper.WrapAngle(MathHelper.ToRadians(carYaw - cameraYaw))); // Calculation of the angle between the cam and the car

            if (System.Math.Abs(angleDiff) > MaxAngleDiff)
            {
                cameraYaw = carYaw - System.Math.Sign(angleDiff) * MaxAngleDiff;
            }

            Vector3 targetPos = carWorld.Translation;
            Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw)));
            Vector3 up = carWorld.Up;

            Vector3 camPos = targetPos + forward * distanceBack + up * heightOffset;
            Vector3 lookAt = targetPos + forward * lookAhead;

            View = Matrix.CreateLookAt(camPos, lookAt, Vector3.Up);
        }
    }
}
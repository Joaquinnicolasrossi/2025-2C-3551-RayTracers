using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Zero
{
    public class Camera
    {
        private float distanceBack;   // Distancia detrás del objeto seguido
        private float heightOffset;   // Altura por encima del objeto
        private float lookAhead;      // Cuánto mira por delante del objeto

        public Camera(float aspectRatio, float distanceBack, float heightOffset, float lookAhead)
        {
            this.distanceBack = distanceBack;
            this.heightOffset = heightOffset;
            this.lookAhead = lookAhead;

            // Proyección en perspectiva (FOV 60°, near/far plane amplios)
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60f),
                aspectRatio,
                0.1f,
                100000f);
            View = Matrix.Identity;
        }

        public Matrix Projection { get; set; }
        public Matrix View { get; set; }

        public void Update(Matrix carWorld)
        {
            Vector3 targetPos = carWorld.Translation;
            Vector3 forward = carWorld.Forward;
            Vector3 up = carWorld.Up;

            Vector3 camPos = targetPos + forward * distanceBack + up * heightOffset;
            Vector3 lookAt = targetPos + forward * lookAhead;

            View = Matrix.CreateLookAt(camPos, lookAt, Vector3.Up);
        }
    }
}
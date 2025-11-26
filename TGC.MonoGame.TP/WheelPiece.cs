using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    internal class WheelPiece
    {
        public ModelMesh Mesh { get; }
        public Matrix World;
        public Vector3 Velocity;
        public Vector3 AngularVelocity; // radians/sec around local axes (approx)
        public float Life;
        private readonly float _initialLife;

        public WheelPiece(ModelMesh mesh, Matrix initialWorld, Vector3 initialVelocity, Vector3 angularVelocity, float lifeSeconds)
        {
            Mesh = mesh;
            World = initialWorld;
            Velocity = initialVelocity;
            AngularVelocity = angularVelocity;
            Life = lifeSeconds;
            _initialLife = lifeSeconds;
        }

        // returns false when dead
        public bool Update(float dt)
        {
            Life -= dt;
            if (Life <= 0f) return false;

            // simple translation + gravity
            Vector3 pos = World.Translation;
            pos += Velocity * dt;
            Velocity += new Vector3(0, -980f, 0) * dt; // gravity (units/sec^2), ajuster si nécessaire

            // delta rotation
            var deltaRot = Matrix.CreateFromYawPitchRoll(AngularVelocity.Y * dt, AngularVelocity.X * dt, AngularVelocity.Z * dt);

            // Decompose current world to preserve scale, apply delta rotation to base rotation, then recompose.
            if (World.Decompose(out Vector3 scale, out Quaternion baseRotation, out Vector3 _))
            {
                Matrix baseRotMat = Matrix.CreateFromQuaternion(baseRotation);
                World = Matrix.CreateScale(scale) * deltaRot * baseRotMat;
                World.Translation = pos;
            }
            else
            {
                // fallback: rotate the whole matrix and preserve translation
                World = deltaRot * World;
                World.Translation = pos;
            }

            return true;
        }

        public float Alpha => System.MathF.Max(0f, Life / _initialLife);
    }
}
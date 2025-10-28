using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public class TrafficCar
    {
        public Vector3 Position { get; private set; }
        public float Speed { get; private set; }
        public Matrix World { get; private set; }
        public BoundingSphere BoundingSphere { get; private set; }
        public Model CarModel { get; private set; }

        private const float TrafficCarRadius = 20f;
        private readonly Vector3 _scale = new Vector3(0.1f);
        // Version simple: se mueven en +Z
        private readonly Vector3 _direction = Vector3.Backward;

        public TrafficCar(Model model, Vector3 startPosition, float speed)
        {
            CarModel = model;
            Position = startPosition;
            Speed = speed;
            UpdateWorldMatrix();
            BoundingSphere = new BoundingSphere(Position, TrafficCarRadius);
        }

        public void Update(float deltaTime)
        {
            // el auto se mueve en +Z
            Position += _direction * Speed * deltaTime;
            UpdateWorldMatrix();
            BoundingSphere = new BoundingSphere(Position, TrafficCarRadius);
        }

        private void UpdateWorldMatrix()
        {
            // We add Pi rotation on Y because our cars likely face -Z, but move towards +Z
            World = Matrix.CreateScale(_scale) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(Position);
        }
    }
}
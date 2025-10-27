using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public enum CollectibleType
    {
        Coin,
        Gas,
        Wrench
    }

    public class Collectible
    {
        public CollectibleType Type { get; private set; }
        public Vector3 Position { get; private set; }
        public Matrix World { get; set; }
        public BoundingSphere BoundingSphere { get; private set; }
        public bool IsActive { get; set; }

        // El radio de la esfera de colisión
        private const float CoinRadius = 3f;
        private const float GasRadius = 5f;
        private const float WrenchRadius = 0.6f;

        // Variables para la animación de rotación
        private float _animationRotation;
        private const float RotationSpeed = 1.5f; // Velocidad de giro (en radianes por segundo)
        private readonly Matrix _initialRotation = Matrix.Identity; // Guardamos la rotación estática inicial
        private readonly Vector3 _scale;

        public Collectible(CollectibleType type, Vector3 position)
        {
            Type = type;
            Position = position + Vector3.Up * 15f;
            IsActive = true;
            _animationRotation = 0f;

            float radius;

            switch (Type)
            {
                case CollectibleType.Coin:
                    _scale = new Vector3(40f);
                    radius = CoinRadius;
                    _initialRotation = Matrix.CreateRotationZ(MathHelper.PiOver2);
                    break;
                case CollectibleType.Gas:
                    _scale = new Vector3(4f);
                    radius = GasRadius;
                    break;
                case CollectibleType.Wrench:
                    _scale = new Vector3(0.2f);
                    radius = WrenchRadius;
                    _initialRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(45f));
                    break;
                default:
                    _scale = new Vector3(1f);
                    radius = 1f;
                    break;
            }

            BoundingSphere = new BoundingSphere(Position, radius);
        }

        public void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _animationRotation += elapsedTime * RotationSpeed;

            var animationMatrix = Matrix.CreateRotationY(_animationRotation);

            World = Matrix.CreateScale(_scale) * _initialRotation * animationMatrix * Matrix.CreateTranslation(Position);
        }
    }
}
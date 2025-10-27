using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public enum ObstacleType
    {
        Cow,
        Deer,
        Goat
    }

    public class Obstacle
    {
        public ObstacleType Type { get; private set; }
        public Matrix World { get; set; }
        public BoundingSphere BoundingSphere { get; private set; }

        private readonly Vector3 _position;
        private readonly Vector3 _scale;
        private readonly Matrix _initialRotation; // Rotación aleatoria en Y
        private readonly Matrix _correctiveRotation; // Rotación para arreglar el modelo

        private const float ObstacleRadius = 2f;

        public Obstacle(ObstacleType type, Vector3 spawnPoint)
        {
            Type = type;
            _position = spawnPoint;

            _correctiveRotation = Matrix.Identity;

            switch (Type)
            {
                case ObstacleType.Cow:
                    _scale = new Vector3(0.04f);
                    break;

                case ObstacleType.Deer:
                    _scale = new Vector3(0.05f);
                    _correctiveRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-90f));
                    break;

                case ObstacleType.Goat:
                    _scale = new Vector3(0.05f);
                    _correctiveRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-90f));
                    break;

                default:
                    _scale = Vector3.One;
                    break;
            }

            // Le damos una rotación inicial aleatoria en el eje Y para que no miren todos igual
            var random = new Random();
            _initialRotation = Matrix.CreateRotationY(MathHelper.ToRadians(random.Next(0, 360)));

            World = Matrix.CreateScale(_scale) * _correctiveRotation * _initialRotation * Matrix.CreateTranslation(_position);

            BoundingSphere = new BoundingSphere(_position, ObstacleRadius);
        }

    }
}
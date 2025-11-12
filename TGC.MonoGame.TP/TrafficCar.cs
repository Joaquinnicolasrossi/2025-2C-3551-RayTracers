using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public class TrafficCar
    {
        public Matrix World { get; private set; }
        public BoundingSphere BoundingSphere { get; private set; }
        public Model CarModel { get; private set; }

        private List<Vector3> _path; // conjunto de waypoints
        private int _currentWaypointIndex;
        private Vector3 _position;
        private float _speed;
        private const float WaypointArrivalThreshold = 50f; // cercania necesaria para pasar al prox waypoint
        private const float TrafficCarRadius = 20f;
        private const float DespawnDistanceSquared = 5000f * 5000f;
        private readonly Vector3 _scale = new Vector3(0.1f);

        public TrafficCar(Model model, List<Vector3> path, float speed)
        {
            CarModel = model;
            _path = path;
            _speed = speed;

            _position = _path[0]; // empieza en el primer waypoint
            _currentWaypointIndex = 1; // se mueve hacia el segundo

            UpdateWorldMatrix();
            BoundingSphere = new BoundingSphere(_position, TrafficCarRadius);
        }

        /// <summary>
        /// Mueve el auto hacia el siguiente waypoint.
        /// </summary>
        /// <returns>True si todavia tiene camino por recorrer</returns>
        public bool Update(float deltaTime, Vector3 playerPosition)
        {
            if (Vector3.DistanceSquared(_position, playerPosition) > DespawnDistanceSquared)
            {
                return false; // si esta muy lejos
            }

            if (_currentWaypointIndex >= _path.Count)
            {
                return false; // si ya termino el camino
            }

            Vector3 target = _path[_currentWaypointIndex];
            Vector3 direction = Vector3.Normalize(target - _position);
            _position += direction * _speed * deltaTime;
            UpdateWorldMatrix(direction);
            BoundingSphere = new BoundingSphere(_position, TrafficCarRadius);

            if (Vector3.DistanceSquared(_position, target) < WaypointArrivalThreshold * WaypointArrivalThreshold)
            {
                _currentWaypointIndex++;
            }

            return true;
        }

        private void UpdateWorldMatrix(Vector3 direction = default)
        {
            Matrix rotationMatrix = Matrix.Identity;

            if (direction != default)
            {
                // CreateLookAt y la invierto (orienta)
                Vector3 lookAtTarget = _position + direction;
                rotationMatrix = Matrix.Invert(Matrix.CreateLookAt(_position, lookAtTarget, Vector3.Up));
                rotationMatrix.Translation = Vector3.Zero; // solo rotación
            }

            // Combinamos escala, rotación y posición
            // (Tu lógica de giro de 180° puede ser necesaria aquí si los modelos miran para atrás)
            // Matrix flip = Matrix.CreateRotationY(MathHelper.Pi); 
            World = Matrix.CreateScale(_scale) * rotationMatrix * Matrix.CreateTranslation(_position);
        }
    }
}
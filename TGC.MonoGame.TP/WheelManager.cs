using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    internal class WheelManager
    {
        private readonly List<WheelPiece> _pieces = new();
        private readonly HashSet<string> _detachedMeshNames = new(StringComparer.OrdinalIgnoreCase);

        // Distance (en unités monde) pour écarter les roues du corps de la voiture.
        // Modifiable depuis l'extérieur si nécessaire.
        public float WheelLateralSeparation { get; set; } = 10f;

        public IReadOnlyCollection<string> DetachedMeshNames => _detachedMeshNames;

        public void Update(float dt)
        {
            for (int i = _pieces.Count - 1; i >= 0; i--)
            {
                if (!_pieces[i].Update(dt))
                    _pieces.RemoveAt(i);
            }
        }

        // Recherche les meshes "wheel" et les détache : crée des WheelPiece
        // Comportement déterministe : pas d'aléatoire.
        // Chaque roue garde la même taille qu'à l'origine et est uniquement décalée latéralement.
        public void DetachWheels(Microsoft.Xna.Framework.Graphics.Model carModel, Microsoft.Xna.Framework.Matrix carWorld, Microsoft.Xna.Framework.Vector3 carForward, float carSpeed, int piecesPerCar = 1, float lifeSeconds = 4f, float outwardImpulse = 5f)
        {
            if (carModel == null) return;

            // Mots-clés pour identifier les meshes/parents de roues (ajouter d'autres variantes si besoin)
            string[] wheelKeywords = new[] { "wheel_FL", "wheel_FR", "wheel_BL", "wheel_BR", "WheelA", "WheelB", "WheelC", "WheelD" };

            // Offsets locaux déterministes si piecesPerCar > 1 (petites variantes fixes le long de l'axe local avant/arrière)
            Vector3[] deterministicLocalOffsets = new[]
            {
                Vector3.Zero,
                new Vector3(0f, 0f, 0.05f),
                new Vector3(0f, 0f, -0.05f),
                new Vector3(0f, 0f, 0.10f)
            };

            foreach (var mesh in carModel.Meshes)
            {
                string meshName = mesh.Name ?? "";
                string parentName = mesh.ParentBone?.Name ?? "";
                string idLower = (meshName + "|" + parentName).ToLowerInvariant();

                bool isWheel = wheelKeywords.Any(k => idLower.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!isWheel) continue;

                // empêcher détachement doublon
                if (_detachedMeshNames.Contains(mesh.Name) || _detachedMeshNames.Contains(mesh.ParentBone?.Name)) continue;
                _detachedMeshNames.Add(mesh.Name);
                if (!string.IsNullOrEmpty(mesh.ParentBone?.Name))
                    _detachedMeshNames.Add(mesh.ParentBone.Name);

                // transform de base du mesh dans le monde (préserve rotation/échelle du modèle)
                Matrix boneCarWorld = mesh.ParentBone.Transform * carWorld;

                // vecteur "droite" stable en espace monde pour effectuer l'écartement
                Vector3 carRight = Vector3.Normalize(Vector3.Cross(Vector3.Up, carForward));
                if (float.IsNaN(carRight.Length()) || carRight.LengthSquared() < 1e-6f)
                    carRight = Vector3.Right;

                // heuristique pour déterminer côté : gauche -> -1, droite -> +1
                int sideSign = 1;
                if (idLower.Contains("_l") || idLower.Contains(" left") || idLower.Contains(".l") || idLower.Contains(" left") || idLower.Contains("fl") || idLower.Contains("bl") || idLower.Contains("left"))
                    sideSign = -1;
                else if (idLower.Contains("_r") || idLower.Contains(" right") || idLower.Contains(".r") || idLower.Contains("fr") || idLower.Contains("br") || idLower.Contains("right"))
                    sideSign = +1;

                // offset latéral déterministe en espace monde
                Vector3 lateralOffsetWorld = carRight * (sideSign * WheelLateralSeparation);

                // vélocité et rotation initiales déterministes : pas d'expulsion dynamique par défaut.
                // Si vous voulez un léger mouvement, ajustez outwardImpulse (non aléatoire).
                Vector3 vel = Vector3.Zero;
                if (outwardImpulse != 0f)
                {
                    // pousse légèrement vers l'extérieur si demandé
                    vel = carRight * (sideSign * outwardImpulse) + carForward * (carSpeed * 0.2f);
                }

                Vector3 ang = Vector3.Zero;

                for (int i = 0; i < piecesPerCar; i++)
                {
                    // offset local déterministe si plusieurs "morceaux" par roue sont demandés
                    Vector3 localOffset = deterministicLocalOffsets[i % deterministicLocalOffsets.Length];

                    // construire la world initiale : appliquer l'offset local en espace bone puis la transform du bone et du véhicule
                    Matrix baseWithLocal = Matrix.CreateTranslation(localOffset) * mesh.ParentBone.Transform * carWorld;
                    Matrix pieceInitialWorld = baseWithLocal;

                    // appliquer uniquement l'écartement latéral (en espace monde)
                    pieceInitialWorld.Translation += lateralOffsetWorld;

                    // créer la pièce : même taille que l'original (NO scale)
                    var wp = new WheelPiece(mesh, pieceInitialWorld, vel, ang, lifeSeconds);
                    _pieces.Add(wp);
                }
            }
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.Effect effect, Microsoft.Xna.Framework.Matrix view, Microsoft.Xna.Framework.Matrix projection, Microsoft.Xna.Framework.Vector3 cameraPos, Microsoft.Xna.Framework.Matrix lightViewProj, Microsoft.Xna.Framework.Graphics.Texture2D shadowMap, Color? color = null)
        {
            foreach (var p in _pieces)
            {
                ModelDrawingHelper.MeshDrawingHelper.DrawMesh(p.Mesh, p.World, view, projection, effect, cameraPos, lightViewProj, shadowMap, color);
            }
        }

        public HashSet<string> GetDetachedMeshNames() => new(_detachedMeshNames, StringComparer.OrdinalIgnoreCase);
    }
}
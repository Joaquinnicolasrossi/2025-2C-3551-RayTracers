using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    /// <summary>
    /// Helper para dibujar modelos pasando un efecto y matrices.
    /// </summary>
    public static class ModelDrawingHelper
    {
        /// <summary>
        /// Asigna la misma instancia de effect a todos los MeshParts del modelo
        /// </summary>
        public static void AttachEffectToModel(Model model, Effect effect)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }
        }

        /// <summary>
        /// Dibuja el modelo usando el Effect que se pasa por parametro
        /// </summary>
        public static void Draw(Model model, Matrix modelWorld, Matrix view, Matrix projection, Color color, Effect effect)
        {
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);
            effect.Parameters["DiffuseColor"]?.SetValue(color.ToVector3());

            foreach (var mesh in model.Meshes)
            {
                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * modelWorld);
                mesh.Draw();
            }
        }
    }
}
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
    }
}
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
        /// Dibuja el modelo usando el BasicShader
        /// </summary>
        public static void Draw(Model model, Matrix modelWorld, Matrix view, Matrix projection, Color color, Effect effect)
        {
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);
            effect.Parameters["UseTexture"]?.SetValue(0);
            effect.Parameters["DiffuseColor"]?.SetValue(color.ToVector3());
            effect.Parameters["LightDirection"]?.SetValue(new Vector3(1, -1, 1)); // hacia abajo adelante der
            effect.Parameters["AmbientColor"]?.SetValue(new Vector3(0.2f));
            effect.Parameters["SpecularColor"]?.SetValue(new Vector3(1));
            effect.Parameters["Shininess"]?.SetValue(32f);

            foreach (var mesh in model.Meshes)
            {
                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * modelWorld);
                mesh.Draw();
            }
        }

        // Dibuja el modelo usando el BasicShader pero con texturas
        public static void Draw(Model model, Matrix modelWorld, Matrix view, Matrix projection, Texture2D texture, Effect effect)
        {
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);
            effect.Parameters["UseTexture"]?.SetValue(1);
            effect.Parameters["MainTexture"]?.SetValue(texture);
            effect.Parameters["LightDirection"]?.SetValue(new Vector3(1, -1, 1));
            effect.Parameters["AmbientColor"]?.SetValue(new Vector3(0.2f));
            effect.Parameters["SpecularColor"]?.SetValue(new Vector3(1));
            effect.Parameters["Shininess"]?.SetValue(32f);

            foreach (var mesh in model.Meshes)
            {
                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * modelWorld);
                mesh.Draw();
            }
        }

        // Dibuja con color, iluminacion y sombras, sin textura
        public static void Draw(Model model, Matrix modelWorld, Matrix view, Matrix projection, Color color, Effect effect, Vector3 cameraPosition, Matrix lightViewProj, Texture2D shadowMap)
        {
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);
            effect.Parameters["CameraPosition"]?.SetValue(cameraPosition);
            effect.Parameters["LightDirection"]?.SetValue(new Vector3(1, -1, 1));
            effect.Parameters["LightViewProj"]?.SetValue(lightViewProj);
            effect.Parameters["ShadowMap"]?.SetValue(shadowMap);

            effect.Parameters["UseTexture"]?.SetValue(0);
            effect.Parameters["DiffuseColor"]?.SetValue(color.ToVector3());
            effect.Parameters["AmbientColor"]?.SetValue(new Vector3(0.2f));
            effect.Parameters["SpecularColor"]?.SetValue(new Vector3(1));
            effect.Parameters["Shininess"]?.SetValue(32f);

            foreach (var mesh in model.Meshes)
            {
                // Asegurarnos que cada MeshPart use EL effect que queremos (evita "estado sucio")
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = effect;

                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * modelWorld);
                mesh.Draw();
            }
        }

        // Dibuja con textura
        public static void Draw(Model model, Matrix modelWorld, Matrix view, Matrix projection, Texture2D texture, Effect effect, Vector3 cameraPosition, Matrix lightViewProj, Texture2D shadowMap)
        {
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection);
            effect.Parameters["CameraPosition"]?.SetValue(cameraPosition);
            effect.Parameters["UseTexture"]?.SetValue(1f);
            effect.Parameters["MainTexture"]?.SetValue(texture);
            effect.Parameters["LightDirection"]?.SetValue(new Vector3(1, -1, 1));
            effect.Parameters["LightViewProj"]?.SetValue(lightViewProj);
            effect.Parameters["ShadowMap"]?.SetValue(shadowMap);
            effect.Parameters["AmbientColor"]?.SetValue(new Vector3(0.2f));
            effect.Parameters["SpecularColor"]?.SetValue(new Vector3(1));
            effect.Parameters["Shininess"]?.SetValue(32f);

            foreach (var mesh in model.Meshes)
            {
                // Asegurarnos que cada MeshPart use EL effect que queremos (evita "estado sucio")
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = effect;

                effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * modelWorld);
                mesh.Draw();
            }
        }

        // Dibuja solo profundidad para shadow pass
        public static void DrawDepth(Model model, Matrix modelWorld, Effect depthEffect)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = depthEffect;

                depthEffect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * modelWorld);
                mesh.Draw();
            }
        }
    }
}
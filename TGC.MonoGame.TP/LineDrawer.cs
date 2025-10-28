using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP // Use your project's namespace
{
    public static class LineDrawer
    {
        private static VertexPositionColor[] _verts = new VertexPositionColor[2];
        private static BasicEffect _effect;

        public static void LoadContent(GraphicsDevice graphicsDevice)
        {
            _effect = new BasicEffect(graphicsDevice);
            _effect.VertexColorEnabled = true;
        }

        public static void DrawLine(GraphicsDevice graphicsDevice, Vector3 start, Vector3 end, Color color, Matrix view, Matrix projection)
        {
            if (_effect == null)
                throw new System.InvalidOperationException("LineDrawer.LoadContent must be called before drawing lines.");

            _verts[0] = new VertexPositionColor(start, color);
            _verts[1] = new VertexPositionColor(end, color);

            _effect.View = view;
            _effect.Projection = projection;
            _effect.World = Matrix.Identity;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _verts, 0, 1);
            }
        }
    }
}
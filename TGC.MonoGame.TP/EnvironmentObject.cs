using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public enum EnvironmentObjectType
    {
        House,
        Tree, // todavia sin implementar
        Rock,
        Plant
    }

    public class EnvironmentObject
    {
        public EnvironmentObjectType Type { get; private set; }
        public Matrix World { get; private set; }
        public BoundingSphere BoundingSphere { get; private set; }
        private const float HouseRadius = 150f;
        private const float TreeRadius = 2f;
        private const float RockRadius = 2f;
        private const float PlantRadius = 10f;

        public EnvironmentObject(EnvironmentObjectType type, Matrix worldMatrix)
        {
            Type = type;
            World = worldMatrix;

            float radius;
            switch (Type)
            {
                case EnvironmentObjectType.House:
                    radius = HouseRadius;
                    break;
                case EnvironmentObjectType.Tree:
                    radius = TreeRadius;
                    break;
                case EnvironmentObjectType.Rock:
                    radius = RockRadius;
                    break;
                case EnvironmentObjectType.Plant:
                    radius = PlantRadius;
                    break;
                default:
                    radius = 1f;
                    break;
            }
            BoundingSphere = new BoundingSphere(World.Translation, radius);
        }
    }
}
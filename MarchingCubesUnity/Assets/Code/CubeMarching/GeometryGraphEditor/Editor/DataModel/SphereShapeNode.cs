using Code.CubeMarching.GeometryComponents;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    public class SphereShapeNode : ShapeNode<CShapeSphere>
    {
        public override string Title
        {
            get => "Sphere";
            set { }
        }

        public IPortModel RadiusIn { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            RadiusIn = this.AddDataInputPort<float>(nameof(RadiusIn), defaultValue:  8);
        }

        protected override TerrainModifierType GetShapeType()
        {
            return TerrainModifierType.Sphere;
        }

        protected override CShapeSphere GetShape()
        {
            return new() {radius = RadiusIn.GetValue()};
        }
    }
}
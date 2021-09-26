using Code.CubeMarching.GeometryComponents;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public class SubtractGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.SmoothSubtract;
    }
}
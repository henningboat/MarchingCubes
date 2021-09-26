using Code.CubeMarching.GeometryComponents;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public class MinGeometryCombinerNode : SymmetricalGeometryCombinerNode
    {
        protected override CombinerOperation CombinerOperation => CombinerOperation.Min;
    }
}
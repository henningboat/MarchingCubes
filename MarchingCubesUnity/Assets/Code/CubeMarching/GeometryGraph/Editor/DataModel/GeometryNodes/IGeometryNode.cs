using Code.CubeMarching.GeometryGraph.Editor.Conversion;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes
{
    public interface IGeometryNode
    {
        void Resolve(GeometryGraphResolverContext context, GeometryTransformationInstruction parent);
    }
}
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    public class GeometryGraphExposedVariableNode : GeometryGraphProperty
    {
        public IVariableDeclarationModel Variable;

        public GeometryGraphExposedVariableNode(IVariableDeclarationModel variableDeclarationModel, object defaultValue, GeometryGraphResolverContext context,
            GeometryPropertyType geometryPropertyType, string variableName,
            string debugInformation) : base(
            geometryPropertyType, debugInformation)
        {
            Variable = variableDeclarationModel;
            Value = defaultValue;
        }
    }
}
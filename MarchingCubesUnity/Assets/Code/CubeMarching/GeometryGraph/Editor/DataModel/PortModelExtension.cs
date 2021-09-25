using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    public static class PortModelExtension
    {
        public static float GetValue(this IPortModel self)
        {
            if (self == null)
                return 0;
            var node = self.GetConnectedEdges().FirstOrDefault()?.FromPort.NodeModel;

            switch (node)
            {
                case MathNode mathNode:
                    return mathNode.Evaluate();
                case IVariableNodeModel varNode:
                    return (float)varNode.VariableDeclarationModel.InitializationModel.ObjectValue;
                case IConstantNodeModel constNode:
                    return (float)constNode.ObjectValue;
                case IEdgePortalExitModel portalModel:
                    var oppositePortal = portalModel.GraphModel.FindReferencesInGraph<IEdgePortalEntryModel>(portalModel.DeclarationModel).FirstOrDefault();
                    if (oppositePortal != null)
                    {
                        return oppositePortal.InputPort.GetValue();
                    }
                    return 0;
                default:
                    return (float)self.EmbeddedValue.ObjectValue;
            }
        }

        public static Vector3 GetValue3(this IPortModel self)
        {
            if (self == null)
                return default;
            var node = self.GetConnectedEdges().FirstOrDefault()?.FromPort.NodeModel;

            switch (node)
            {
                
                case IVariableNodeModel varNode:
                    return (Vector3) varNode.VariableDeclarationModel.InitializationModel.ObjectValue;
                case IConstantNodeModel constNode:
                    return (Vector3) constNode.ObjectValue;
                case IEdgePortalExitModel portalModel:
                    var oppositePortal = portalModel.GraphModel.FindReferencesInGraph<IEdgePortalEntryModel>(portalModel.DeclarationModel).FirstOrDefault();
                    if (oppositePortal != null)
                    {
                        return oppositePortal.InputPort.GetValue3();
                    }

                    return default;
                default:
                    return (Vector3) self.EmbeddedValue.ObjectValue;
            }
        }
    }
}

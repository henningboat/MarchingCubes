using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes;
using Code.CubeMarching.GeometryGraph.Editor.GraphElements;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    [GraphElementsExtensionMethodsCache(typeof(ModelInspectorView))]
    public static class ModelInspectorViewFactoryExtensions
    {
        public static IModelUI CreateNodeInspector(this ElementBuilder elementBuilder, CommandDispatcher commandDispatcher, MathOperator model)
        {
            var ui = new ModelInspector();
            ui.Setup(model, commandDispatcher, elementBuilder.View as ModelInspectorView, elementBuilder.Context);

            var nodeInspectorFields = NodeFieldsInspector.Create("node-fields", model, ui, ModelInspector.ussClassName);
            ui.PartList.AppendPart(nodeInspectorFields);

            var mathOpInspectorFields = MathOperatorInspector.Create("mathop-fields", model, ui, ModelInspector.ussClassName);
            ui.PartList.AppendPart(mathOpInspectorFields);

            ui.BuildUI();
            ui.UpdateFromModel();

            return ui;
        }
    }
}
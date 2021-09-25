using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.GraphElements
{
    public class GeometryGraphBBVarPropertyView : BlackboardVariablePropertyView
    {
        protected override void BuildRows()
        {
            AddInitializationField();
            AddTooltipField();
        }

    }
}

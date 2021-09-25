using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine.UIElements;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    public class MathBookOnboardingProvider : OnboardingProvider
    {
        public override VisualElement CreateOnboardingElements(CommandDispatcher store)
        {
            var template = new GraphTemplate<MathBookStencil>(MathBookStencil.GraphName);
            return AddNewGraphButton<GeometryGraphAsset>(template);
        }
    }
}

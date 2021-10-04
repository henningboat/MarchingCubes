using System.Collections.Generic;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    class SimpleGraphViewWindow : GraphViewEditorWindow
    {
        [InitializeOnLoadMethod]
        static void RegisterTool()
        {
            ShortcutHelper.RegisterDefaultShortcuts<SimpleGraphViewWindow>(GeometryGraphStencil.GraphName);
        }

        [MenuItem("Window/Geometry Graph Editor")]
        public static void ShowWindow()
        {
            GetWindow<SimpleGraphViewWindow>();
        }

        protected override void OnEnable()
        {
            EditorToolName = "Geometry Graph";
            base.OnEnable();
        }

        protected override GraphToolState CreateInitialState()
        {
            var prefs = Preferences.CreatePreferences(EditorToolName);
            return new GeometryGraphState(GUID, prefs);
        }

        protected override GraphView CreateGraphView()
        {
            return new GraphView(this, CommandDispatcher, EditorToolName);
        }

        protected override BlankPage CreateBlankPage()
        {
            var onboardingProviders = new List<OnboardingProvider>();
            onboardingProviders.Add(new GeometryGraphOnboardingProvider());

            return new BlankPage(CommandDispatcher, onboardingProviders);
        }

        protected override bool CanHandleAssetType(IGraphAssetModel asset)
        {
            return asset is GeometryGraphAsset;
        }

        protected override MainToolbar CreateMainToolbar()
        {
            return new GeometryGraphMainToolbar(CommandDispatcher, GraphView);
        }
    }
}

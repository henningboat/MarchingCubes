using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.TerrainChunkEntitySystem;
using UnityEditor.Callbacks;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    [Serializable]
    public class MathBookAsset : GraphAssetModel
    {
        protected override Type GraphModelType => typeof(MathBook);
        
        [MenuItem("Assets/Create/Math Book")]
        public static void CreateGraph(MenuCommand menuCommand)
        {
            const string path = "Assets";
            var template = new GraphTemplate<MathBookStencil>(MathBookStencil.GraphName);
            CommandDispatcher commandDispatcher = null;
            if (EditorWindow.HasOpenInstances<SimpleGraphViewWindow>())
            {
                var window = EditorWindow.GetWindow<SimpleGraphViewWindow>();
                if (window != null)
                {
                    commandDispatcher = window.CommandDispatcher;
                }
            }

            GraphAssetCreationHelpers<MathBookAsset>.CreateInProjectWindow(template, commandDispatcher, path);
        }

        [OnOpenAsset(1)]
        public static bool OpenGraphAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is MathBookAsset graphAssetModel)
            {
                var window = GraphViewEditorWindow.FindOrCreateGraphWindow<SimpleGraphViewWindow>();
                window.SetCurrentSelection(graphAssetModel, GraphViewEditorWindow.OpenMode.OpenAndFocus);
                return window != null;
            }

            return false;
        }

        public List<GeometryInstruction> GetInstructions()
        {
            List<GeometryInstruction> results = new List<GeometryInstruction>();
            var resultsNode = GraphModel.NodeModels.FirstOrDefault(model => model is MathResult) as MathResult;
            resultsNode.AddInstructions(results);
            return results;
        }
    }
}

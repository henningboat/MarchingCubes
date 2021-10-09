using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Entities;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.UIElements;

namespace Code.CubeMarching.GeometryGraph.Editor.GraphElements
{
    public class MathResultUI : CollapsibleInOutNode
    {
        public static readonly string printResultPartName = "print-result";

        protected override void BuildPartList()
        {
            base.BuildPartList();

            PartList.AppendPart(PrintResultPart.Create(printResultPartName, Model, this, ussClassName));
        }
    }
}

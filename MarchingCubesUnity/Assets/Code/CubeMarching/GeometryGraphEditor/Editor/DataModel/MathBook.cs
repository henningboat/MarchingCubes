using System;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    [Serializable]
    public class MathBook : GraphModel
    {
        public MathBook()
        {
            StencilType = null;
        }
        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
        }

        public override Type DefaultStencilType => typeof(MathBookStencil);
    }
}

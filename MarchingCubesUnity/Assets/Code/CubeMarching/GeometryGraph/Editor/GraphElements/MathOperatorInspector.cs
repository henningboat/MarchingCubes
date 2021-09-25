using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.GraphElements.Commands;
using UnityEditor.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.GraphElements
{
    class MathOperatorInspector : FieldsInspector
    {
        public static MathOperatorInspector Create(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
        {
            if (model is MathOperator)
            {
                return new MathOperatorInspector(name, model, ownerElement, parentClassName);
            }

            return null;
        }

        /// <inheritdoc />
        public MathOperatorInspector(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
            : base(name, model, ownerElement, parentClassName) { }

        /// <inheritdoc />
        protected override IEnumerable<BaseModelPropertyField> GetFields()
        {
            if (m_Model is MathOperator mathOperator)
            {
                yield return new ModelPropertyField<int>(
                    m_OwnerElement.CommandDispatcher,
                    mathOperator,
                    nameof(mathOperator.InputPortCount),
                    "Number of Inputs",
                    typeof(SetNumberOfInputPortCommand));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class GeometryGraphResolverContext
    {
        private Dictionary<SerializableGUID, GeometryGraphProperty> _properties = new();
        private List<GeometryGraphInstruction> _instructions = new();

        public int CurrentCombinerDepth => _combinerStack.Count - 1;
        private List<float> _propertyValueBuffer;
        private List<GeometryInstruction> _geometryInstructionBuffer;
        private List<MathInstruction> _mathInstructionsBuffer;
        private List<GeometryTransformationInstruction> _transformations;

        public List<float> PropertyValueBuffer => _propertyValueBuffer;
        public List<MathInstruction> MathInstructionBuffer => _mathInstructionsBuffer;

        public List<GeometryInstruction> GeometryInstructionBuffer => _geometryInstructionBuffer;

        private Stack<CombinerInstruction> _combinerStack = new();
        private GeometryGraphProperty _zeroFloatProperty;
        private List<GeometryTransformation> _translationsBuffer;

        public CombinerInstruction CurrentCombiner => _combinerStack.Peek();
        public GeometryTransformationInstruction OriginTransformation { get; }


        public GeometryGraphResolverContext()
        {
            _zeroFloatProperty = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(0.0f, this, GeometryPropertyType.Float, "Zero Float Constant"));
            _combinerStack.Push(new CombinerInstruction(CombinerOperation.Min, _zeroFloatProperty, 0));
            _transformations = new List<GeometryTransformationInstruction>();
            OriginTransformation = GeometryTransformationInstruction.Origin(_zeroFloatProperty);
            _transformations.Add(OriginTransformation);
        }

        public void BeginWriteCombiner(CombinerInstruction combiner)
        {
            _combinerStack.Push(combiner);
        }

        public void FinishWritingCombiner()
        {
            var combinerToFinish = _combinerStack.Pop();
            _instructions.Add(combinerToFinish);
        }

        public void WriteShape(ShapeType shapeType, GeometryTransformationInstruction transformation, List<GeometryGraphProperty> getProperties)
        {
            _instructions.Add(new ShapeInstruction(shapeType, transformation, getProperties, CurrentCombinerDepth, CurrentCombiner));
        }

        public GeometryGraphExposedVariableNode GetExposedVariableProperty(SerializableGUID guid)
        {
            return _properties.Values.Where(property => property is GeometryGraphExposedVariableNode).FirstOrDefault(property => ((GeometryGraphExposedVariableNode) property).Variable.Guid == guid) as
                GeometryGraphExposedVariableNode;
        }

        public GeometryTransformationInstruction PushTranslation(GeometryGraphProperty translationProperty, GeometryTransformationInstruction parent)
        {
            var transformationInstruction = new GeometryTransformationInstruction(GeometryTransformationType.Translation, translationProperty, parent);
            _transformations.Add(transformationInstruction);
            return transformationInstruction;
        }

        public GeometryTransformationInstruction PushEulerRotationInstruction(GeometryGraphProperty eulerAnglesProperty, GeometryTransformationInstruction parent)
        {
            var transformationInstruction = new GeometryTransformationInstruction(GeometryTransformationType.EulerRotation,eulerAnglesProperty, parent);        
            _transformations.Add(transformationInstruction);
            return transformationInstruction;
        }

        public GeometryTransformationInstruction PushScaleInstruction(GeometryGraphProperty scaleProperty, GeometryTransformationInstruction parent)
        {
            var translation = new GeometryTransformationInstruction(GeometryTransformationType.Scale,scaleProperty, parent);
            return translation;
        }

        public GeometryGraphProperty GetOrCreateProperty(SerializableGUID guid, GeometryGraphProperty newProperty)
        {
            if (_properties.TryGetValue(guid, out var existingProperty))
            {
                return existingProperty;
            }
            else
            {
                _properties[guid] = newProperty;
                return newProperty;
            }
        }

        public void BuildBuffers()
        {
            _propertyValueBuffer = new List<float>();
            var geometryGraphProperties = _properties.Values;
            foreach (var property in geometryGraphProperties)
            {
                property.Index = _propertyValueBuffer.Count;
                switch (property.Type)
                {
                    case GeometryPropertyType.Float:
                        var constantFloatValue = property.GetValue<float>();
                        _propertyValueBuffer.Add(constantFloatValue);
                        break;
                    case GeometryPropertyType.Float3:
                        var constantFloat3Value = property.GetValue<Vector3>();
                        _propertyValueBuffer.Add(constantFloat3Value.x);
                        _propertyValueBuffer.Add(constantFloat3Value.y);
                        _propertyValueBuffer.Add(constantFloat3Value.z);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _mathInstructionsBuffer = new List<MathInstruction>();
            foreach (var geometryGraphProperty in geometryGraphProperties)
            {
                if (geometryGraphProperty is GeometryGraphMathOperatorProperty mathOperator)
                {
                    _mathInstructionsBuffer.Add(mathOperator.GetMathInstruction());
                }
            }

            _translationsBuffer = new List<GeometryTransformation>();
            for (var i = 0; i < _transformations.Count; i++)
            {
                _transformations[i].Index = i;
            }

            foreach (var transformationInstruction in _transformations)
            {
                _translationsBuffer.Add(new GeometryTransformation()
                {
                    Type = transformationInstruction.Type,
                    Value = new FloatValue() {Index = transformationInstruction.Value.Index},
                    ParentIndex = transformationInstruction.Type == GeometryTransformationType.Origin ? 0 : transformationInstruction.Parent.Index,
                });
            }

            _geometryInstructionBuffer = new List<GeometryInstruction>();
            for (var i = 0; i < _instructions.Count; i++)
            {
                GeometryInstructionBuffer.Add(_instructions[i].GetInstruction());
            }
        }
    }

    public struct GeometryTransformation
    {
        public GeometryTransformationType Type;
        public FloatValue Value;
        public int ParentIndex;
    }

    public enum GeometryTransformationType
    {
        Translation = 1,
        EulerRotation = 2,
        Scale = 3,
        Origin
    }
}
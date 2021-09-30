using System;
using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class GeometryGraphResolverContext
    {
        private Dictionary<SerializableGUID, GeometryGraphProperty> _properties = new();
        private List<GeometryGraphInstruction> _instructions = new();

        public int CurrentCombinerDepth => _combinerStack.Count-1;
        private List<float> _propertyValueBuffer;
        private List<GeometryInstruction> _geometryInstructionBuffer;
        private List<MathInstruction> _mathInstructionsBuffer;

        public List<float> PropertyValueBuffer => _propertyValueBuffer;
        public List<MathInstruction> MathInstructionBuffer => _mathInstructionsBuffer;

        public List<GeometryInstruction> GeometryInstructionBuffer => _geometryInstructionBuffer;

        private Stack<CombinerInstruction> _combinerStack = new();
        private GeometryGraphProperty _zeroFloatProperty;

        public CombinerInstruction CurrentCombiner => _combinerStack.Peek();
        
        
        public GeometryGraphResolverContext()
        {
            _zeroFloatProperty = GetOrCreateProperty(SerializableGUID.Generate(), new GeometryGraphConstantProperty(0.0f, this, GeometryPropertyType.Float, "Zero Float Constant"));
            _combinerStack.Push(new CombinerInstruction(CombinerOperation.Min, _zeroFloatProperty, 0));
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

        public void WriteShape(ShapeType shapeType, GeometryGraphProperty positionProperty, List<GeometryGraphProperty> getProperties)
        {
            _instructions.Add(new ShapeInstruction(shapeType, positionProperty, getProperties, CurrentCombinerDepth, CurrentCombiner));
        }

        public GeometryGraphExposedVariableNode GetExposedVariableProperty(SerializableGUID guid)
        {
            return _properties.Values.Where(property => property is GeometryGraphExposedVariableNode).FirstOrDefault(property => ((GeometryGraphExposedVariableNode) property).Variable.Guid == guid) as
                GeometryGraphExposedVariableNode;
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

            _geometryInstructionBuffer = new List<GeometryInstruction>();
            for (var i = 0; i < _instructions.Count; i++)
            {
                GeometryInstructionBuffer.Add(_instructions[i].GetInstruction());
            }
        }

        public void WriteTransformation()
        {
            throw new NotImplementedException();
        }
    }
}
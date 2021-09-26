using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public class GeometryGraphResolverContext
    {
        internal abstract class GeometryGraphInstruction
        {
            public readonly int Depth;

            protected GeometryGraphInstruction(int depth)
            {
                Depth = depth;
            }

            public abstract GeometryInstruction GetInstruction();
        }

        private Dictionary<SerializableGUID, GeometryGraphProperty> _properties = new();
        private List<GeometryGraphInstruction> _instructions = new();
        private CGeometryCombiner _currentCombiner;

        private int currentCombinerDepth;
        private List<float> _propertyValueBuffer;
        private List<GeometryInstruction> _geometryInstructionBuffer;
        private List<MathInstruction> _mathInstructionsBuffer;

        public List<float> PropertyValueBuffer => _propertyValueBuffer;
        public List<MathInstruction> MathInstructionBuffer => _mathInstructionsBuffer;

        public List<GeometryInstruction> GeometryInstructionBuffer => _geometryInstructionBuffer;

        public void BeginWriteCombiner(CGeometryCombiner combiner)
        {
            _currentCombiner = combiner;
            currentCombinerDepth++;
        }

        public void FinishWritingCombiner(CombinerOperation operation, GeometryGraphProperty property)
        {
            throw new NotImplementedException();
            // RegisterProperty(property);
            currentCombinerDepth--;
            _instructions.Add(new CombinerInstruction(operation, property, currentCombinerDepth));
        }

        public void WriteShape(ShapeType shapeType, GeometryGraphProperty positionProperty, List<GeometryGraphProperty> getProperties)
        {
            _instructions.Add(new ShapeInstruction(shapeType, positionProperty, getProperties, currentCombinerDepth, _currentCombiner));
            //RegisterProperty(positionProperty);
            // getProperties.ForEach(property => RegisterProperty(property));

            _currentCombiner = default;
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
    }
}
using System;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.TerrainChunkSystem;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    /// <summary>
    ///     Calculates the TerrainData for a NativeArray world space positions
    /// </summary>
    public struct TerrainInstructionIterator
    {
        #region Public Fields

        public NativeArray<PackedTerrainData> _terrainDataBuffer;

        #endregion

        #region Private Fields

        private readonly NativeArray<GeometryInstruction>.ReadOnly _combinerInstructions;
        private readonly NativeArray<PackedFloat3>.ReadOnly _postionsWS;
        private readonly int _combinerStackSize;
        private readonly int _indexInsideChunk;
        private NativeArray<PackedFloat3> _postionStack;
        private NativeArray<bool> _hasWrittenToCurrentCombiner;
        private int _lastCombinerDepth;

        /// <summary>
        ///     The TerrainChunkData that was inside of this chunk in the previous frame
        /// </summary>
        private TerrainChunkData _originalTerrainData;

        private NativeArray<float> _valueBuffer;

        #endregion

        #region Constructors

        public TerrainInstructionIterator(NativeArray<PackedFloat3> positions, DynamicBuffer<GeometryInstruction> combinerInstructions, int indexInsideChunk, TerrainChunkData existingData,
            NativeArray<float> valueBuffer)
        {
            _valueBuffer = valueBuffer;
            _combinerInstructions = combinerInstructions.AsNativeArray().AsReadOnly();
            //todo cache this between pre-pass and actual pass
            _combinerStackSize = 0;
            for (var i = 0; i < combinerInstructions.Length; i++)
            {
                _combinerStackSize = max(_combinerInstructions[i].CombinerDepth, _combinerStackSize);
            }

            //todo workaround. Remove this and see the exceptions
            _combinerStackSize++;

            _postionsWS = positions.AsReadOnly();

            _terrainDataBuffer = new NativeArray<PackedTerrainData>(_combinerStackSize * _postionsWS.Length, Allocator.Temp);
            _postionStack = new NativeArray<PackedFloat3>(_postionsWS.Length * _combinerStackSize, Allocator.Temp);

            _indexInsideChunk = indexInsideChunk;
            _hasWrittenToCurrentCombiner = new NativeArray<bool>(_combinerStackSize, Allocator.Temp);
            _lastCombinerDepth = -1;

            _originalTerrainData = existingData;
        }

        #endregion

        #region Public methods

        public void CalculateTerrainData()
        {
            for (var i = 0; i < _combinerInstructions.Length; i++)
            {
                ProcessTerrainData(i);
            }

            //In case we did not write any data to the terrain, we fill it with a dummy value
            if (_hasWrittenToCurrentCombiner[0] == false)
            {
                for (var i = 0; i < _postionsWS.Length; i++)
                {
                    _terrainDataBuffer[i] = new PackedTerrainData(10, new PackedTerrainMaterial(TerrainMaterial.GetDefaultMaterial()));
                }
            }
        }

        public void Dispose()
        {
            _terrainDataBuffer.Dispose();
            _postionStack.Dispose();
            _hasWrittenToCurrentCombiner.Dispose();
        }

        #endregion

        #region Private methods

        private void ProcessTerrainData(int instructionIndex)
        {
            GeometryInstruction combinerInstruction = _combinerInstructions[instructionIndex];


            if (combinerInstruction.CombinerDepth > _lastCombinerDepth)
            {
                for (var combinerDepthToInitialize = max(0, _lastCombinerDepth + 1); combinerDepthToInitialize <= combinerInstruction.CombinerDepth; combinerDepthToInitialize++)
                {
                    _hasWrittenToCurrentCombiner[combinerDepthToInitialize] = false;

                    var positionsLength = _postionsWS.Length;

                    if (combinerDepthToInitialize == 0)
                    {
                        NativeArray<PackedFloat3>.Copy(_postionsWS, 0, _postionStack, positionsLength * combinerDepthToInitialize, positionsLength);
                    }
                    else
                    {
                        //copy all positions from the previous level in the stack to the new one
                        NativeArray<PackedFloat3>.Copy(_postionStack, positionsLength * (combinerDepthToInitialize - 1), _postionStack, positionsLength * combinerDepthToInitialize, positionsLength);
                    }
                }
            }

            if (combinerInstruction.GeometryInstructionType == GeometryInstructionType.Transformation)
            {
                for (var i = 0; i < _postionsWS.Length; i++)
                {
                    var position = _postionStack[_postionsWS.Length * combinerInstruction.CombinerDepth + i];

                    _postionStack[_postionsWS.Length * combinerInstruction.CombinerDepth + i] = combinerInstruction.GetTerrainTransformation().TransformPosition(position, _valueBuffer);
                }

                _lastCombinerDepth = combinerInstruction.CombinerDepth;
                return;
            }

            if (_hasWrittenToCurrentCombiner[combinerInstruction.CombinerDepth] == false)
            {
                combinerInstruction.Combiner.Operation = CombinerOperation.Replace;

                _hasWrittenToCurrentCombiner[combinerInstruction.CombinerDepth] = true;
            }

            var stackBaseOffset = _postionsWS.Length * combinerInstruction.CombinerDepth;

            for (var i = 0; i < _postionsWS.Length; i++)
            {
                PackedTerrainData terrainData = default;
                switch (combinerInstruction.GeometryInstructionType)
                {
                    case GeometryInstructionType.CopyOriginal:
                        terrainData = _originalTerrainData[i];
                        break;
                    case GeometryInstructionType.Shape:
                        var shape = combinerInstruction.GetShapeInstruction();

                        float4x4 transformation = combinerInstruction.TransformationValue.Resolve(_valueBuffer);

                        PackedFloat3 positionOS=default;


                        var positionWSValue = _postionStack[_postionsWS.Length * combinerInstruction.CombinerDepth + i];

                        //todo add SIMD version
                        for (int j = 0; j < 4; j++)
                        {
                            float4 positionWSSlice = new float4(positionWSValue.x.PackedValues[j], positionWSValue.y.PackedValues[j], positionWSValue.z.PackedValues[j], 1);
                            float4 positionOSSlice = mul(transformation, positionWSSlice);
                            positionOS.x.PackedValues[j] = positionOSSlice.x;
                            positionOS.y.PackedValues[j] = positionOSSlice.y;
                            positionOS.z.PackedValues[j] = positionOSSlice.z;
                        }
                        
                        //var positionOS = shape.Translation.TransformPosition(_postionStack[_postionsWS.Length * combinerInstruction.CombinerDepth + i],_valueBuffer);

                        var surfaceDistance = shape.GetSurfaceDistance(positionOS, _valueBuffer);
                        terrainData = new PackedTerrainData(surfaceDistance, default);
                        break;
                    case GeometryInstructionType.Combiner:
                        terrainData = _terrainDataBuffer[(combinerInstruction.CombinerDepth+1) * _postionsWS.Length + i];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var existingData = _terrainDataBuffer[stackBaseOffset + i];
                var combinedResult = TerrainChunkOperations.CombinePackedTerrainData(combinerInstruction.Combiner, terrainData, existingData, _valueBuffer);
                _terrainDataBuffer[stackBaseOffset + i] = combinedResult;
            }

            _lastCombinerDepth = combinerInstruction.CombinerDepth;
        }

        #endregion
    }
}
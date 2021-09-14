// using System;
// using Code.CubeMarching.GeometryComponents;
// using Code.CubeMarching.Rendering;
// using Code.CubeMarching.Utils;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using CGenericTerrainModifier = Code.CubeMarching.Authoring.CGenericTerrainModifier;
//
// namespace Code.CubeMarching.TerrainChunkEntitySystem
// {
//     /// <summary>
//     ///     Takes the CMainTerrainCombiner and writes the dependencies of all it's children to all TerrainChunks
//     /// </summary>
//     internal unsafe struct TerrainInstructionWriter
//     {
//         #region Static Stuff
//
//         private static void GetPositionAndSizeInTerrainChunkGrid(TerrainBounds boundsWS, CClusterPosition clusterPosition, out int3 boundsMinGS, out int3 boundsSizeGS, out int boundsVolumeGS)
//         {
//             boundsMinGS = (int3) math.floor(boundsWS.min / SSpawnTerrainChunks.TerrainChunkLength);
//             boundsSizeGS = (int3) math.ceil(boundsWS.size / SSpawnTerrainChunks.TerrainChunkLength);
//             boundsVolumeGS = boundsSizeGS.x * boundsSizeGS.y * boundsSizeGS.z;
//             boundsMinGS -= clusterPosition.PositionGS;
//         }
//
//         #endregion
//
//         #region Private Fields
//
//         private Entity MainCombinerEntity;
//         private ComponentDataFromEntity<CGeometryCombiner> getTerrainCombinerSettings;
//         private ComponentDataFromEntity<CTerrainModifierBounds> getTerrainModifierBounds;
//         private BufferFromEntity<CTerrainChunkCombinerChild> getCombinerChildren;
//         private ComponentDataFromEntity<CGenericTerrainTransformation> getGenericTerrainTransformation;
//         private ComponentDataFromEntity<CGeometryTransformation> getTerrainTransformation;
//         private int maxCombinerDepth;
//         private ComponentDataFromEntity<CGenericTerrainModifier> getTerrainModifier;
//         private ComponentDataFromEntity<CTerrainMaterial> getTerrainMaterial;
//         private ComponentDataFromEntity<WorldToLocal> getWorldToLocal;
//
//         #endregion
//
//         #region Constructors
//
//         public TerrainInstructionWriter(SystemBase systemBase, Entity mainCombiner)
//         {
//             MainCombinerEntity = mainCombiner;
//             getTerrainCombinerSettings = systemBase.GetComponentDataFromEntity<CGeometryCombiner>();
//             getTerrainModifierBounds = systemBase.GetComponentDataFromEntity<CTerrainModifierBounds>();
//             getGenericTerrainTransformation = systemBase.GetComponentDataFromEntity<CGenericTerrainTransformation>();
//             getTerrainTransformation = systemBase.GetComponentDataFromEntity<CGeometryTransformation>();
//             getTerrainModifier = systemBase.GetComponentDataFromEntity<CGenericTerrainModifier>();
//             getTerrainMaterial = systemBase.GetComponentDataFromEntity<CTerrainMaterial>();
//             getWorldToLocal = systemBase.GetComponentDataFromEntity<WorldToLocal>();
//
//             getCombinerChildren = systemBase.GetBufferFromEntity<CTerrainChunkCombinerChild>();
//
//             maxCombinerDepth = 0;
//         }
//
//         #endregion
//
//         #region Public methods
//
//         public void Execute(DynamicBuffer<GeometryInstruction> terrainInstructions, ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition,bool includeLastFrameResult)
//         {
//             terrainInstructions.Clear();
//             if (includeLastFrameResult)
//             {
//                 terrainInstructions.Add(new GeometryInstruction {TerrainInstructionType = TerrainInstructionType.CopyOriginal, CoverageMask = BitArray512.AllBitsTrue});
//             }
//
//             var combinerDepth = 0;
//             WriteChildInstructions(clusterPosition, MainCombinerEntity, combinerDepth, out var terrainBounds, 8, out var extraAreaInsideCombiner,
//                 out var coverageHandler);
//             coverageHandler->ComputeMasksAndWriteToParents(this);
//             coverageHandler->WriteInstructionsToChunk(terrainInstructions);
//
//             clusterParameters.WriteMask = coverageHandler->TotalWriteMask;
//
//             coverageHandler->Dispose(this);
//
//             UnsafeUtility.Free(coverageHandler, Allocator.Temp);
//
//             //calculate hashes for all written instructions
//             for (int i = 0; i < terrainInstructions.Length; i++)
//             {
//                 var terrainInstruction = terrainInstructions[i];
//                 terrainInstruction.Hash = terrainInstruction.CalculateHash();
//                 terrainInstructions[i] = terrainInstruction;
//             }
//         }
//
//         #endregion
//
//         #region Private methods
//
//         private void WriteTerrainCombiner(CGeometryCombiner combiner, int combinerDepth, TerrainGridCoverageHandler* ownCoverageHandler,
//             TerrainGridCoverageHandler* parentCoverageHandler, int childIndex)
//         {
//             var instruction = new GeometryInstruction
//             {
//                 DependencyIndex = combinerDepth + 1,
//                 CombinerDepth = combinerDepth,
//                 Combiner = combiner,
//                 TerrainInstructionType = TerrainInstructionType.Combiner
//             };
//
//             parentCoverageHandler->RegisterChild(this, childIndex, instruction, true, false, ownCoverageHandler);
//             parentCoverageHandler->SetWriteMask(ownCoverageHandler->TotalWriteMask, childIndex);
//         }
//
//         private void WriteShapeInstruction(CClusterPosition clusterPosition, Entity entity, out TerrainBounds elementTerrainBounds, int combinerDepth, CGeometryCombiner combiner,
//             float boundsExtraArea,
//             TerrainGridCoverageHandler* coverageHandler, int childIndex)
//         {
//             var terrainModifierBounds = getTerrainModifierBounds[entity];
//             elementTerrainBounds = terrainModifierBounds.Bounds;
//             var indexInShapeMap = terrainModifierBounds.IndexInShapeMap;
//
//             var elementTerrainBounds1 = elementTerrainBounds;
//             elementTerrainBounds1.min -= boundsExtraArea;
//             elementTerrainBounds1.max += boundsExtraArea;
//
//             var wholeTerrainBounds = new TerrainBounds {min = clusterPosition.PositionGS * 8, max = (clusterPosition.PositionGS + 8) * 8};
//             elementTerrainBounds1.LimitBy(wholeTerrainBounds);
//
//             GetPositionAndSizeInTerrainChunkGrid(elementTerrainBounds1, clusterPosition, out var boundsMinPositionGS, out var boundsSizeGS, out var boundsGSVolume);
//
//             boundsMinPositionGS = math.clamp(boundsMinPositionGS, 0, 8);
//             boundsSizeGS = math.clamp(boundsSizeGS, 0, 8 - boundsMinPositionGS);
//
//             var instructionToRegister = new GeometryInstruction
//             {
//                 DependencyIndex = indexInShapeMap,
//                 CombinerDepth = combinerDepth,
//                 Combiner = combiner,
//                 TerrainInstructionType = false ? TerrainInstructionType.Combiner : TerrainInstructionType.Shape,
//                 TerrainShape = new GeometryShapeTranslationTuple
//                     {Translation = getTerrainTransformation[entity], TerrainMaterial = getTerrainMaterial[entity], TerrainModifier = getTerrainModifier[entity]},
//                 WorldToLocal = getWorldToLocal[entity]
//             };
//
//             coverageHandler->RegisterChild(this, childIndex, instructionToRegister, false, false, default);
//             coverageHandler->SetWriteMask(BitArray512.BoundsToBitArray(boundsMinPositionGS, boundsSizeGS), childIndex);
//         }
//
//         private void WriteTransformationInstruction(int combinerDepth,
//             TerrainGridCoverageHandler* coverageHandler, int childIndex, int indexInTransformationMap, CGenericTerrainTransformation transfomation, bool injectAsFirstChild)
//         {
//             var instructionToRegister = new GeometryInstruction
//             {
//                 DependencyIndex = indexInTransformationMap,
//                 CombinerDepth = combinerDepth,
//                 Combiner = default,
//                 TerrainInstructionType = TerrainInstructionType.Transformation,
//                 TerrainTransformation = transfomation
//             };
//
//
//             //used by GeometryProxy to inject a Transformation to be applied before the children are written to the cluster
//             if (injectAsFirstChild)
//             {
//                 coverageHandler->RegisterFirstChild(this, childIndex, instructionToRegister, false, true, default);
//             }
//             else
//             {
//                 coverageHandler->RegisterChild(this, childIndex, instructionToRegister, false, true, default);
//                 coverageHandler->SetWriteMask(default, childIndex);
//             }
//         }
//
//         private void WriteChildInstructions(CClusterPosition clusterPosition, Entity entity, int combinerDepth, out TerrainBounds elementTerrainBounds, float boundsExtraArea,
//             out float extraAreaInsideCombiner,
//             out TerrainGridCoverageHandler* coverageHandler)
//         {
//             elementTerrainBounds = default;
//
//             var children = getCombinerChildren[entity];
//             var combinerSettings = getTerrainCombinerSettings[entity];
//             extraAreaInsideCombiner = boundsExtraArea + combinerSettings.BlendFactor;
//
//             coverageHandler = (TerrainGridCoverageHandler*) UnsafeUtility.Malloc(sizeof(TerrainGridCoverageHandler), 4, Allocator.Temp);
//             UnsafeUtility.WriteArrayElement(coverageHandler, 0, new TerrainGridCoverageHandler(children.Length, combinerSettings));
//
//             if (getGenericTerrainTransformation.HasComponent(entity))
//             {
//                 coverageHandler->RegisterFirstChild(this, 0,
//                     new GeometryInstruction
//                     {
//                         CombinerDepth = combinerDepth, Combiner = default, CoverageMask = BitArray512.AllBitsTrue, DependencyIndex = default, TerrainShape = default,
//                         TerrainTransformation = getGenericTerrainTransformation[entity], TerrainInstructionType = TerrainInstructionType.Transformation
//                     }, false, true, coverageHandler);
//             }
//
//             for (var i = 0; i < children.Length; i++)
//             {
//                 var childEntity = children[i].SourceEntity;
//                 WriteChildInstruction(clusterPosition, combinerDepth, elementTerrainBounds, extraAreaInsideCombiner, coverageHandler, childEntity, combinerSettings, i, out var childTerrainBounds);
//
//                 elementTerrainBounds.ExpandTo(childTerrainBounds);
//             }
//         }
//
//         private void WriteChildInstruction(CClusterPosition clusterPosition, int combinerDepth, TerrainBounds elementTerrainBounds, float extraAreaInsideCombiner,
//             TerrainGridCoverageHandler* coverageHandler, Entity childEntity, CGeometryCombiner combiner, int i, out TerrainBounds childTerrainBounds)
//         {
//             if (combinerDepth > 10)
//             {
//                 throw new Exception("lol");
//             }
//
//             if (getCombinerChildren.HasComponent(childEntity))
//             {
//                 WriteChildInstructions(clusterPosition, childEntity, combinerDepth + 1, out childTerrainBounds, extraAreaInsideCombiner, out var extraAreaInsideCombiner1,
//                     out var childCoverageHandler);
//                 childCoverageHandler->ComputeMasksAndWriteToParents(this);
//                 WriteTerrainCombiner(combiner, combinerDepth, childCoverageHandler, coverageHandler, i);
//             }
//             else if (getGenericTerrainTransformation.HasComponent(childEntity))
//             {
//                 WriteTransformationInstruction(combinerDepth, coverageHandler, i, 0, getGenericTerrainTransformation[childEntity], false);
//                 //todo workaround, clean this up
//                 childTerrainBounds = elementTerrainBounds;
//             }
//
//             else
//             {
//                 WriteShapeInstruction(clusterPosition, childEntity, out childTerrainBounds, combinerDepth, combiner, extraAreaInsideCombiner, coverageHandler, i);
//             }
//         }
//
//         #endregion
//
//         #region Nested type: TerrainGridCoverageHandler
//
//         private struct TerrainGridCoverageHandler
//         {
//             #region Enums
//
//             private enum WriteCondition
//             {
//                 AnyMustWrite,
//                 FirstMustWrite,
//                 AllMustWrite
//             }
//
//             #endregion
//
//             #region Public Fields
//
//             public readonly int ChildCount;
//             public BitArray512 TotalWriteMask;
//
//             #endregion
//
//             #region Private Fields
//
//             private readonly WriteCondition _writeCondition;
//             private Child* _children;
//
//             //used by GeometryProxy to inject a Transformation to be applied before the children are written to the cluster
//             private Child _firstChild;
//
//             #endregion
//
//             #region Constructors
//
//             public TerrainGridCoverageHandler(int childCount, CGeometryCombiner combiner)
//             {
//                 ChildCount = childCount;
//                 TotalWriteMask = new BitArray512();
//
//                 switch (combiner.Operation)
//                 {
//                     case CombinerOperation.Min:
//                     case CombinerOperation.SmoothMin:
//                         _writeCondition = WriteCondition.AnyMustWrite;
//                         break;
//                     case CombinerOperation.Max:
//                         _writeCondition = WriteCondition.AllMustWrite;
//                         break;
//                     case CombinerOperation.SmoothSubtract:
//                     case CombinerOperation.Add:
//                         _writeCondition = WriteCondition.FirstMustWrite;
//                         break;
//                     case CombinerOperation.ReplaceMaterial:
//                     case CombinerOperation.Replace:
//                     default:
//                         throw new ArgumentOutOfRangeException();
//                 }
//
//                 _children = (Child*) UnsafeUtility.Malloc(childCount * sizeof(Child), 4, Allocator.Temp);
//
//                 _firstChild = default;
//             }
//
//             #endregion
//
//             #region Public methods
//
//             public void SetWriteMask(BitArray512 coverageMask, int childIndex)
//             {
//                 _children[childIndex].CoverageMask = coverageMask;
//             }
//
//             public void RegisterChild(TerrainInstructionWriter job, int childIndex, GeometryInstruction instructionToRegister, bool isCombiner, bool isTransformation,
//                 TerrainGridCoverageHandler* childCombiner)
//             {
//                 _children[childIndex] = new Child {CoverageMask = default, InstructionReference = new TerrainInstructionReference(instructionToRegister, isCombiner, isTransformation, childCombiner)};
//             }
//
//             //used by GeometryProxy to inject a Transformation to be applied before the children are written to the cluster
//             public void RegisterFirstChild(TerrainInstructionWriter job, int childIndex, GeometryInstruction instructionToRegister, bool isCombiner, bool isTransformation,
//                 TerrainGridCoverageHandler* childCombiner)
//             {
//                 _firstChild = new Child {CoverageMask = default, InstructionReference = new TerrainInstructionReference(instructionToRegister, isCombiner, isTransformation, childCombiner)};
//             }
//
//             public void ComputeMasksAndWriteToParents(TerrainInstructionWriter job)
//             {
//                 //todo replace with operator overloads for BitArray512
//
//                 //calculate write to mask
//                 switch (_writeCondition)
//                 {
//                     case WriteCondition.AnyMustWrite:
//                         for (var childIndex = 0; childIndex < ChildCount; childIndex++)
//                         {
//                             if (!_children[childIndex].InstructionReference.IsTransformation)
//                             {
//                                 TotalWriteMask |= _children[childIndex].CoverageMask;
//                             }
//                         }
//
//                         break;
//                     case WriteCondition.FirstMustWrite:
//                         for (var childIndex = 0; childIndex < ChildCount; childIndex++)
//                         {
//                             if (!_children[childIndex].InstructionReference.IsTransformation)
//                             {
//                                 TotalWriteMask = _children[childIndex].CoverageMask;
//                                 break;
//                             }
//                         }
//
//                         break;
//                     case WriteCondition.AllMustWrite:
//                         TotalWriteMask.Fill(true);
//                         for (var childIndex = 0; childIndex < ChildCount; childIndex++)
//                         {
//                             if (!_children[childIndex].InstructionReference.IsTransformation)
//                             {
//                                 TotalWriteMask &= _children[childIndex].CoverageMask;
//                             }
//                         }
//
//                         break;
//                     default:
//                         throw new ArgumentOutOfRangeException();
//                 }
//             }
//
//             public void WriteInstructionsToChunk(DynamicBuffer<GeometryInstruction> clusterBuffer)
//             {
//                 if (_firstChild.InstructionReference.GeometryInstruction.TerrainInstructionType != TerrainInstructionType.None)
//                 {
//                     if (_firstChild.InstructionReference.GeometryInstruction.TerrainInstructionType != TerrainInstructionType.Transformation)
//                     {
//                         throw new Exception("_firstChild is only used as a workaround to inject a transformation when using geometry proxy. it can only be used for transformations");
//                     }
//
//                     clusterBuffer.Add(_firstChild.InstructionReference.GeometryInstruction);
//                 }
//
//                 for (var childIndex = 0; childIndex < ChildCount; childIndex++)
//                 {
//                     ref var childWriteMask = ref _children[childIndex].CoverageMask;
//                     ref var registeredChild = ref _children[childIndex].InstructionReference;
//
//                     if (registeredChild.IsTerrainCombiner)
//                     {
//                         registeredChild.ChildCombinerCoverageHandler->ApplyGlobalWriteMaskOfParent(this);
//                         registeredChild.ChildCombinerCoverageHandler->WriteInstructionsToChunk(clusterBuffer);
//                     }
//
//                     var instructionToWrite = registeredChild.GeometryInstruction;
//                     instructionToWrite.CoverageMask = childWriteMask & TotalWriteMask;
//                     clusterBuffer.Add(instructionToWrite);
//                 }
//             }
//
//             public void Dispose(TerrainInstructionWriter job)
//             {
//                 for (var i = 0; i < ChildCount; i++)
//                 {
//                     var child = _children[i];
//                     if (child.InstructionReference.IsTerrainCombiner)
//                     {
//                         child.InstructionReference.ChildCombinerCoverageHandler->Dispose(job);
//                         UnsafeUtility.Free(child.InstructionReference.ChildCombinerCoverageHandler, Allocator.Temp);
//                     }
//                 }
//
//                 UnsafeUtility.Free(_children, Allocator.Temp);
//             }
//
//             #endregion
//
//             #region Private methods
//
//             private void ApplyGlobalWriteMaskOfParent(TerrainGridCoverageHandler parentCoverageHandler)
//             {
//                 TotalWriteMask &= parentCoverageHandler.TotalWriteMask;
//             }
//
//             #endregion
//
//             #region Nested type: Child
//
//             private struct Child
//             {
//                 #region Public Fields
//
//                 public BitArray512 CoverageMask;
//                 public TerrainInstructionReference InstructionReference;
//
//                 #endregion
//             }
//
//             #endregion
//         }
//
//         #endregion
//
//         private struct TerrainInstructionReference
//         {
//             #region Public Fields
//
//             public readonly TerrainGridCoverageHandler* ChildCombinerCoverageHandler;
//             public readonly bool IsTerrainCombiner;
//             public readonly bool IsTransformation;
//             public readonly GeometryInstruction GeometryInstruction;
//
//             #endregion
//
//             #region Constructors
//
//             public TerrainInstructionReference(GeometryInstruction geometryInstruction, bool isTerrainCombiner, bool isTransformation, TerrainGridCoverageHandler* childCombinerCoverageHandler)
//             {
//                 GeometryInstruction = geometryInstruction;
//                 IsTerrainCombiner = isTerrainCombiner;
//                 ChildCombinerCoverageHandler = childCombinerCoverageHandler;
//                 IsTransformation = isTransformation;
//             }
//
//             #endregion
//         }
//
//         #region Nested type: TerrainInstructionReference
//
//         #endregion
//     }
// }
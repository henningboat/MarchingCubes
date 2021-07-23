using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Code.CubeMarching.Utils
{
    //todo rename to something reasonable
    //todo add safety checks
    public struct ParallelSubListCollection<T> where T : struct
    {
        [NativeDisableParallelForRestriction]public NativeArray<T> _buffer;
        private readonly int _totalCapacity;
        [NativeDisableParallelForRestriction]public NativeArray<int> _subListCounts;
        [NativeDisableParallelForRestriction] public NativeArray<int> _listCounts;
        private readonly int _listCapacity;
        
        private bool _isWritable;
        private int _subListCount;
        private int _subListCapacity;
        private int _listCount;

        public ParallelSubListCollection(int listCount, int subListCount, int subListCapacity)
        {
            _listCount = listCount;
            _subListCapacity = subListCapacity;
            _subListCount = subListCount;
            _totalCapacity = listCount * subListCount * subListCapacity;
            _listCapacity = subListCount * subListCapacity;
            
            _buffer = new NativeArray<T>(_totalCapacity, Allocator.Persistent);
            _subListCounts = new NativeArray<int>(subListCount * listCount, Allocator.Persistent);
            _listCounts = new NativeArray<int>(listCount, Allocator.Persistent);
            _isWritable = false;
            IsCreated = true;
        }

        public bool IsCreated { get; private set; }

        public void Write(int listIndex, int subListIndex, T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (subListIndex >= _subListCount || subListIndex < 0)
            {
                throw new OutOfMemoryException($"subListIndex was {subListIndex}, needs to be within 0-{_subListCount}");
            }
#endif
            var currentCountInSubList = _subListCounts[listIndex * _subListCount + subListIndex];
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (currentCountInSubList >= _subListCapacity)
            {
                throw new OutOfMemoryException($"list:{listIndex} subList:{subListIndex} is out of capacity. current count is {currentCountInSubList}, maximum is {_subListCapacity}, can't add more");
            }
#endif
            
            int indexInBuffer = listIndex * _listCapacity + subListIndex * _subListCapacity + currentCountInSubList;
         
            _buffer[indexInBuffer] = value;
            _subListCounts[listIndex * _subListCount + subListIndex] = currentCountInSubList + 1;
        }

        public JobHandle ScheduleListCollapse(JobHandle dependency)
        {
            _isWritable = false;
            var collapseJob = new CollapseJob<T>() {_collection = this};
            return collapseJob.Schedule(_listCount, 1, dependency);
        }

        public int ReadListLength(int listIndex)
        {
            return _listCounts[listIndex];
        }
 
        public T ReadListValue(int listIndex, int indexInList)
        {
            return _buffer[listIndex*_listCapacity+indexInList];
        }

        [BurstCompile]
        private struct CollapseJob<T> : IJobParallelFor where T : struct
        {
            [NativeDisableParallelForRestriction] public ParallelSubListCollection<T> _collection;

            //todo use memcopy
            public void Execute(int listIndex)
            {
                _collection._isWritable = false;
                int countInList = 0;
                int listIndexInDataBuffer = listIndex * _collection._listCapacity;

                for (int subListIndex = 0; subListIndex < _collection._subListCount; subListIndex++)
                {
                    for (int indexInSubList = 0; indexInSubList < _collection._subListCounts[listIndex * _collection._subListCount + subListIndex]; indexInSubList++)
                    {
                        _collection._buffer[listIndexInDataBuffer + countInList] = _collection._buffer[listIndexInDataBuffer + subListIndex * _collection._subListCapacity + indexInSubList];
                        countInList++;
                    }
                }

                _collection._listCounts[listIndex] = countInList;
            }
        }

        public void Reset()
        {
            //todo replace with memclear
            for (var i = 0; i < _subListCounts.Length; i++)
            {
                _subListCounts[i] = 0;
            }

            for (var i = 0; i < _listCounts.Length; i++)
            {
                _listCounts[i] = 0;
            }

            _isWritable = true;
        }

        public void Dispose()
        {
            _buffer.Dispose();
            _listCounts.Dispose();
            _subListCounts.Dispose();
        }
    }
}
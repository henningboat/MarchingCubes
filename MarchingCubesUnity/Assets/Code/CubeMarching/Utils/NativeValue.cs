using Unity.Collections;
using Unity.Jobs;

namespace Code.CubeMarching.Utils
{
    public struct NativeValue<T> where T : struct
    {
        private NativeArray<T> _nativeArray;

        public NativeValue(Allocator allocator)
        {
            _nativeArray = new NativeArray<T>(1, allocator);
        }

        public T Value
        {
            get => _nativeArray[0];
            set => _nativeArray[0] = value;
        }

        public void Dispose(JobHandle jobDependency)
        {
            _nativeArray.Dispose(jobDependency);
        }

        public void Dispose()
        {
            _nativeArray.Dispose();
        }
    }
}
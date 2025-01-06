using System.Collections.Generic;
using Unity.Collections;

namespace Core.Extensions
{
    public static class NativeExtensions
    {
        public static NativeArray<T> ToNativeArray<T>(this IEnumerable<T> enumerable, int length, Allocator allocator) where T : struct
        {
            var nativeArray = new NativeArray<T>(length, allocator);

            var iterator = 0;
            foreach (var element in enumerable)
            {
                nativeArray[iterator] = element;
            }

            return nativeArray;
        }
    }
}
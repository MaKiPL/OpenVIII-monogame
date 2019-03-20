using System.IO;
using System.Runtime.InteropServices;

namespace FF8
{
    class MemoryReader : BinaryReader
    {
        public long Length => BaseStream.Length;
        public long Position => BaseStream.Position;

        public MemoryReader(byte[] buffer) : base(new MemoryStream(buffer)) { }

        public void Seek(long offset, SeekOrigin origin)
        {
            BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Allows C/C++ classes and structs to be read directly from a stream <see cref="https://docs.microsoft.com/en-us/dotnet/framework/interop/marshaling-classes-structures-and-unions"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Read<T>()
        {
            var buffer = ReadBytes(Marshal.SizeOf<T>());
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return result;
        }

        /// <summary>
        /// Allows an array of C/C++ classes and structs to be read directly from a stream <see cref="https://docs.microsoft.com/en-us/dotnet/framework/interop/marshaling-classes-structures-and-unions"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] Read<T>(int count)
        {
            T[] array = new T[count];
            var size = Marshal.SizeOf<T>();

            for (var i = 0; i < count; i++)
            {
                var buffer = ReadBytes(size);
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                array[i] = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                handle.Free();
            }
            return array;
        }
    }
}

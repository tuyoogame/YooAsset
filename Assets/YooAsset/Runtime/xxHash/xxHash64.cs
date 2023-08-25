// ReSharper disable InconsistentNaming

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Standart.Hash.xxHash
{
    public static partial class xxHash64
    {
        /// <summary>
        /// Compute xxHash for the data byte array
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe ulong ComputeHash(byte[] data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);

            fixed (byte* pData = &data[0])
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }

        /// <summary>
        /// Compute xxHash for the data byte array
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe ulong ComputeHash(byte[] data, int offset, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(offset < data.Length);
            Debug.Assert(length <= data.Length - offset);

            fixed (byte* pData = &data[0 + offset])
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }

        /// <summary>
        /// Compute xxHash for the data byte array
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe ulong ComputeHash(System.ArraySegment<byte> data, ulong seed = 0)
        {
            Debug.Assert(data != null);

            return ComputeHash(data.Array, data.Offset, data.Count, seed);
        }

        /// <summary>
        /// Compute xxHash for the async stream
        /// </summary>
        /// <param name="stream">The stream of data</param>
        /// <param name="bufferSize">The buffer size</param>
        /// <param name="seed">The seed number</param>
        /// <returns>The hash</returns>
        public static async ValueTask<ulong> ComputeHashAsync(Stream stream, int bufferSize = 8192, ulong seed = 0)
        {
            return await ComputeHashAsync(stream, bufferSize, seed, CancellationToken.None);
        }

        /// <summary>
        /// Compute xxHash for the async stream
        /// </summary>
        /// <param name="stream">The stream of data</param>
        /// <param name="bufferSize">The buffer size</param>
        /// <param name="seed">The seed number</param>
        /// <param name="cancellationToken">The cancelation token</param>
        /// <returns>The hash</returns>
        public static async ValueTask<ulong> ComputeHashAsync(Stream stream, int bufferSize, ulong seed,
            CancellationToken cancellationToken)
        {
            Debug.Assert(stream != null);
            Debug.Assert(bufferSize > 32);

            // Optimizing memory allocation
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize + 32);

            int readBytes;
            int offset = 0;
            long length = 0;

            // Prepare the seed vector
            ulong v1 = seed + XXH_PRIME64_1 + XXH_PRIME64_2;
            ulong v2 = seed + XXH_PRIME64_2;
            ulong v3 = seed + 0;
            ulong v4 = seed - XXH_PRIME64_1;

            try
            {
                // Read flow of bytes
                while ((readBytes =
                           await stream.ReadAsync(buffer, offset, bufferSize, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    length = length + readBytes;
                    offset = offset + readBytes;

                    if (offset < 32) continue;

                    int r = offset % 32; // remain
                    int l = offset - r; // length

                    // Process the next chunk 
                    __inline__XXH64_stream_process(buffer, l, ref v1, ref v2, ref v3, ref v4);

                    // Put remaining bytes to buffer
                    Utils.BlockCopy(buffer, l, buffer, 0, r);
                    offset = r;
                }

                // Process the final chunk
                ulong h64 = __inline__XXH64_stream_finalize(buffer, offset, ref v1, ref v2, ref v3, ref v4, length, seed);

                return h64;
            }
            finally
            {
                // Free memory
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Compute xxHash for the data byte span
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe ulong ComputeHash(Span<byte> data, int length, ulong seed = 0)
        {
           //Debug.Assert(data != null);
           //Debug.Assert(length >= 0);
           //Debug.Assert(length <= data.Length);

            fixed (byte* pData = &MemoryMarshal.GetReference(data))
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }

        /// <summary>
        /// Compute xxHash for the data byte span
        /// </summary>
        /// <param name="data">The source of data</param>
        /// <param name="length">The length of the data for hashing</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe ulong ComputeHash(ReadOnlySpan<byte> data, int length, ulong seed = 0)
        {
            Debug.Assert(data != null);
            Debug.Assert(length >= 0);
            Debug.Assert(length <= data.Length);

            fixed (byte* pData = &MemoryMarshal.GetReference(data))
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }

        /// <summary>
        /// Compute xxHash for the stream
        /// </summary>
        /// <param name="stream">The stream of data</param>
        /// <param name="bufferSize">The buffer size</param>
        /// <param name="seed">The seed number</param>
        /// <returns>The hash</returns>
        public static ulong ComputeHash(Stream stream, int bufferSize = 8192, ulong seed = 0)
        {
            Debug.Assert(stream != null);
            Debug.Assert(bufferSize > 32);

            // Optimizing memory allocation
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize + 32);

            int readBytes;
            int offset = 0;
            long length = 0;

            // Prepare the seed vector
            ulong v1 = seed + XXH_PRIME64_1 + XXH_PRIME64_2;
            ulong v2 = seed + XXH_PRIME64_2;
            ulong v3 = seed + 0;
            ulong v4 = seed - XXH_PRIME64_1;

            try
            {
                // Read flow of bytes
                while ((readBytes = stream.Read(buffer, offset, bufferSize)) > 0)
                {
                    length = length + readBytes;
                    offset = offset + readBytes;

                    if (offset < 32) continue;

                    int r = offset % 32; // remain
                    int l = offset - r; // length

                    // Process the next chunk 
                    __inline__XXH64_stream_process(buffer, l, ref v1, ref v2, ref v3, ref v4);

                    // Put remaining bytes to buffer
                    Utils.BlockCopy(buffer, l, buffer, 0, r);
                    offset = r;
                }

                // Process the final chunk
                ulong h64 = __inline__XXH64_stream_finalize(buffer, offset, ref v1, ref v2, ref v3, ref v4, length, seed);

                return h64;
            }
            finally
            {
                // Free memory
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        
        /// <summary>
        /// Compute xxHash for the string 
        /// </summary>
        /// <param name="str">The source of data</param>
        /// <param name="seed">The seed number</param>
        /// <returns>hash</returns>
        public static unsafe ulong ComputeHash(string str, uint seed = 0)
        {
            Debug.Assert(str != null);

            fixed (char* c = str)
            {
                byte* ptr = (byte*) c;
                int length = str.Length * 2;

                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong UnsafeComputeHash(byte* ptr, int length, ulong seed)
        {
            // Use inlined version
            // return XXH64(ptr, length, seed);
            
            return __inline__XXH64(ptr, length, seed);
        }
    }    
}


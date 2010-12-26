using System;

namespace Scopus.Auxiliary
{
    internal static class ByteArrayRoutines
    {
        private static readonly uint[] LENGTH_MASKS =
            new uint[]
                {
                    0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF
                };

        private const int FNV_PRIME = 16777619; // FNV prime
        private const int FNV_OFFSET_BASIS = unchecked ((int)2166136261); // FNV offset basis

        internal static unsafe bool AreEqual(byte[] first, int firstOffset, int firstLength, byte[] second, int secondOffset, int secondLength)
        {
            if (firstLength != secondLength)
                return false;

            fixed (byte* secondBufferBytePtr = second)
            fixed (byte* firstBufferBytePtr = first)
            {
                var secondBufferIntPtr = (int*)(secondBufferBytePtr + secondOffset);
                var firstBufferIntPtr = (int*)(firstBufferBytePtr + firstOffset);

                int length = firstLength >> 2;
                int i;
                for (i = 0; i < length; i++)
                {
                    if (firstBufferIntPtr[i] != secondBufferIntPtr[i])
                        return false;
                }

                int skippedBytesNum = (length << 2) ^ firstLength;
                if (skippedBytesNum != 0)
                {
                    uint mask = LENGTH_MASKS[skippedBytesNum];
                    return (firstBufferIntPtr[i] & mask) == (secondBufferIntPtr[i] & mask);
                }

                return true;
            }
        }

        internal static bool AreEqual(ArraySegment<byte> segment1, ArraySegment<byte> segment2)
        {
            return AreEqual(segment1.Array, segment1.Offset, segment1.Count, segment2.Array, segment2.Offset,
                            segment2.Count);
        }

        internal static bool AreEqual(byte[] first, byte[] second)
        {
            return AreEqual(first, 0, first.Length, second, 0, second.Length);
        }

        // http://en.wikipedia.org/wiki/Fowler_Noll_Vo_hash
        internal static int GetArrayHashCode(byte[] array, int offset, int length)
        {
            unchecked
            {
                var hash = FNV_OFFSET_BASIS;

                for (int i = offset; i < offset + length; i++)
                    hash = (hash ^ array[i]) * FNV_PRIME;

                return hash;
            }

        }

        internal static int GetArrayHashCode(byte[] array)
        {
            return GetArrayHashCode(array, 0, array.Length);
        }

        internal static int GetArrayHashCode(ArraySegment<byte> segment)
        {
            return GetArrayHashCode(segment.Array, segment.Offset, segment.Count);
        }
    }
}

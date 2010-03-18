using System;
using System.Text;

namespace Scopus.LexicalAnalysis
{
    public class Token : ICloneable, IEquatable<Token>, IDisposable
    {
        public const int UNDEFINED_CLASS = -1;
        private static readonly uint[] LENGTH_MASKS =
            new uint[]
                {
                    0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF
                };

        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public int Class { get; set; }

        internal Token()
        {
        }
        internal Token(string lexemeStr)
        {
            InitializeTo(lexemeStr);
        }
        internal Token(byte[] buffer, int offset, int length)
        {
            InitializeTo(buffer, offset, length);
        }
        internal Token(ArraySegment<byte> data) : this(data.Array, data.Offset, data.Count)
        {
        }

        private void InitializeTo(string lexemeStr)
        {
            Buffer = Encoding.ASCII.GetBytes(lexemeStr);
            Offset = 0;
            Length = lexemeStr.Length;
            Class = UNDEFINED_CLASS;
        }
        private void InitializeTo(byte[] buffer, int offset, int length)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
            Class = UNDEFINED_CLASS;
        }

        public object Clone()
        {
            // Note that all data fields are value type except Buffer.
            // We don't really allocate new buffer when cloning, just point to the same area.
            return MemberwiseClone();
        }

        // http://en.wikipedia.org/wiki/Fowler_Noll_Vo_hash
        public override int GetHashCode()
        {
            const int P = 16777619; // FNV prime

            unchecked
            {
                int hash = (int)2166136261; // FNV offset basis

                for (int i = Offset; i < Offset + Length; i++)
                    hash = (hash ^ Buffer[i]) * P;

                return hash;
            }
        }
        public unsafe bool Equals(Token other)
        {
            if (this.Length != other.Length)
                return false;
            if (this.Class != other.Class)
                return false;

            fixed (byte* buffOtherB = other.Buffer)
            fixed (byte* buffThisB = this.Buffer)
            {
                int* buffOtherI = (int*)(buffOtherB + other.Offset);
                int* buffThisI = (int*)(buffThisB + Offset);

                int length = Length >> 2;
                int i;
                for (i = 0; i < length; i++)
                {
                    if (buffThisI[i] != buffOtherI[i])
                        return false;
                }

                int skippedBytesNum = (length << 2) ^ Length;
                if (skippedBytesNum != 0)
                {
                    uint mask = LENGTH_MASKS[skippedBytesNum];
                    return (buffThisI[i] & mask) == (buffOtherI[i] & mask);
                }

                return true;
            }
        }
        public override bool Equals(Object obj)
        {
            if (obj == null) return base.Equals(obj);

            if (obj is Token)
                return Equals(obj as Token);

            return false;
        }
        public override string ToString()
        {
            return AsString();
        }

        public string AsString()
        {
            return PrimitivesParser.ParseString(this);
        }
        public int AsInt()
        {
            return PrimitivesParser.ParseInt(this);
        }
		public float AsFloat()
		{
			return PrimitivesParser.ParseFloat(this);
		}

        #region IDisposable Members

        ~Token()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
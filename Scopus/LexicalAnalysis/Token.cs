using System;
using System.Text;
using Scopus.Auxiliary;

namespace Scopus.LexicalAnalysis
{
    public class Token : ICloneable, IEquatable<Token>, IDisposable
    {
        public const int UNDEFINED_CLASS = -1;

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

        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public int Class { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            // Note that all data fields are value type except Buffer.
            // We don't really allocate new buffer when cloning, just point to the same area.
            return MemberwiseClone();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEquatable<Token> Members

        public bool Equals(Token other)
        {
            if (Length != other.Length)
                return false;
            if (Class != other.Class)
                return false;

            return ByteArrayRoutines.AreEqual(Buffer, Offset, Length, other.Buffer, other.Offset, other.Length);
        }

        #endregion

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

        public override int GetHashCode()
        {
            return ByteArrayRoutines.GetArrayHashCode(Buffer, Offset, Length);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null) return base.Equals(obj);

            var token = obj as Token;
            if (token != null)
                return Equals(token);

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
    }
}
using System;

namespace Scopus.LexicalAnalysis
{
    //TODO: replace char? with byte?, since unicode will be supported by double transition 
    //TODO: I.e. transition on first byte and second byte. 
    /// <summary>
    /// Represents single input char or epsilon
    /// </summary>
    internal class InputChar : IEquatable<InputChar> 
    {
        private readonly char? value;

        private InputChar(char? value)
        {
            this.value = value;
        }

        /// <summary>
        /// Creates InputChar structure for given char
        /// </summary>
        /// <param name="value">Input symbol</param>
        /// <returns></returns>
        internal static InputChar For(char value)
        {
            return new InputChar(value);
        }

        /// <summary>
        /// Creates epsilon as input char
        /// </summary>
        /// <returns></returns>
        internal static InputChar Epsilon()
        {
            return new InputChar(null);
        }

        public bool IsEpsilon()
        {
            return (this.value == null);
        }

        // Throws InvalidOperationException in case of value == null
        public byte Value
        {
            get { return (byte) value; }
        }

        public bool Equals(InputChar other)
        {
            return (value == other.value);
        }

        public override int GetHashCode()
        {
            return (value == null)? -1 : (int)value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            if (obj is InputChar)
                return Equals(obj as InputChar);

            return false;
        }
    }
}
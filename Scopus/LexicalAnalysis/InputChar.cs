using System;

namespace Scopus.LexicalAnalysis
{
    //TODO: Rename to InputByte, cause it really byte, not char.
    /// <summary>
    /// Represents single input char or epsilon
    /// </summary>
    [Serializable]
    internal class InputChar : IEquatable<InputChar>
    {
        private readonly byte? value;

        // Factory method pattern - in order to instanciate this class use InputChar.For() or 
        // InputChar.Epsilon() methods.
        private InputChar(byte? value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets value stored for the char. Throws InvalidOperationException in case of value == null
        /// </summary>
        public byte Value
        {
            // ReSharper disable PossibleInvalidOperationException
            get { return (byte) value; }
            // ReSharper restore PossibleInvalidOperationException
        }

        #region IEquatable<InputChar> Members

        public bool Equals(InputChar other)
        {
            return (value == other.value);
        }

        #endregion

        /// <summary>
        /// Creates InputChar structure for given char
        /// </summary>
        /// <param name="value">Input symbol</param>
        /// <returns></returns>
        internal static InputChar For(byte value)
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

        /// <summary>
        /// Checks if value stored represents epsilon (empty word)
        /// </summary>
        /// <returns>true, if epsilon (empty word), otherwise - false</returns>
        public bool IsEpsilon()
        {
            return (value == null);
        }

        public override int GetHashCode()
        {
            return (value == null) ? -1 : (int) value;
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
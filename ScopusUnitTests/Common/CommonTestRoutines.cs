using System;
using System.Text;

namespace ScopusUnitTests.Common
{
    public static class CommonTestRoutines
    {
        /// <summary>
        /// <para>This is obsolete </para>
        /// <para>Use the following instead:</para>
        /// <code>
        ///         private static
        ///        object[] Encodings = { Encoding.ASCII, Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF32, Encoding.UTF8 };
        ///
        ///        [TestCaseSource("Encodings")]
        ///        [Test]
        ///  ... here goes your test ...
        /// </code>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obsolete]
        public static Encoding GetEncoding(string name)
        {
            switch (name.ToLower())
            {
                case "utf-16le":
                    return Encoding.Unicode;
                case "utf-8":
                    return Encoding.UTF8;
                case "utf-7":
                    return Encoding.UTF7;
                case "utf-16be":
                    return Encoding.BigEndianUnicode;
                case "utf-32le":
                    return Encoding.UTF32;
                case "ascii":
                    return Encoding.ASCII;

                default:
                    throw new ArgumentException("name");
            }
        }
    }
}

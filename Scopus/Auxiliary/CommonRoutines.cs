using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Scopus.Auxiliary
{
    public static class CommonRoutines
    {
        /// <summary>
        /// Copies object with all the referenced objects (i.e. all objects will be recreated)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T obj)
        {
            object result = null;

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                result = (T)formatter.Deserialize(ms);
                ms.Close();
            }

            return (T)result;
        }

        /// <summary>
        /// Gets KeyValuePair<T1, T2> from given dictionary at given index
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static KeyValuePair<T1, T2> GetKVP<T1, T2>(IEnumerable<KeyValuePair<T1, T2>> dictionary, int index)
        {
            int currentItem = 0;

            foreach (var kvp in dictionary)
            {
                if (currentItem == index)
                {
                    return kvp;
                }
                currentItem++;
            }

            throw new IndexOutOfRangeException(String.Format("Given index ({0}) is greater than dictionary size ({1})", index, currentItem));
        }
    }
}

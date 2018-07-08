using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager.Geocaching
{
    /// <summary>
    /// Simple ROT13 encoder/decoder
    /// </summary>
    public static class ROT13
    {
        /// <summary>
        /// Encode/Decode in ROT13
        /// </summary>
        /// <param name="value">string to encode/decode</param>
        /// <returns>encoded/decoded string</returns>
        public static string Transform(string value)
        {
            if (value.Length == 0)
                return value;

            char[] array = value.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                int number = (int)array[i];

                if (number >= 'a' && number <= 'z')
                {
                    if (number > 'm')
                    {
                        number -= 13;
                    }
                    else
                    {
                        number += 13;
                    }
                }
                else if (number >= 'A' && number <= 'Z')
                {
                    if (number > 'M')
                    {
                        number -= 13;
                    }
                    else
                    {
                        number += 13;
                    }
                }
                array[i] = (char)number;
            }
            return new string(array);
        }
    }
}

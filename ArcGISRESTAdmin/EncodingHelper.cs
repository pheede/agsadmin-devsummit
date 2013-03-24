// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Text;

namespace ArcGISRESTAdmin
{
    /// <summary>
    /// A set of helper methods for converting to/from various encoding formats used by the ArcGIS for Server Admin REST API.
    /// </summary>
    public static class EncodingHelper
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a hex-encoded string to the corresponding byte array.
        /// </summary>
        /// <param name="hex">Hex-encoded string</param>
        /// <returns>Byte representation of the hex-encoded input</returns>
        public static byte[] HexToBytes(string hex)
        {
            int length = hex.Length;

            if (length % 2 != 0)
            {
                length += 1;
                hex = "0" + hex;
            }

            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Hex-encodes a byte array.
        /// </summary>
        /// <param name="bytes">Byte array to encode</param>
        /// <returns>Hex-encoded string</returns>
        public static string BytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat("{0:x2}", bytes[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return the given DateTime as the count of milliseconds since the Unix epoch (1970-01-01).
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixTimestampMillis(DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            double totalMs = (dateTime - UnixEpoch).TotalMilliseconds;
            return (long)totalMs;
        }

        /// <summary>
        /// Return the current UTC date and time as the count of milliseconds since the Unix epoch (1970-01-01).
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        /// Return a DateTime corresponding to the input Unix timestamp (in milliseconds).
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }
    }
}

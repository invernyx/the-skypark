using System.Collections.Generic;
using System.Text;

namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// Utility methods for reading coded strings from a dword value.
    /// </summary>
    public class CodedString
    {
        /// <summary>
        /// Reads a dword value and converts it to a short string.
        /// </summary>
        /// <param name="value">The dword value.</param>
        /// <param name="fromAirportData">Whether this dword value needs to be shifted.</param>
        /// <returns>The ICAO or region code.</returns>
        public static string FromDword(uint value, bool fromAirportData)
        {
            if (fromAirportData)
                value >>= 5;

            var codedChars = new List<byte>();

            while (value > 37)
            {
                var codedChar = value % 38;
                codedChars.Insert(0, (byte)codedChar);
                value = (value - codedChar) / 38;
                if (value < 38)
                    codedChars.Insert(0, (byte)value);
            }

            var finalString = new StringBuilder();

            foreach (var codedChar in codedChars)
            {
                if (codedChar == 0)
                    finalString.Append(" ");
                else if (codedChar > 0 && codedChar < 12)
                    finalString.Append((char)('0' + (codedChar - 2)));
                else
                    finalString.Append((char)('A' + (codedChar - 12)));
            }

            return finalString.ToString();
        }
    }
}

using System;
using System.Linq;

namespace Orbx.DataManager.Core.Esp
{
    /// <summary>
    /// Represents a single QMID tile in the simulator.
    /// </summary>
    public class Qmid
    {
        /// <summary>
        /// The U value of the QMID tile.
        /// </summary>
        public int U { get; set; }

        /// <summary>
        /// The V value of the QMID tile.
        /// </summary>
        public int V { get; set; }

        /// <summary>
        /// The zoom level of the QMID tile.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Initialzes a new QMID tile with the specified location and zoom.
        /// </summary>
        /// <param name="u">The tile's U value.</param>
        /// <param name="v">The tile's V value.</param>
        /// <param name="level">The tile's zoom level.</param>
        public Qmid(int u, int v, int level)
        {
            U = u;
            V = v;
            Level = level;
        }

        /// <summary>
        /// Represents a QMID tile in an autogen file name encoding.
        /// </summary>
        /// <returns>The autogen name for this QMID tile.</returns>
        public string ToAutogenEncoding()
        {
            if (Level != 15)
                throw new Exception("QMID level must be 15 to perform autogen encoding");

            ulong uBinaryInt = ulong.Parse(Convert.ToString(U, 2));
            ulong vBinaryInt = ulong.Parse(Convert.ToString(V, 2).Replace('1', '2'));

            return (uBinaryInt + vBinaryInt).ToString().PadLeft(15, '0');
        }

        /// <summary>
        /// Creates a new QMID tile from an autogen filename.
        /// </summary>
        /// <param name="autogenName">The name of the file (without extension)</param>
        /// <returns>The completed QMID tile.</returns>
        public static Qmid FromAutogenEncoding(string autogenName)
        {
            string uBitString = autogenName.Replace('3', '1').Replace('2', '0');
            string vBitString = autogenName.Replace('1', '0').Replace('3', '1').Replace('2', '1');

            return new Qmid(BitStringToInt(uBitString), BitStringToInt(vBitString), 15);
        }

        /// <summary>
        /// Creates a new QMID tile from dword values from a BGL file.
        /// </summary>
        /// <returns>The completed QMID tile.</returns>
        public static Qmid FromDwords(uint a, uint b)
        {
            var v = 0;
            var u = 0;
            var cnt = 0x1F;
            var workDwordA = a;
            var workDwordB = b;

            while (cnt > 0 && (workDwordB & 0x80000000) == 0)
            {
                workDwordB <<= 2;
                workDwordB += (workDwordA & 0xC0000000) >> 30;

                workDwordA += workDwordA;
                workDwordA += workDwordA;
                cnt--;
            }

            workDwordB &= 0x7FFFFFFF;
            var level = cnt;

            while (cnt >= 0)
            {
                if ((workDwordB & 0x80000000) != 0)
                    v += (1 << cnt);

                if ((workDwordB & 0x40000000) != 0)
                    u += (1 << cnt);

                workDwordB <<= 2;
                workDwordB += (workDwordA & 0xC0000000) >> 30;
                workDwordA += workDwordA;
                workDwordA += workDwordA;
                cnt--;
            }

            return new Qmid(u, v, level);
        }

        /// <summary>
        /// Converts a bit string to an integer.
        /// </summary>
        /// <param name="bits">The bit string to convert.</param>
        /// <returns>The resulting integer.</returns>
        private static int BitStringToInt(string bits)
        {
            char[] reversedBits = bits.Reverse().ToArray();
            int num = 0;

            for (int power = 0; power < reversedBits.Count(); power++)
            {
                if (reversedBits[power] == '1')
                {
                    int currentNum = (int)Math.Pow(2, power);
                    num += currentNum;
                }
            }

            return num;
        }

        #region Comparison
        public override bool Equals(object obj)
        {
            if (!(obj is Qmid otherQmid))
                return false;

            return U == otherQmid.U
                && V == otherQmid.V
                && Level == otherQmid.Level;
        }

        public override int GetHashCode()
        {
            return U.GetHashCode() + V.GetHashCode() + Level.GetHashCode();
        }

        public static bool operator ==(Qmid q1, Qmid q2)
        {
            return q1.Equals(q2);
        }

        public static bool operator !=(Qmid q1, Qmid q2)
        {
            return !(q1 == q2);
        }
        #endregion

        public override string ToString()
            => $"QMID (u {U}, v {V}, lv {Level})";
    }
}

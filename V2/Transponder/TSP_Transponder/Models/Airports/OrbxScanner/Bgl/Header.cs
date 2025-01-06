using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl
{
    /// <summary>
    /// Represents the header inside the BGL file.
    /// </summary>
    public class Header : IHasData
    {
        /// <summary>
        /// The time the BGL file was created at.
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// The QMID tiles this BGL file covers.
        /// </summary>
        public List<Qmid> QmidTiles { get; private set; }

        /// <summary>
        /// The number of sections in this BGL file.
        /// </summary>
        public int NumberOfSections { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="Header"/>.
        /// </summary>
        public Header()
        {
            QmidTiles = new List<Qmid>();
        }

        /// <summary>
        /// Reads all of the header information from the file.
        /// </summary>
        /// <param name="reader">The reader with an opened BGL file.</param>
        public void GetData(BinaryReader reader)
        {
            // signature
            var magicNumber = reader.ReadUInt32();
            if (magicNumber != 0x19920201)
                throw new System.IO.FileFormatException("BGL files must start with 0x19920201");

            // header size
            var headerSize = reader.ReadUInt32();
            if (headerSize != 0x38)
                throw new System.IO.FileFormatException("BGL file headers must be 0x38 long");

            // filetime
            long fileTime = reader.ReadInt64();
            CreatedAt = DateTimeOffset.FromFileTime(fileTime);

            // second signature
            var magicNumber2 = reader.ReadUInt32();
            if (magicNumber2 != 0x08051803)
                throw new System.IO.FileFormatException("Second BGL signature doesn't match");

            // number of sections
            NumberOfSections = reader.ReadInt32();

            // QMID
            for (int i = 0; i < 8; i++)
            {
                var dwordValue = reader.ReadUInt32();
                if (dwordValue != 0)
                    QmidTiles.Add(Qmid.FromDwords(dwordValue, 0));
            }
        }
    }
}

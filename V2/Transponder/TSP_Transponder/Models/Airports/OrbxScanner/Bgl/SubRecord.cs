using System.IO;

namespace Orbx.DataManager.Core.Bgl
{
    /// <summary>
    /// Represents a subrecord in a BGL file's subsection.
    /// </summary>
    public class SubRecord
    {
        private readonly BinaryReader reader;

        /// <summary>
        /// The starting location of this subrecord.
        /// </summary>
        public long StartOffset { get; set; }

        /// <summary>
        /// The identifier from the file of this subrecord.
        /// </summary>
        public int Identifier { get; set; }

        /// <summary>
        /// The total size of this subrecord.
        /// </summary>
        public uint RecordSize { get; set; }

        /// <summary>
        /// Initializes a new <see cref="SubRecord"/> with a reader, ID and size.
        /// </summary>
        /// <param name="reader">The binary reader for this file.</param>
        /// <param name="identifier">The ID of this subrecord.</param>
        /// <param name="largeSize">Whether the size of this subrecord is expressed as 2 or 4 bytes.</param>
        public SubRecord(BinaryReader reader, int identifier, bool largeSize)
        {
            this.reader = reader;
            Identifier = identifier;
            StartOffset = reader.BaseStream.Position - 2;
            RecordSize = largeSize ? reader.ReadUInt32() : reader.ReadUInt16();
        }

        /// <summary>
        /// Skips this whole subrecord.
        /// </summary>
        public void Skip()
        {
            reader.BaseStream.Seek(StartOffset + RecordSize, SeekOrigin.Begin);
        }
    }
}

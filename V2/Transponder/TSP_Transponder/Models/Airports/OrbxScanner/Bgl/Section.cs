using Orbx.DataManager.Core.Common;
using System.IO;

namespace Orbx.DataManager.Core.Bgl
{
    /// <summary>
    /// Represents a section in the BGL file.
    /// </summary>
    public abstract class Section : IHasData
    {
        /// <summary>
        /// This sections type as seen in the file.
        /// </summary>
        public int SectionType { get; set; }

        /// <summary>
        /// The size of each subsection in this section.
        /// </summary>
        public uint SubSectionSize { get; set; }

        /// <summary>
        /// The number of subsections in this section.
        /// </summary>
        public int SubSectionCount { get; set; }

        /// <summary>
        /// This section will start at this point in the file.
        /// </summary>
        public uint FileOffset { get; set; }

        /// <summary>
        /// The total size of this whole section.
        /// </summary>
        public uint SectionSize { get; set; }

        /// <summary>
        /// Gets the data from this section.
        /// </summary>
        /// <param name="reader">The binary reader which has an opened file.</param>
        public abstract void GetData(BinaryReader reader);
    }
}

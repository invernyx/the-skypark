using Orbx.DataManager.Core.Esp;

namespace Orbx.DataManager.Core.Bgl
{
    /// <summary>
    /// Represents a single subsection in the file.
    /// </summary>
    public class SubSection
    {
        /// <summary>
        /// The QMID cell that this subsection represents.
        /// </summary>
        public Qmid Qmid { get; set; }

        /// <summary>
        /// The number of records in this subsection.
        /// </summary>
        public int NumberOfRecords { get; set; }

        /// <summary>
        /// The offset of the start of this subsection.
        /// </summary>
        public uint FileOffset { get; set; }

        /// <summary>
        /// The total size of this subsection.
        /// </summary>
        public uint SubsectionSize { get; set; }
    }
}

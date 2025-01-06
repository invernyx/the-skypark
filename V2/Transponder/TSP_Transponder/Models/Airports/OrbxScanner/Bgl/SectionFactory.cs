using Orbx.DataManager.Core.Bgl.Sections;
using System;
using System.Collections.Generic;

namespace Orbx.DataManager.Core.Bgl
{
    /// <summary>
    /// A factory for creating sections based on the type of the identifier.
    /// </summary>
    public class SectionFactory
    {
        /// <summary>
        /// A key:value store of section identifiers and their parseres.
        /// </summary>
        private static readonly Dictionary<int, Type> supportedSections = new Dictionary<int, Type>()
        {
            { 0x03, typeof(AirportSection) },
            { 0x27, typeof(NameListSection) }
        };

        /// <summary>
        /// Creates a section based on on the type.
        /// </summary>
        /// <param name="type">The ID of this section.</param>
        /// <param name="size">The size of each subsection.</param>
        /// <param name="count">The number of subsections.</param>
        /// <param name="offset">The offset of the start of this section.</param>
        /// <param name="totalSize">The total size of this section including subsections.</param>
        /// <returns>The newly created section.</returns>
        public static Section CreateSection(int type, uint size, int count, uint offset, uint totalSize)
        {
            if (!supportedSections.TryGetValue(type, out Type concreteSection))
            {
                //Console.WriteLine($"Unsupported section type: {type}");
                throw new Exception($"Unsupported section type: {type}");
            }

            //Console.WriteLine($"Supported section type: {type}");
            var section = (Section)Activator.CreateInstance(concreteSection);
            section.SectionType = type;
            section.SectionSize = size;
            section.SubSectionCount = count;
            section.FileOffset = offset;
            section.SectionSize = totalSize;

            return section;
        }
    }
}

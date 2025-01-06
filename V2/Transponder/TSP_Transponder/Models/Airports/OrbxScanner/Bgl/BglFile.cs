using Orbx.DataManager.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl
{
    /// <summary>
    /// A simple class for reading an ESP BGL file.
    /// </summary>
    public class BglFile : SimulatorFile, IDisposable, IHasData
    {
        /// <summary>
        /// The file's binary reader.
        /// </summary>
        public BinaryReader Reader { get; set; }

        /// <summary>
        /// The header section of the BGL file. Will be set after <see cref="GetData"/>
        /// is called.
        /// </summary>
        public Header Header { get; private set; }

        /// <summary>
        /// A list of sections contained within the BGL file. These sections are not
        /// read until the <see cref="Section.GetData(BinaryReader)"/> method is called
        /// on them.
        /// </summary>
        public List<Section> Sections { get; private set; }


        /// <summary>
        /// Initializes a new <see cref="BglFile"/> with an opened <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The reader that has the BGL file open.</param>
        private BglFile(BinaryReader reader)
        {
            Reader = reader;
            Sections = new List<Section>();
        }

        /// <summary>
        /// Initializes a new <see cref="BglFile"/> and provides a file that will be opened.
        /// </summary>
        /// <param name="path">The location of the BGL file on disk.</param>
        public BglFile(string path)
            : this(new BinaryReader(File.Open(path, FileMode.Open)))
        { }

        /// <summary>
        /// Gets the header and base sections from the BGL file.
        /// </summary>
        public void GetData()
        {
            GetData(Reader);
        }

        /// <summary>
        /// Gets the header and base sections from the BGL file.
        /// </summary>
        /// <param name="reader">The reader which has an opened BGL file.</param>
        public void GetData(BinaryReader reader)
        {
            Header = new Header();
            Header.GetData(reader);

            ReadSectionDefinitions();
        }

        /// <summary>
        /// Reads the section definitions from the top of the BGL file, and places them in the list.
        /// </summary>
        private void ReadSectionDefinitions()
        {
            for (int i = 0; i < Header.NumberOfSections; i++)
            {
                var type = Reader.ReadInt32();
                var size = ((Reader.ReadUInt32() & 0x10000) | 0x40000) >> 0x0E;
                var count = Reader.ReadInt32();
                var offset = Reader.ReadUInt32();
                var totalSize = Reader.ReadUInt32();

                if (size * count != totalSize)
                    throw new System.IO.FileFormatException($"Section total size {totalSize} doesn't match calculated size ({size} * {count}) {size * count}");

                try
                {
                    Sections.Add(SectionFactory.CreateSection(type, size, count, offset, totalSize));
                }
                catch
                {
                //    Console.WriteLine($"Unexpected section with value {type}");
                }
            }
        }

        /// <summary>
        /// Clean up our <see cref="BinaryReader"/>.
        /// </summary>
        public void Dispose()
        {
            Reader.Close();
            Reader.Dispose();
        }
    }
}

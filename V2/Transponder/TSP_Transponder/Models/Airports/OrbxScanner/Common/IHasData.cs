using System.IO;

namespace Orbx.DataManager.Core.Common
{
    /// <summary>
    /// An interface for showing whether the current object has more optional
    /// data that can be obtained.
    /// </summary>
    public interface IHasData
    {
        /// <summary>
        /// Gets the data for the current object from file.
        /// </summary>
        /// <param name="reader">The current file's reader.</param>
        void GetData(BinaryReader reader);
    }
}

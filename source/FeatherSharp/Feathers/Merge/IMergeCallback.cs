using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Feathers.Merge
{
    /// <summary>
    /// Clients that use the <see cref="MergeFeather"/> must provide an implementation
    /// of this interface to receive notifications about the feathering process.
    /// </summary>
    /// <remarks>
    /// The notification methods are not guaranteed to be invoked in any specific order.
    /// </remarks>
    public interface IMergeCallback
    {
        /// <summary>
        /// Notifies the client that a file has been processed.
        /// </summary>
        /// <param name="fileName">The name of the processed file.</param>
        /// <param name="info">Flag that indicates the processing result.</param>
        void NotifyFile(string fileName, MergeFileInfo info);
    }

    /// <summary>
    /// Specifies the file-specific information flags for the
    /// Merge feather.
    /// </summary>
    public enum MergeFileInfo
    {
        /// <summary>
        /// Signals that the file has been merged into the main assembly.
        /// </summary>
        Merged,
    }
}

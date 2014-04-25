using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MP3Tagger.Interfaces
{
    /// <summary>
    /// Interface providing access to data common to all ID3 Tags.
    /// </summary>
    public interface IID3Tag
    {
        /// <summary>
        /// Artist name, truncate before saving when nessessary
        /// </summary>
        string Artist { get; set; }

        /// <summary>
        /// Album name, truncate before saving when nessessary
        /// </summary>
        string Album { get; set; }

        /// <summary>
        /// Track title, truncate before saving when nessessary
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Year as 4-digit year, must be positive
        /// </summary>
        int Year { get; set; }

        /// <summary>
        ///  the track number, cannot be negative
        /// </summary>
        int TrackNumber { get; set; }

        string Comments { get; set; }

        string Genre { get; set; }

        /// <summary>
        /// Writes the Tag to the given FileStream. Implementations must reset
        /// the stream position to the value prior to the writing operation.
        /// </summary>
        ///<param name="stream">a file stream ready to be written into</param>
        int Write(Stream stream);

        /// <summary>
        /// Reads the Tag from the given FileStream. Overwrites the current properties. 
        /// Implementations must reset
        /// the stream position to the value prior to the writing operation.
        /// </summary>
        /// <param name="stream">a file stream ready to be read from</param>
        bool Read(Stream stream,bool strict=false);
    }
}

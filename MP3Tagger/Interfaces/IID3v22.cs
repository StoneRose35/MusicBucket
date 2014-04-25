using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MP3Tagger.Interfaces
{
    public interface IID3v22
    {
        /// <summary>
        /// The front cover image
        /// </summary>
        Image FrontCover { get; set; }

        /// <summary>
        /// True if unsynchronization is used, in short when unsynchronizing a 0x00 byte is added to each 0xFF byte 
        /// </summary>
        bool UnsynchronizationUsed { get; }

        /// <summary>
        /// True if compression is used
        /// </summary>
        bool CompressionUsed { get; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace CDRipperLib
{
    public class WaveWriter
    {
        FileStream _stream;
        int _bytesWritten;
        int _datasizeoffset,_datasize;
        public bool CreateFile(string filename)
        {
            bool res = false;
            byte[] towrite;
            ushort framesize;
            int offset = 0;
            try
            {
                _stream = File.Create(filename);
                towrite=new byte[]{(byte)'R',(byte)'I',(byte)'F',(byte)'F'};
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)0, (byte)0, (byte)0, (byte)0 }; // setting zero-ength initially
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)16, (byte)0, (byte)0, (byte)0 };
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)1, (byte)0 }; // format tag
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)2, (byte)0 }; //channels
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = BitConverter.GetBytes((uint)44100); //samplerate
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;
                framesize = 2 * ((16 + 7) / 8);

                towrite = BitConverter.GetBytes((uint)(framesize * 44100)); // samples/sec
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = BitConverter.GetBytes(framesize);
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)16, (byte)0 }; // bits per sample
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                towrite = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                _datasizeoffset = offset;

                towrite = BitConverter.GetBytes(_bytesWritten); // preliminary data content size
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                _bytesWritten = offset;

            }
            catch
            {
                return false;
            }
            return res;
        }

        public void WriteChunk(byte[] content)
        {
            _datasize += content.Length;
            _stream.Write(content,0,content.Length);
        }
        /// <summary>
        /// Write the entire file content at once
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int WriteContent(byte[] content)
        {
            byte[] towrite;
            int offset = 0;
            _datasize = content.Length;

            try
            {
                _stream.Seek(_datasizeoffset, SeekOrigin.Begin);

                towrite = BitConverter.GetBytes(_datasize);
                _stream.Write(towrite, 0, towrite.Length);
                offset += towrite.Length;

                _stream.Write(content, 0, content.Length);

                return offset;
            }
            catch
            {
                return 0;
            }

        }

        public void CloseFile()
        {
            int filesize;
            byte[] towrite;
            try
            {


                _stream.Seek(4, SeekOrigin.Begin);
                filesize = (int)_stream.Length - 8;
                towrite = BitConverter.GetBytes(filesize);
                _stream.Write(towrite, 0, towrite.Length);

                _stream.Seek(_datasizeoffset, SeekOrigin.Begin);

                towrite = BitConverter.GetBytes((uint)_datasize);
                _stream.Write(towrite, 0, towrite.Length);

                _stream.Close();
            }
            catch
            {
 
            }
        }
    }
}

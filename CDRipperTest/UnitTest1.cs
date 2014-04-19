using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDRipperLib;
using MP3Tagger.Interfaces;
using MP3Tagger.Classes;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using CDDBAccess;
using MusicBucket;
using Yeti.MMedia.Mp3;

namespace CDRipperTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void CDDBAccessTest()
        {
            CDDBEntry[] dl;
            string extdata;
            //CDDBEntry onesecond;
            List<ID3Tag> tags;
            CDROM_TOC toc;
            CDRipper cdr = new CDRipper("h");
            CDDBConnection cddbconn;
            cddbconn = new CDDBConnection();
            toc = cdr.READ_TOC();
            dl=cddbconn.QueryCD(toc);
            tags = cddbconn.ReadCD(dl[0], MP3Tagger.TagTypeEnum.ID3v23,out extdata);
            //onesecond = cddbconn.ReadCD(dl[0]);
        }
        [TestMethod]
        public void TestCDRipping()
        {
            bool lockresult,unlockresult;
            CDRipper cdrip;
            CDROM_TOC toc;
            TRACK_DATA trdata;
            WaveWriter waveWriter;
            cdrip = new CDRipper("h");
            cdrip.NotifyProgress += DebugProgress;
            toc = cdrip.READ_TOC();
            lockresult=cdrip.LockDrive();
            trdata = toc.GetTrackData(0);
            waveWriter = new WaveWriter();
            waveWriter.CreateFile(@"C:\testing\UfEmMuenster.wav");
            cdrip.ReadTrack(toc, 0,waveWriter);
            waveWriter.CloseFile();

            /*
            for (int k = 0; k < toc.LastTrack-1; k++)
            {
                
            }*/
            unlockresult = cdrip.UnLockDrive();
        }

        [TestMethod]
        public void ReadID3V1()
        {
            IID3Tag tag;
            string fin;
            FileStream stream;
            fin=@"C:\Users\fuerh_000\Music\Solitude Aeturnus - Discography\1992 - Beyond The Crimson Horizon\07 - Plague Of Procreation.mp3";
            stream = File.OpenRead(fin);
            tag = new ID3v1();
            tag.Read(stream);
            stream.Close();
        }

        [TestMethod]
        public void ReadID3V23()
        {
            IID3Tag tag;
            string fin;
            FileStream stream;
            fin = @"C:\Users\fuerh_000\Music\Solitude Aeturnus - Discography\1992 - Beyond The Crimson Horizon\07 - Plague Of Procreation.mp3";
            stream = File.OpenRead(fin);
            tag = new ID3v23();
            tag.Read(stream);
            stream.Close();
        }

        [TestMethod]
        public void ReadSaveID3V23()
        {
            IID3Tag tag;
            string fin;
            FileStream stream;
            fin = @"C:\Users\fuerh_000\Music\Solitude Aeturnus - Discography\1992 - Beyond The Crimson Horizon\07 - Plague Of Procreation - Kopie.mp3";
            stream = File.Open(fin,FileMode.Open);
            tag = new ID3v23();
            tag.Read(stream);
            tag.Write(stream);
            stream.Close();
        }

        [TestMethod]
        public void TestMp3Write()
        {
            Stream outstream;
            Mp3Writer mp3writer;
            outstream = File.Open("C:\\testing\\muh.mp3", FileMode.Create, FileAccess.ReadWrite);
            try
            {
                mp3writer = new Mp3Writer(outstream, new Mp3WriterConfig());
            }
            finally
            {
                outstream.Close();
            }
        }

        void DebugProgress(int progress,int test)
        {
            Debug.WriteLine("Progress: " + progress + "%");
        }

        [TestMethod]
        public void WaveSpy()
        {
            //string fin = @"C:\testing\Track01.wav";
            string fin=@"C:\testing\UfEmMuenster.wav";

            byte[] data4 = new byte[4];
            byte[] data2 = new byte[2];
            string outstr;
            ushort outshort;
            uint outint;
            FileStream stream = File.Open(fin, FileMode.Open);
            stream.Read(data4, 0, 4);
            outstr = Encoding.ASCII.GetString(data4);
            stream.Read(data4, 0, 4);
            outint = BitConverter.ToUInt32(data4, 0);
            stream.Read(data4, 0, 4);
            outstr = Encoding.ASCII.GetString(data4); // "WAVE"
            stream.Read(data4, 0, 4);
            outstr = Encoding.ASCII.GetString(data4); // "fmt "
            stream.Read(data4, 0, 4);
            outint = BitConverter.ToUInt32(data4, 0); // format length
            stream.Read(data2, 0, 2);
            outshort = BitConverter.ToUInt16(data2, 0); // format tag
            stream.Read(data2, 0, 2);
            outshort = BitConverter.ToUInt16(data2, 0); // channels
            stream.Read(data4, 0, 4);
            outint = BitConverter.ToUInt32(data4, 0); // samplerate
            stream.Read(data4, 0, 4);
            outint = BitConverter.ToUInt32(data4, 0); // bytes / second
            stream.Read(data2, 0, 2);
            outshort = BitConverter.ToUInt16(data2, 0); // framesize
            stream.Read(data2, 0, 2);
            outshort = BitConverter.ToUInt16(data2, 0); // bits per sample
            stream.Close();
        }


        [TestMethod]
        public void EncodingTest()
        {
            string smiley;
            byte a = 97;
            smiley = Encoding.Unicode.GetString(new byte[] { 38, 59 });
            smiley += (char)a;
            int z = 0;
        }
    }
}

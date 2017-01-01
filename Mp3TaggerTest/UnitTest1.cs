using System;
using System.IO;
using System.Collections.Generic;
using MP3Tagger.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Mp3TaggerTest
{
    [TestClass]
    public class UnitTest1
    {
        string basepath = @"C:\Users\fuerh_000\Desktop\mp3_examples";//@"Z:\backup_magnolia_27_9_15\Musik";
        string logfilepath = @"C:\Users\fuerh_000\Documents";  
        IEnumerator<string> filelist;
        FileStream log_no_imgs;
        FileStream log_exceptions;
        FileStream log_converted;

        [TestMethod]
        public void TestMethod1()
        {

            // browse through all mp3 files
            filelist = Directory.EnumerateFiles(basepath,"*.mp3").GetEnumerator();

            // create logfiles
            log_no_imgs = File.OpenWrite(logfilepath + "\\mp3tag_conv_noimages.txt");
            log_exceptions = File.OpenWrite(logfilepath + "\\mp3tag_conv_errors.txt");
            log_converted = File.OpenWrite(logfilepath + "\\mp3tag_conv_converted.txt");

            while (filelist.MoveNext())
            {
                analysefile();
            }

            log_no_imgs.Close();
            log_exceptions.Close();
            log_converted.Close();
        }


        public void analysefile()
        {
            try
            {
                FileStream fstream = File.Open(filelist.Current, FileMode.Open);
                ID3v23 id3tag = new ID3v23();
                ID3v1 id3v1tag = new ID3v1();
                if (id3tag.Read(fstream))
                {
                    int imgfound = 0;
                    foreach (TagFrameV23 tf in id3tag.Frames)
                    {
                        if (tf is ImageTagFrameV23)
                        {
                            imgfound = 1;
                            if ((tf as ImageTagFrameV23).PictureType != 3 && imgfound != 4)
                            {
                                imgfound = 3;
                                (tf as ImageTagFrameV23).PictureType = 3;
                            }
                            else 
                            {
                                imgfound = 4;
                            }
                        }
                    }
                    if (imgfound == 3)
                    {
                        byte[] barray = System.Text.Encoding.UTF8.GetBytes(filelist.Current + "\r\n");
                        log_converted.Write(barray, 0, barray.Length);
                        id3tag.Write(fstream);
                    }
                    if (imgfound == 0)
                    {
                        byte[] barray = System.Text.Encoding.UTF8.GetBytes(filelist.Current + "\r\n");
                        log_no_imgs.Write(barray, 0, barray.Length);
                    }
                }
                else if (id3v1tag.Read(fstream))
                {
                    id3tag = (ID3v23)id3v1tag;
                    id3tag.Write(fstream);
 
                }
                fstream.Close();
            }
            catch (Exception e)
            { 
                byte[] barray = System.Text.Encoding.UTF8.GetBytes(filelist.Current + ": " + e.ToString() + "\r\n");
                log_exceptions.Write(barray, 0, barray.Length);
            }
        }
    }
}

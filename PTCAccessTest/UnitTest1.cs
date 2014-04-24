using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PTCAccess;
using System.Collections.Generic;
namespace PTCAccessTest
{
    [TestClass]
    [DeploymentItem("PortableDevice.h")]
    public class UnitTest1
    {
        [TestMethod]
        public void GetDevicesTest()
        {
            List<PTCDevice> devices;
            List<PTCFolder> folders;
            devices = PTCWrapper.GetDevices();
            Assert.IsNotNull(devices);
            //var portableDevice = wrapper.OpenDevice(devices[0].ID);
            folders = PTCWrapper.GetFolders(null, devices[0].ID);
        }

        [TestMethod]
        public void CopyFileTest()
        {
            PTCFolder outfolder;
            string pathin = @"C:\Users\fuerh_000\Music\plastic_world.mp3";
            string finalFileName = "TestingMusicBucket.mp3";
            string pathout = "[GT-I8190N]:\\Card\\music\\";
            PTCWrapper.Exists(pathout,out outfolder);
            PTCWrapper.CopyMp3File(pathin, finalFileName, outfolder);
        }
    }
}

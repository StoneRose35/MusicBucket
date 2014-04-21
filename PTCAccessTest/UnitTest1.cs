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
            PTCWrapper wrapper;
            List<PTCFolder> folders;
            wrapper = new PTCWrapper();
            devices = wrapper.GetDevices();
            Assert.IsNotNull(devices);
            //var portableDevice = wrapper.OpenDevice(devices[0].ID);
            folders = wrapper.GetFolders(null, devices[0].ID);
        }
    }
}

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
            string[] devices;
            PTCWrapper wrapper;
            List<string> folders;
            wrapper = new PTCWrapper();
            devices = wrapper.GetDevices();
            Assert.IsNotNull(devices);
            var portableDevice = wrapper.OpenDevice(devices[0]);
            folders = wrapper.GetFolders("RenderingInformation", devices[0]);
            int end = 1;
        }
    }
}

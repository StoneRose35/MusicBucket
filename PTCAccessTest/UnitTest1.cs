using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PTCAccess;

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
            string objectsstr;
            uint fetched=0;
            PortableDeviceApiLib.IPortableDeviceContent content;
            PortableDeviceApiLib.IEnumPortableDeviceObjectIDs oids;
            wrapper = new PTCWrapper();
            devices = wrapper.GetDevices();
            Assert.IsNotNull(devices);
            var portableDevice = wrapper.OpenDevice(devices[0]);
            portableDevice.Content(out content);
            content.EnumObjects(0, "DEVICE", null, out oids);
            oids.Next(1, out objectsstr, ref fetched);
            oids.Next(1, out objectsstr, ref fetched);
            oids.Next(1, out objectsstr, ref fetched);
            oids.Next(1, out objectsstr, ref fetched);
            int end = 1;
        }
    }
}

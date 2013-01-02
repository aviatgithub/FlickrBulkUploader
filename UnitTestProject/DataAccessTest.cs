using System;
using HashNash.FlickrUploader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class DataAccessTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            DataAccess da = new DataAccess("test.sqlite");
        }
    }
}

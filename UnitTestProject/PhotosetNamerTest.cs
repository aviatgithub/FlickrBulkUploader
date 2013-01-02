using HashNash.FlickrUploader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class PhotosetNamerTest
    {
        [TestMethod]
        public void PhotosetNamer_1Level()
        {
            var fn = new PhotosetNamer();

            string psname = fn.GetPhotoSetName(@"c:\flickrsyncfolder\folder1\img1.jpg");

            Assert.AreEqual(psname,"folder1");
        }

        [TestMethod]
        public void PhotosetNamer_2Level()
        {
            var fn = new PhotosetNamer();

            string psname = fn.GetPhotoSetName(@"c:\flickrsyncfolder\folder1\sub1\img1.jpg");

            Assert.AreEqual(psname, "folder1_sub1");
        }

        [TestMethod]
        public void PhotosetNamer_3Level()
        {
            var fn = new PhotosetNamer();

            string psname = fn.GetPhotoSetName(@"c:\flickrsyncfolder\folder1\sub1\sub2\img1.jpg");

            Assert.AreEqual(psname, "folder1_sub1_sub2");
        }

        [TestMethod]
        public void PhotosetNamer_noLevel()
        {
            //Arrange
            var fn = new PhotosetNamer();

            //Act
            string psname = fn.GetPhotoSetName(@"c:\flickrsyncfolder\img1.jpg");

            //Assert
            Assert.AreEqual(psname, "General");
        }

        [TestMethod]
        public void PhotosetNamer_WithComma()
        {
            //Arrange
            var fn = new PhotosetNamer();

            //Act
            string psname = fn.GetPhotoSetName(@"c:\flickrsyncfolder\Aswini\me,me and only me\img1.jpg");

            //Assert
            Assert.AreEqual(psname, "aswini_me,me and only me");
        }

    }
}

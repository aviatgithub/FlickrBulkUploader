using System;
using System.IO;

namespace HashNash.FlickrUploader
{
    public class AImg
    {
        public AImg(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                //throw ex ?
            }
            else
            {
                FileFullPath = fileFullPath;
                FileName = Path.GetFileName(FileFullPath);

                var fInfo = new FileInfo(FileFullPath);
                FolderName = fInfo.Directory.Name;

                PhotoSetName = new PhotosetNamer().GetPhotoSetName(fileFullPath);
            }
        }

        public string FileFullPath { get; private set; }
        public string FolderName { get; private set; }
        
        //Upload
        public string FileName { get; private set; }
        public string FlickrPhotoId { get; set; }// From Flickr
        public bool IsUploaded { get; set; }
        public Exception UploadEx { get; set; }
        public DateTime DateUploaded { get; set; }
        public int SecondsToUpload { get; set; }

        //AddToSet
        public string PhotoSetName { get; private set; }
        public string FlickrPhotoSetId { get;  set; }//From Flickr
        public bool IsAddToSetCompleted { get; set; }
        public Exception AddToSetEx { get; set; }
        public DateTime DateAddedToSet { get; set; }
        public int SecondsToAddToSet { get; set; }

        public string Print()
        {
            string exstrting = "";
            if (UploadEx != null)
            {
                exstrting = UploadEx.ToString();
            }

            if (AddToSetEx != null)
            {
                exstrting += AddToSetEx.ToString();
            }


            return string.Format(" {0},{1},{2},{3},{4},{5},{6},{7}", this.FileName, this.FolderName,
                                 this.FileFullPath,this.PhotoSetName,
                                 IsUploaded, IsAddToSetCompleted, FlickrPhotoId, exstrting);
        }

        public override string ToString()
        {
            string exstrting = "";
            if (UploadEx != null)
            {
                exstrting = UploadEx.ToString();
            }

            if (AddToSetEx != null)
            {
                exstrting += AddToSetEx.ToString();
            }

            return string.Format(" {0},{1},Upload:{2},AddToSet:{3},{4},{5},{6},{7}", this.FileName, this.FolderName,
               IsUploaded, IsAddToSetCompleted,this.FileFullPath,this.PhotoSetName, FlickrPhotoId, exstrting);
        }
    }
}
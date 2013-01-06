using System;
using System.IO;

namespace HashNash.FlickrUploader
{
    public class AImg
    {
        private FileInfo _fileInfo;
        private string _folderName;
        private string _fileName;
        private string _photoSetName;

        public AImg(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                //throw ex ?
            }
            else
            {
                FileFullPath = fileFullPath.ToLower();
            }
        }

        public string FileFullPath { get; private set; }

        
        public FileInfo FileInfo
        {
            get
            {
                if (_fileInfo == null)
                {
                    _fileInfo = new FileInfo(FileFullPath);
                }
                return _fileInfo;
            }
        }

        public string FolderName
        {
            get
            {
                if (string.IsNullOrEmpty(_folderName))
                {
                    _folderName = FileInfo.Directory.Name;
                }
                return _folderName;
            }
        }
        
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                {
                    _fileName = Path.GetFileName(FileFullPath);
                }
                return _fileName;
            }
        }

        public string PhotoSetName
        {
            get
            {
                if (string.IsNullOrEmpty(_photoSetName))
                {
                    _photoSetName = new PhotosetNamer().GetPhotoSetName(FileFullPath);
                }
                return _photoSetName;
            }
        }

        //Upload
        public string FlickrPhotoId { get; set; }// From Flickr
        public bool IsUploaded { get; set; }
        public Exception UploadEx { get; set; }
        public DateTime DateUploaded { get; set; }
        public double SecondsToUpload { get; set; }

        //AddToSet
        public string FlickrPhotoSetId { get;  set; }//From Flickr
        public bool IsAddToSetCompleted { get; set; }
        public Exception AddToSetEx { get; set; }
        public DateTime DateAddedToSet { get; set; }
        public double SecondsToAddToSet { get; set; }

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

            return string.Format(" {0},{1},Upload:{2},AddToSet:{3},{4},{5},{6},{7}", this.FileName,this.PhotoSetName,
               IsUploaded, IsAddToSetCompleted, this.PhotoSetName, FlickrPhotoId, FlickrPhotoSetId, exstrting);
        }

        public bool IsEqualToPath(String filepath)
        {
            return filepath.ToLower() == FileFullPath;
        }
    }
}
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

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Size
        {
            get { return FileInfo.Length; }
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
        public string FlickrPhotoId { get; private set; }// From Flickr
        public bool IsUploaded { get; private set; }
        public Exception UploadEx { get; set; }
        public DateTime DateUploaded { get;private set; }
        public double SecondsToUpload { get; private set; }

        //AddToSet
        public string FlickrPhotoSetId { get; private set; }//From Flickr
        public bool IsAddToSetCompleted { get; private set; }
        public Exception AddToSetEx { get; set; }
        public DateTime DateAddedToSet { get; private set; }
        public double SecondsToAddToSet { get; private set; }

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

            return string.Format(" {0},{1},Upload:{2},AddToSet:{3},{4},{5},{6},{7}", this.FileName, this.PhotoSetName,
               IsUploaded, IsAddToSetCompleted, this.PhotoSetName, FlickrPhotoId, FlickrPhotoSetId, exstrting);
        }

        public void UpdatePhotosetId(string flickrpsid,DateTime date = default (DateTime),double seconds =0)
        {
            if (string.IsNullOrWhiteSpace(flickrpsid))
            {
                throw new ArgumentNullException("flickrpsid");
            }
            FlickrPhotoSetId = flickrpsid;
            IsAddToSetCompleted = true;
            DateAddedToSet = date;
            SecondsToAddToSet = seconds;
        }

        public bool IsEqualToPath(String filepath)
        {
            return filepath.ToLower() == FileFullPath;
        }

        public void UpdateFlickrPhotoId(string flickrid, DateTime date = default (DateTime),double seconds =0)
        {
            if (string.IsNullOrWhiteSpace(flickrid))
            {
                throw new ArgumentNullException("flickrid");
            }
            FlickrPhotoId = flickrid;
            IsUploaded = true;
            DateUploaded = date;
            SecondsToUpload = seconds;
        }
    }
}
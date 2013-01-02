using System;
using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class Uploader
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Flickr _flickr;

        public Uploader(Flickr flickr)
        {
            this._flickr = flickr;
        }

        public void Upload(AImg img)
        {

            try
            {
                string response = _flickr.UploadPicture(img.FileFullPath, img.FileName,
                                                       description: string.Empty, 
                                                       tags: string.Empty,
                                                       isPublic: false,
                                                       isFamily: false, isFriend: false);

                _log.DebugFormat("Upload success . Response :" + response);

                img.FlickrPhotoId = response;
                img.IsUploaded = true;
            }
            catch (Exception ex)
            {
                img.IsUploaded = false;
                img.UploadEx = ex;

                _log.Error(string.Format("error uploading " + img.FileFullPath), ex);

            }
        }
    }
}
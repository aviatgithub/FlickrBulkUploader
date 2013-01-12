using System;
using System.Diagnostics;
using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class FlickrUploadMan
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Flickr _flickr;
        private AppConfig _config;

        public FlickrUploadMan(Flickr flickr)
        {
             _config = new AppConfig();
            this._flickr = flickr;
        }

        public void Upload(AImg img)
        {

            try
            {
                Stopwatch stopWatch = Stopwatch.StartNew();

                string response = _flickr.UploadPicture(img.FileFullPath, img.FileName,
                                                        description: img.FolderName,
                                                        tags: img.FolderName,
                                                        isPublic: _config.IsPrivate,
                                                        isFamily: _config.IsPrivate,
                                                        isFriend: _config.IsPrivate);
                stopWatch.Stop();
                _log.DebugFormat("Upload success. Took:{0}s. Response :{1}", stopWatch.Elapsed.TotalSeconds, response);
                img.UpdateFlickrPhotoId(response, DateTime.Now, stopWatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                img.UploadEx = ex;
                _log.Error(string.Format("error uploading " + img.FileFullPath), ex);
            }
        }
    }
}
﻿using System;
using System.Diagnostics;
using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class FlickrUploadMan
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Flickr _flickr;

        public FlickrUploadMan(Flickr flickr)
        {
            this._flickr = flickr;
        }

        public void Upload(AImg img)
        {

            try
            {
                Stopwatch stopWatch = Stopwatch.StartNew();

                string response = _flickr.UploadPicture(img.FileFullPath, img.FileName,
                                                        description: string.Empty,
                                                        tags: string.Empty,
                                                        isPublic: false,
                                                        isFamily: false,
                                                        isFriend: false);
                stopWatch.Stop();
                img.SecondsToUpload = stopWatch.Elapsed.TotalSeconds;

                _log.DebugFormat("Upload success. Took:{0}s. Response :{1}" ,img.DateUploaded, response);

                img.DateUploaded = DateTime.Now;
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
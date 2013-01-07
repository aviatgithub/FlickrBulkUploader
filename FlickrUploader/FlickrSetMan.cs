using System;
using System.Diagnostics;
using System.Linq;
using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class FlickrSetMan
    {
        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private  PhotosetCollection _collection;
        
        private readonly Flickr _flickr;

        public FlickrSetMan(Flickr flickr)
        {
            _log.Info("ctor");
            _flickr = flickr;
        }

        /// <summary>
        /// Gets all sets
        /// </summary>
        /// <exception cref="System.Exception">flickr error</exception>
        private void Refresh()
        {
            _collection = _flickr.PhotosetsGetList();

//            _collection = _flickr.PhotosetsGetList(1,500);
        }

        private bool Exists(string photosetname,ref Photoset set)
        {
           Refresh();

           set = _collection.FirstOrDefault(x => x.Title.ToLower() == photosetname.ToLower());

           _log.InfoFormat("this folder {0} {1} exists as set.", photosetname, set != null ? "" : "DOES NOT");

            return set != null;
        }

        /// <summary>
        /// This is called for each grouping.This can throw exception !!!
        /// </summary>
        /// <param name="photosetname"></param>
        /// <param name="photoid"></param>
        /// <returns></returns>
        public Photoset GetSet(string photosetname, AImg photoid)
        {
            Photoset existingset = null;

            if (Exists(photosetname, ref existingset))
            {
                return existingset;
            }

            Photoset newset = _flickr.PhotosetsCreate(photosetname, photoid.FlickrPhotoId);

            return newset;
        }


        public void AddPhotoToSet(AImg img,Photoset set)
        {
            _log.InfoFormat("adding {0} to photoset : {1}",img.FileFullPath,set.PhotosetId);

            try
            {
                Stopwatch stopWatch = Stopwatch.StartNew();
                _flickr.PhotosetsAddPhoto(set.PhotosetId, img.FlickrPhotoId);
                stopWatch.Stop();
                _log.DebugFormat("Add to Set success. Took:{0}s.", stopWatch.Elapsed.TotalSeconds);
                img.UpdatePhotosetId(set.PhotosetId, DateTime.Now, stopWatch.Elapsed.TotalSeconds);
            }
            catch (FlickrApiException ex)
            {
                if (ex.Code == 3) //photo already exists.
                {
                    //DONT consider this as failure
                    img.UpdatePhotosetId(set.PhotosetId, DateTime.Now, 0);
                }
                else
                {
                    img.AddToSetEx = ex;
                    _log.Error(string.Format("Failed to add {0} to photoset : {1}", img.FileFullPath, set.Title), ex);
                }
            }
            catch (Exception ex)
            {
                img.AddToSetEx = ex;
                _log.Error(string.Format("Failed to add {0} to photoset : {1}", img.FileFullPath, set.Title), ex);
            }
        }
    }
}
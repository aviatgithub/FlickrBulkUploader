using System;
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
        }

        private bool Exists(string photosetname,ref Photoset set)
        {
           Refresh();

           set = _collection.FirstOrDefault(x => x.Title == photosetname);

           _log.InfoFormat("this folder {0} {1} exists as set.", photosetname, set != null ? "" : "DOES NOT");

            return set != null;
        }

        //THis is called for each grouping

        /// <summary>
        /// This can throw exception !!!
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
            _log.InfoFormat("adding {0} to photoset : {1}",img.FileFullPath,set.Title);

            try
            {
                _flickr.PhotosetsAddPhoto(set.PhotosetId, img.FlickrPhotoId);
                img.IsAddToSetCompleted = true;
            }
            catch (FlickrApiException ex)
            {
                if (ex.Code == 3) //photo already exists.
                {
                    img.IsAddToSetCompleted = true; //DONT consider this as failure
                }
                else
                {
                    img.AddToSetEx = ex;
                    img.IsAddToSetCompleted = false;
                    _log.Error(string.Format("Failed to add {0} to photoset : {1}", img.FileFullPath, set.Title), ex);
                }
            }
            catch (Exception ex)
            {
                img.AddToSetEx = ex;
                img.IsAddToSetCompleted = false;
                _log.Error(string.Format("Failed to add {0} to photoset : {1}", img.FileFullPath, set.Title), ex);
            }
        }
    }
}
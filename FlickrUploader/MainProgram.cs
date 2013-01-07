using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class MainProgram
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FlickrUploadMan _flickrUploadMan;
        private FlickrProxy _flproxy;
        private List<AImg> _pendingImagesForUpload;
        private List<AImg> _pendingImagesForAddSet;
        private List<AImg> _allimages;
        private IDataAccess _dataaccess;
        private FlickrSetMan _setman;
        readonly AppConfig _config = new AppConfig();

        public void Do(string[] args)
        {
            string dir = Path.Combine(_config.FlickrDir, _config.FolderToScan);

            //see if there are files to upload
            bool proceed = DoInitCheck(dir);

            if (proceed == false)
            {
                return;
            }

            //Auth
            DoAuth();

            //Auth succeeded
            

            if (_config.DebugMode)
            {
                DebugSettingsSave();
                // DebugDataAccess();

                _log.InfoFormat("in Debug Mode..  Quiting...");
                return;
            }

            DoFlickrUpload(_pendingImagesForUpload);
            DoFlickrAddToSet(_pendingImagesForAddSet);
        }

        /// <summary>
        ///Populate _images,_pendingImagesForUpload,_pendingImagesForAddSet
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool DoInitCheck(string dir)
        {
            _log.InfoFormat("DoInitCheck: {0}", dir);
            _dataaccess = new DataAccess();
            _allimages = _dataaccess.GetAllStatus(dir);
            _log.InfoFormat("Total Count :{0}", _allimages.Count);
            _pendingImagesForUpload = _allimages.Where(image => image.IsUploaded == false).ToList();
            _pendingImagesForAddSet = _allimages.Where(image => image.IsAddToSetCompleted == false).ToList();

            if (_pendingImagesForUpload.Count == 0 && _pendingImagesForAddSet.Count == 0)
            {
                _log.InfoFormat("All images already uploaded and Added to set ...");
                return false;
            }

            Print();

           

            return true;
        }

        private void DebugSettingsSave()
        {
            //_config.UpdateVerificationCode("testupdate");
        }

        private void DebugDataAccess()
        {

            ////test dataacess
            //_allimages[0].IsUploaded = true;
            //_allimages[0].DateUploaded = DateTime.Now;
            //_allimages[0].SecondsToUpload = 1.343;
            //_allimages[0].FlickrPhotoId = "fid";
            //_dataaccess.SaveUploadStatus(_allimages[0]);

            //_allimages[0].IsAddToSetCompleted = true;
            //_allimages[0].DateAddedToSet = DateTime.Now;
            //_allimages[0].SecondsToAddToSet = 2.343;
            //_allimages[0].FlickrPhotoSetId = "fsid";
            //_dataaccess.SaveAddToSetStatus(_allimages[0]);
        }

        private void Print()
        {
            _log.InfoFormat("Pending for upload : {0} out of {1}", _pendingImagesForUpload.Count, _allimages.Count);
            _log.InfoFormat("Pending for AddToSet : {0} out of {1}", _pendingImagesForAddSet.Count, _allimages.Count);
            _log.InfoFormat(" +++++++++++++  Printing Pending Images For Upload... +++++++++++++ ");

            foreach (var pendingUpload in _pendingImagesForUpload)
            {
                _log.Info(pendingUpload.ToString());
            }

            _log.InfoFormat(" +++++++++++++ END Printing Pending Images For Upload... +++++++++++++ ");
            _log.InfoFormat(" +++++++++++++  Printing Pending Images for AddToSet... +++++++++++++ ");

            foreach (var pendingAddToSet in _pendingImagesForAddSet)
            {
                _log.Info(pendingAddToSet.ToString());
            }

            _log.InfoFormat(" +++++++++++++ END Printing Pending Images for AddToSet... +++++++++++++ ");
        }

        private void DoAuth()
        {
            _flproxy = new FlickrProxy();

            if (string.IsNullOrWhiteSpace(_config.OAuthAccessToken))
            {
                string url = _flproxy.GetFrobAuthUrl();
                _log.InfoFormat("Auth url : {0}", url);
                Process.Start(url); //

                Console.WriteLine(
                    " \r\nClick authorize in Flickr Page");

                Console.ReadLine();
                _flproxy.SetUserVerified();
            }
            else
            {
                _flproxy.Connect();
            }
        }
        /*
        private void DoAuth_NoFrob()
        {
            _flproxy = new FlickrProxy();

            if (string.IsNullOrWhiteSpace(_config.OAuthAccessToken))
            {

                string url = _flproxy.GetAuthUrl();
                _log.InfoFormat("Auth url : {0}", url);
                Process.Start(url); //

                Console.WriteLine(
                    " \r\nClick authorize in Flickr Page,this will take to a diff page with verification code." +
                    " \r\n Enter the verification Code: : : ");
                
                string verificationotp = Console.ReadLine();
                _flproxy.SetVerificationCode(verificationotp);

            }
            else
            {
                _flproxy.SetToken(_config.OAuthAccessToken);
            }
        }
        */
        private void DoFlickrUpload(List<AImg> pendingImages)
        {
            _log.Info("DoFlickrUpload");
            _flickrUploadMan = new FlickrUploadMan(_flproxy.FlickrObj);
            var pendcount = pendingImages.Count;
            
            for (int i = 0; i < pendcount; i++)
            {
                _log.InfoFormat("Uploading {0} of {1} . Details : {2}", i + 1, pendcount, pendingImages[i]);
                _flickrUploadMan.Upload(pendingImages[i]);
                _dataaccess.SaveUploadStatus(pendingImages[i]);
            }

            _log.Info("Upload completed. Printing post upload status");

            foreach (var pendingImage in pendingImages)
            {
                _log.Info(pendingImage.ToString());
            }
        }

        private void DoFlickrAddToSet(List<AImg> pendingImagesForAddSet)
        {
            _log.Info("DoFlickrAddToSet");
            _setman = new FlickrSetMan(_flproxy.FlickrObj);

            var groupedImages =
                from img in pendingImagesForAddSet
                group img by img.PhotoSetName
                into grouping
                select grouping;

            foreach (IGrouping<string, AImg> grouping in groupedImages)
            {
                string photosetname = grouping.Key;
                Photoset setForThisGroup = null;
                AImg primaryimg = grouping.ToList()[0];
                bool isSetAvailable = false;

                try
                {
                    setForThisGroup = _setman.GetSet(photosetname, primaryimg);
                    isSetAvailable = true;
                }
                catch (Exception ex)
                {
                    _log.Error("Unable to get set for this group " + photosetname, ex);
                    //unable to get for this group..
                    isSetAvailable = false;
                }

                if (isSetAvailable)
                {
                    foreach (AImg aImg in grouping)
                    {
                        _setman.AddPhotoToSet(aImg, setForThisGroup);
                        _dataaccess.SaveAddToSetStatus(aImg);
                    }
                }
                else
                {
                    //ignore all fotos in this group
                    //foreach (AImg aImg in grouping)
                    //{
                        //unable to create set or some other problem.
                    //}
                }//issetavailable - false

            }//end of foreach
        }      
    }
}
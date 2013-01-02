using System;
using System.Collections.Generic;
using System.Configuration;
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
        private Uploader _uploader;
        private FlickrProxy _flproxy;
        private List<AImg> _pendingImagesForUpload;
        private List<AImg> _pendingImagesForAddSet;
        private List<AImg> _allimages;
        private IDataAccess _dataaccess;
        private SetMan _setman;
        private bool _justPrintFileNames;

        public void DO(string[] args)
        {
            _justPrintFileNames = Convert.ToBoolean(ConfigurationManager.AppSettings["justPrintFileNames"]);
            
            string dir = Path.Combine( ConfigurationManager.AppSettings["flickrdir"] ,ConfigurationManager.AppSettings["foldertoscan"]);
             
        //DB setup
            bool dbready = DoInitDb();
            if (dbready == false)
            {
                return;
            }

            //see if there are files to upload
            bool proceed = DoInitCheck(dir);

            if (proceed == false)
            {
                return;
            }

            //Auth
            DoAuth();

            //Auth succeeded

            DoUpload(_pendingImagesForUpload);

            DoSet(_pendingImagesForAddSet);

        }

        private bool DoInitDb()
        {
            _dataaccess = new DataAccess("test.sqlite");

            return false;
        }

        /// <summary>
        ///Populate _images,_pendingImagesForUpload,_pendingImagesForAddSet
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool DoInitCheck(string dir)
        {
            _log.InfoFormat("DoInitCheck: {0}", dir);

            _allimages = _dataaccess.GetAllStatus(dir);

            var fl = new FilesLister();
            _allimages = fl.List(dir);

            _log.InfoFormat("Total Count :{0}", _allimages.Count);

            _pendingImagesForUpload = _allimages.Where(image => image.IsUploaded == false).ToList();
            _pendingImagesForAddSet = _allimages.Where(image => image.IsAddToSetCompleted == false).ToList();

            if (_pendingImagesForUpload.Count == 0 && _pendingImagesForAddSet.Count == 0)
            {
                _log.InfoFormat("All images already uploaded and Added to set ...");
                return false;
            }

            Print();

            if (_justPrintFileNames)
            {
                _log.InfoFormat("justPrintFileNames..  Quiting...");
                return false;
            }

            return true;
        }

        private void Print()
        {
            _log.InfoFormat("Pending for upload : {0} out of {1}", _pendingImagesForUpload.Count, _allimages.Count);
            _log.InfoFormat("Pending for AddToSet : {0} out of {1}", _pendingImagesForAddSet.Count, _allimages.Count);

            _log.InfoFormat(" +++++++++++++  Printing Pending Images For Upload... +++++++++++++ ");

            foreach (var pendingUpload in _pendingImagesForUpload)
            {
                _log.Info(pendingUpload.Print());
            }

            _log.InfoFormat(" +++++++++++++ END Printing Pending Images For Upload... +++++++++++++ ");

            _log.InfoFormat(" +++++++++++++  Printing Pending Images for AddToSet... +++++++++++++ ");

            foreach (var pendingAddToSet in _pendingImagesForAddSet)
            {
                _log.Info(pendingAddToSet.Print());
            }

            _log.InfoFormat(" +++++++++++++ END Printing Pending Images for AddToSet... +++++++++++++ ");

        }


        private void DoUpload(List<AImg> pendingImages)
        {
            _log.Info("DoUpload");

            //Upload
            var pendcount = pendingImages.Count;
            for (int i = 0; i < pendcount; i++)
            {
                _log.InfoFormat("Uploading {0} of {1} . Details : {2}", i + 1, pendcount, pendingImages[i]);

                _uploader.Upload(pendingImages[i]);

                _dataaccess.SaveUploadStatus(pendingImages[i]);

            }

            _log.Info("Upload completed. Printing post upload status");

            foreach (var pendingImage in pendingImages)
            {
                _log.Info(pendingImage.Print());
            }

        }

        private void DoSet(List<AImg> pendingImagesForAddSet)
        {
            //Get photoset
            _setman = new SetMan(_flproxy.FlickrObj);

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
                    }
                }
                else
                {
                    //set all fotos  in this set to false..

                    foreach (AImg aImg in grouping)
                    {
                        aImg.IsAddToSetCompleted = false; 
                    }
                }//issetavailable - false

            }//end of foreach
        }

        private void DoAuth()
        {
            //upload

            _flproxy = new FlickrProxy();
            string url = _flproxy.GetAuthUrl();

            _log.InfoFormat("Auth url : {0}", url);

            Process.Start(url);

            Console.WriteLine("\r\n Enter the verification Code: : : ");
            string verificationotp = Console.ReadLine();

            _flproxy.Set(verificationotp);

            _uploader = new Uploader(_flproxy.FlickrObj);
        }
    }
}
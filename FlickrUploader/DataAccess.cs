using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using log4net;

namespace HashNash.FlickrUploader
{
    public class DataAccess : IDataAccess
    {
        private static string tblImages = "tblImages";
        private static string colFullFilePath = "FullFilePath";
        private static string colFlickrImgId = "FlickrImgId";
        private static string colFlickrPhotoSetId = "FlickrPhotoSetId";
        private static string colDateUploaded = "DateUploaded";
        private static string colDateAddedToSet = "DateAddedToSet";
        private static string colSecondsToUpload = "SecondsToUpload";
        private static string colSecondsToAddToSet = "SecondsToAddToSet";

        private static readonly ILog _log =
          LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly AppConfig _config = new AppConfig();

        public DataAccess()
        {
            Init(_config.DbFilename);
        }

        private string _dbName;
        private void Init(string dbname)
        {
            _dbName = dbname;
            _log.InfoFormat("Init with {0}", dbname);

            if (File.Exists(dbname) == false)
            {
                _log.Info("creating new");
                SQLiteConnection.CreateFile(dbname);

                string sql = string.Format("CREATE TABLE {0} ({1} TEXT,{2} TEXT,{3} TEXT,{4} DATETIME,{5} DATETIME, {6} REAL,{7} REAL)",
                                           tblImages,
                                           colFullFilePath,//1
                                           colFlickrImgId,//2
                                           colFlickrPhotoSetId,//3
                                           colDateUploaded,//4
                                           colDateAddedToSet,//5
                                           colSecondsToUpload,//6
                                           colSecondsToAddToSet//7
                                           );

                using (var dbConnection = GetConnection())
                {
                    using (var cmd = new SQLiteCommand(sql, dbConnection))
                    {
                        int result = cmd.ExecuteNonQuery();
                        _log.InfoFormat("create table command {0}", result);

                    }
                }
            }
        }

        /// <summary>
        /// Creates new conn and Opens it. 
        /// Note:Make sure to close this when completed.
        /// </summary>
        private SQLiteConnection GetConnection()
        {
            var dbConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", _dbName));
            dbConnection.Open();
            return dbConnection;
        }

        public void SaveUploadStatus(AImg img)
        {
            if (img.IsUploaded == false)
            {
                return;
            }

            string updateUpload = string.Format("update {0} " +
                                                "set {1} = \"{2}\" ," +
                                                " {3} = \"{4}\" ," +
                                                " {5} = \"{6}\"" +
                                                "where {7} = \"{8}\"",
                                                tblImages,
                                                colFlickrImgId, img.FlickrPhotoId,
                                                colDateUploaded, img.DateUploaded.ToString("yyyy-MM-dd HH:mm:ss.000"),
                                                colSecondsToUpload, img.SecondsToUpload,
                                                colFullFilePath, img.FileFullPath);

            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(updateUpload, dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveAddToSetStatus(AImg img)
        {
            if (img.IsAddToSetCompleted == false)
            {
                return;
            }

            string updateAddToSet = string.Format("update {0} " +
                                                  "set {1} = \"{2}\" ," +
                                                  " {3} = \"{4}\" ," +
                                                  " {5} = \"{6}\"" +
                                                  "where {7} = \"{8}\"",
                                                  tblImages,
                                                  colFlickrPhotoSetId, img.FlickrPhotoSetId,
                                                  colDateAddedToSet, img.DateAddedToSet.ToString("yyyy-MM-dd HH:mm:ss.000"),
                                                  colSecondsToAddToSet, img.SecondsToAddToSet,
                                                  colFullFilePath, img.FileFullPath);

            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(updateAddToSet, dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<AImg> GetAllStatus(string foldertoscan)
        {
            var filesLister = new FilesLister();

            List<AImg> imagesInFilePath = filesLister.List(foldertoscan);

            if (imagesInFilePath.Count == 0)
            {
                _log.Info("no files in this directory");
                
                return imagesInFilePath;
            }

            //update status from DB. 

            string sqlselect =string.Format( "select {0},{1},{2} from {3}",
                                             colFullFilePath,colFlickrPhotoSetId,colFlickrImgId, 
                                             tblImages);

            var imgsInDB = new List<AImg>();

            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(sqlselect,conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fullfilepath = reader[colFullFilePath].ToString();
                            string flickrid = ConvertToString(reader[colFlickrImgId]);
                            string flickrphotosetid = ConvertToString(reader[colFlickrPhotoSetId]);

                            imgsInDB.Add(new AImg(fullfilepath)
                                {
                                    IsUploaded = string.IsNullOrEmpty(flickrid) == false,
                                    FlickrPhotoId = flickrid,
                                    FlickrPhotoSetId = flickrphotosetid,
                                    IsAddToSetCompleted = string.IsNullOrEmpty(flickrphotosetid) == false
                                });
                        }
                    }
                }
            }
            
            Update(imgsInDB,imagesInFilePath);
           
            return imagesInFilePath;
        }

        private void Update(List<AImg> imgListInDb, List<AImg> imgListInFilePath)
        {
            foreach (var imgInFilePath in imgListInFilePath)
            {
                var matchingImgInDb =
                    imgListInDb.FirstOrDefault(imgInDb => imgInDb.IsEqualToPath(imgInFilePath.FileFullPath));
                
                if (matchingImgInDb == null)
                {
                    //not in DB. This is a newly added file in the folder. Add to DB
                    AddToDb(imgInFilePath.FileFullPath);
                }
                else
                {
                    //Update status from DB 
                    imgInFilePath.IsUploaded            = matchingImgInDb.IsUploaded;
                    imgInFilePath.IsAddToSetCompleted   = matchingImgInDb.IsAddToSetCompleted;
                    imgInFilePath.FlickrPhotoId         = matchingImgInDb.FlickrPhotoId;
                    imgInFilePath.FlickrPhotoSetId      = matchingImgInDb.FlickrPhotoSetId;
                }
            }
        }

        private void AddToDb(string fullFilePath)
        {
            string insertstatement = string.Format("insert into {0} ({1}) values (\"{2}\")",
                                                   tblImages,
                                                   colFullFilePath, fullFilePath);

            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(insertstatement, dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private string ConvertToString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            if (obj == DBNull.Value)
            {
                return string.Empty;
            }
            return obj.ToString();
        }
    }
}
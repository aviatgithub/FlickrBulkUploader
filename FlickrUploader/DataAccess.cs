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
        private static string colFullFilePath = "colFullFilePath";
        private static string colFlickrImgId = "colFlickrImgId";
        private static string colFlickrPhotoSetId = "colFlickrPhotoSetId";
        private static string colDateUploaded = "colDateUploaded";
        private static string colDateAddedToSet = "colDateAddedToSet";
        private static string colSecondsToUpload = "colSecondsToUpload";
        private static string colSecondsToAddToSet = "colSecondsToAddToSet";
        private static string colSize = "colSize";

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

                string sql = string.Format("CREATE TABLE {0} ({1} TEXT,{2} REAL,{3} TEXT,{4} TEXT,{5} DATETIME,{6} DATETIME, {7} REAL,{8} REAL)",
                                           tblImages,
                                           colFullFilePath,//1
                                           colSize,//2
                                           colFlickrImgId,//3
                                           colFlickrPhotoSetId,//4
                                           colDateUploaded,//5
                                           colDateAddedToSet,//6
                                           colSecondsToUpload,//7
                                           colSecondsToAddToSet//8
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
                                                " {5} = \"{6}\" " +
                                                "where {7} = \"{8}\"",
                                                tblImages,
                                                colFlickrImgId, img.FlickrPhotoId,
                                                colDateUploaded, img.DateUploaded.ToString("yyyy-MM-dd HH:mm:ss.000"),
                                                colSecondsToUpload, img.SecondsToUpload,
                                                colFullFilePath, img.FileFullPath);

            using (var dbConnection = GetConnection())
            {
                using (var cmd = new SQLiteCommand(updateUpload, dbConnection))
                {
                    int result = cmd.ExecuteNonQuery();
                    _log.InfoFormat("SaveUploadStatus {0}", result);
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
                    int result = command.ExecuteNonQuery();
                    _log.InfoFormat("SaveAddToSetStatus {0}", result);

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
            var imglistInDb = GetImgsInDb();
            Update(imglistInDb, imagesInFilePath);
            return imagesInFilePath;
        }

        private List<AImg> GetImgsInDb()
        {
            string sqlselect = string.Format("select {0},{1},{2} from {3}",
                                           colFullFilePath, colFlickrPhotoSetId, colFlickrImgId,
                                           tblImages);

            var imgsInDb = new List<AImg>();

            using (var conn = GetConnection())
            {
                using (var command = new SQLiteCommand(sqlselect, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fullfilepath = reader[colFullFilePath].ToString();
                            string flickrid = ConvertToString(reader[colFlickrImgId]);
                            string flickrphotosetid = ConvertToString(reader[colFlickrPhotoSetId]);
                            var imgdb = new AImg(fullfilepath);

                            if (string.IsNullOrEmpty(flickrphotosetid) == false)
                            {
                                imgdb.UpdatePhotosetId(flickrphotosetid);
                            }

                            if (string.IsNullOrEmpty(flickrid) == false)
                            {
                                imgdb.UpdateFlickrPhotoId(flickrid);
                            }
                            imgsInDb.Add(imgdb);
                        }
                    }
                }
            }
            return imgsInDb;
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
                    AddToDb(imgInFilePath);
                }
                else
                {
                    //Update status from DB 
                    if (matchingImgInDb.IsUploaded)
                    {
                       imgInFilePath.UpdateFlickrPhotoId(matchingImgInDb.FlickrPhotoId); 
                    }
                    if (matchingImgInDb.IsAddToSetCompleted)
                    {
                        imgInFilePath.UpdatePhotosetId(matchingImgInDb.FlickrPhotoSetId);
                    }
                }
            }
        }

        private void AddToDb(AImg imgToAdd)
        {
            string insertstatement = string.Format("insert into {0} ({1},{2}) values (\"{3}\",{4})",
                                                   tblImages,
                                                   colFullFilePath,colSize,
                                                   imgToAdd.FileFullPath,imgToAdd.Size);

            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(insertstatement, dbConnection))
                {
                    int result = command.ExecuteNonQuery();
                    _log.InfoFormat("AddToDb {0}", result);
                }
            }
        }

        private string ConvertToString(object obj)
        {
            if (obj == null ||
                obj == DBNull.Value)
            {
                return string.Empty;
            }

            return obj.ToString();
        }
    }
}
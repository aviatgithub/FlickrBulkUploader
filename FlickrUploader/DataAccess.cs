using System;
using System.Collections.Generic;
using System.Data;
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

        public DataAccess(string dbfilename)
        {
            Init(dbfilename);
        }

        private void Init(string dbname)
        {
            _log.InfoFormat("Init with {0}", dbname);

            if (File.Exists(dbname) == false)
            {
                _log.Info("creating new");


                SQLiteConnection.CreateFile(dbname);

                var dbConnection = GetConnection();

                string sql = string.Format("CREATE TABLE {0} ({1} TEXT,{2} TEXT,{3} TEXT,{4} DATE,{5} DATE, {6} INT,{7} INT)",
                                           tblImages,
                                           colFullFilePath,//1
                                           colFlickrImgId,//2
                                           colFlickrPhotoSetId,//3
                                           colDateUploaded,//4
                                           colDateAddedToSet,//5
                                           colSecondsToUpload,//6
                                           colSecondsToAddToSet//7
                                           );

                var cmd = new SQLiteCommand(sql, dbConnection);
                int result = cmd.ExecuteNonQuery();

                _log.InfoFormat("create table command {0}", result);

                dbConnection.Close();

               

            }
        }

        private SQLiteConnection GetConnection()
        {
            var dbConnection =
                new SQLiteConnection("Data Source=test.sqlite;Version=3;");

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
                                                   "set {1} = {2} ," +
                                                   " {3} = {4} ," +
                                                   " {5} = {6} ," +
                                                   "where {7} = {8}",
                                                   tblImages,
                                                   colFlickrImgId, img.FlickrPhotoId,
                                                   colDateUploaded, img.DateUploaded,
                                                   colSecondsToUpload,img.SecondsToUpload,
                                                   colFullFilePath, img.FileFullPath);

            var dbConnection = GetConnection();
            var command = new SQLiteCommand(updateUpload, dbConnection);
            command.ExecuteNonQuery();

            dbConnection.Close();

        }

        public void SaveAddToSetStatus(AImg img)
        {
            if (img.IsAddToSetCompleted == false)
            {
                return;
            }

            string updateAddToSet = string.Format("update {0} " +
                                                  "set {1} = {2} ," +
                                                  " {3} = {4} ," +
                                                  " {5} = {6} ," +
                                                  "where {7} = {8}",
                                                  tblImages,
                                                  colFlickrPhotoSetId, img.FlickrPhotoSetId,
                                                  colDateAddedToSet, img.DateAddedToSet,
                                                  colSecondsToAddToSet, img.SecondsToAddToSet,
                                                  colFullFilePath, img.FileFullPath);

            var dbConnection = GetConnection();
            var command = new SQLiteCommand(updateAddToSet, dbConnection);
            command.ExecuteNonQuery();

            dbConnection.Close();
        }

        public List<AImg> GetAllStatus(string fileName)
        {
            var filesLister = new FilesLister();
            
            List<AImg> imagesInFilePath = filesLister.List(fileName);

            if (imagesInFilePath.Count == 0)
            {
                _log.Info("no files in this directory");
                
                return imagesInFilePath;
            }

            //update status from DB. 
            var conn = GetConnection();

            string sqlselect =string.Format( "select {0},{1},{2} from {3}",
                                             colFullFilePath,colFlickrPhotoSetId,colFlickrImgId, 
                                             tblImages);

            var adapter = new SQLiteDataAdapter(sqlselect, conn);
            conn.Open();
            var ds2 = new DataSet();
            adapter.Fill(ds2);
            conn.Close();

            var imgstable =  ds2.Tables[0];

            foreach (AImg aImg in imagesInFilePath)
            {
                var aimgwithingforeachloop = aImg;

                var q = from row in imgstable.AsEnumerable()
                        where row[colFullFilePath].ToString().ToLower() == aimgwithingforeachloop.FileFullPath.ToLower()
                        select new 
                            {
                                tFullFilePath = ConvertToString( row[colFullFilePath]),
                                tFlickrId =ConvertToString(row[colFlickrImgId]),
                                tFlickrPSId = ConvertToString(row[colFlickrPhotoSetId])
                            };

                if (q.Any())
                {
                    var matchingrow = q.First();
                    //ignore
                    aImg.IsUploaded = !string.IsNullOrWhiteSpace(matchingrow.tFlickrId);
                    aImg.IsAddToSetCompleted = !string.IsNullOrWhiteSpace(matchingrow.tFlickrPSId);
                    aImg.FlickrPhotoId = matchingrow.tFlickrId;

                }
                else
                {
                    //not present in db. not possible
                    AddToDb(aImg.FileFullPath);
                }
            }

            return imagesInFilePath;
        }

        private void AddToDb(string fullFilePath)
        {
            var dbConnection = GetConnection();

            string insertstatement = string.Format("insert into {0} ({1}) values ({2})",
                                                   tblImages,
                                                   colFullFilePath, fullFilePath);

            var command = new SQLiteCommand(insertstatement, dbConnection);
            command.ExecuteNonQuery();
        
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
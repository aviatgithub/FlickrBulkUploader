using System;
using System.Configuration;

namespace HashNash.FlickrUploader
{
    public class AppConfig
    {
        public AppConfig()
        {
            DebugMode = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugMode"]);
            FlickrDir = ConfigurationManager.AppSettings["flickrdir"];
            FolderToScan = ConfigurationManager.AppSettings["foldertoscan"];
            Apikey = ConfigurationManager.AppSettings["apikey"];
            Apisecret = ConfigurationManager.AppSettings["apisecret"];
            DbFilename = ConfigurationManager.AppSettings["dbfilename"];
            OAuthAccessToken = ConfigurationManager.AppSettings["OAuthAccessToken"];

        }

        public string FlickrDir { get; set; }
        public string FolderToScan { get; set; }
        public string Apikey { get; set; }
        public string Apisecret { get; set; }
        public string DbFilename { get; set; }
        public bool DebugMode { get; set; }
        public string OAuthAccessToken { get; set; }

        public void UpdateVerificationCode(string token)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["OAuthAccessToken"].Value = token;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
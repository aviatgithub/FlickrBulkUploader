using System.Configuration;
using System.IO;

namespace HashNash.FlickrUploader
{
   public  class PhotosetNamer
    {
       /// <summary>
       /// Check test cases.
       /// </summary>
       /// <param name="fullfilepath"></param>
       /// <returns></returns>
       public string GetPhotoSetName(string fullfilepath)
       {
           var config = new AppConfig();

           string initPath = config.FolderToScan.ToLower();

           string fullfilepathlower = fullfilepath.ToLower();

           var dirname = Path.GetDirectoryName(fullfilepathlower);

           int indexofInitPath = dirname.IndexOf(initPath);

           int endOfindexofInitPath = indexofInitPath + initPath.Length;

           int lengthofAllFolders = dirname.Length - endOfindexofInitPath;

           string folderafterinitpath = dirname.Substring(endOfindexofInitPath, lengthofAllFolders);

           string allfolderstrwithoutslash = folderafterinitpath.Trim(new[] { '\\' });

           if (string.IsNullOrEmpty(allfolderstrwithoutslash))
           {
               return "General";
           }
       
           return allfolderstrwithoutslash.Replace('\\', '_');
       }
    }
}

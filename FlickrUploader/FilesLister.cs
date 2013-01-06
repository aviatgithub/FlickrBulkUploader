using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HashNash.FlickrUploader
{
    public class FilesLister
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<AImg> List(string path)
        {
            log.InfoFormat("Fetching Files in this path :" + path);

            return Directory.EnumerateFiles(path, "*.jpg", SearchOption.AllDirectories).Select(file => new AImg(file)).ToList();
        }
    }
}

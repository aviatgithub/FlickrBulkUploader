using System.Collections.Generic;
using System.IO;

namespace HashNash.FlickrUploader
{
    public class FilesLister
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<AImg> List(string path)
        {
            var imgs = new List<AImg>();

            log.InfoFormat("Fetching Files in this path :" + path);

            foreach (string file in Directory.EnumerateFiles(
                path, "*.jpg", SearchOption.AllDirectories))
            {
                imgs.Add(new AImg(file));

            }

            return imgs;
        }
    }
}

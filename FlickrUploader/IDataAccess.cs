using System.Collections.Generic;

namespace HashNash.FlickrUploader
{
    public interface IDataAccess
    {
        List<AImg> GetAllStatus(string fileName);
        void SaveAddToSetStatus(AImg img);
        void SaveUploadStatus(AImg img);
    }
}

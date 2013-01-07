using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class FlickrProxy
    {

        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private  Flickr _flickrObj;
        private OAuthRequestToken _requestToken;
        private string _frob;
        
        private static Flickr CreateFlickr()
        {
            Flickr.CacheDisabled = true;
            var config = new AppConfig();

            var newflickr = new Flickr(
                apiKey: config.Apikey,
                sharedSecret: config.Apisecret,
                token: config.OAuthAccessToken); //token can be null.

            return newflickr;
        }

        public FlickrProxy()
        {
            _log.Info("ctor");
        }

        public Flickr FlickrObj
        {
            get { return _flickrObj; }
        }

        public string GetFrobAuthUrl()
        {
            _flickrObj = CreateFlickr();
            _frob  = _flickrObj.AuthGetFrob();
            return _flickrObj.AuthCalcUrl(_frob, AuthLevel.Write);
        }

       

        /// <summary>
        /// uses authtoken from config and tries to connect. if there is any error throw exception.
        /// </summary>
        public void Connect()
        {
            _flickrObj = CreateFlickr();
            Auth authorization = _flickrObj.AuthCheckToken(_flickrObj.AuthToken);
            string userId = authorization.User.UserId;
        }

        /// <summary>
        /// saves authtoken in config and calls Connect()
        /// </summary>
        public void SetUserVerified()
        {
            var config = new AppConfig();
            string authToken = _flickrObj.AuthGetToken(_frob).Token;
            _log.InfoFormat("code : {0}", authToken);
            config.UpdateVerificationCode(authToken);
            Connect(); //verify if the user actually connected.
        }
    }
}
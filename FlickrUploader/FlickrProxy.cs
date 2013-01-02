using System.Configuration;
using FlickrNet;
using log4net;

namespace HashNash.FlickrUploader
{
    public class FlickrProxy
    {

        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Flickr _flickrObj;
        private OAuthRequestToken _requestToken;


        public Flickr FlickrObj
        {
            get { return _flickrObj; }
        }

        public FlickrProxy()
        {
            _log.Info("ctor");

            string apikey = ConfigurationManager.AppSettings["apikey"];
            string apisecret = ConfigurationManager.AppSettings["apisecret"];

            _flickrObj = new Flickr(apiKey: apikey, sharedSecret: apisecret);
        }

        public string GetAuthUrl()
        {
            _requestToken = _flickrObj.OAuthGetRequestToken("http://google.com");
            return _flickrObj.OAuthCalculateAuthorizationUrl(_requestToken.Token, AuthLevel.Write);
        }

        public void Set(string verificationCode)
        {
            _log.InfoFormat("code : {0}", verificationCode);

            var accessToken = _flickrObj.OAuthGetAccessToken(_requestToken, verificationCode);

            _flickrObj.OAuthAccessToken = accessToken.Token;
            _flickrObj.OAuthAccessTokenSecret = accessToken.TokenSecret;
        }
    }
}
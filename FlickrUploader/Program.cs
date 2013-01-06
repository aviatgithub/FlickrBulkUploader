using System;
using log4net;

namespace HashNash.FlickrUploader
{
    class Program
    {
        private static void Main(string[] args)
        {
            // Set up a simple configuration that logs on the console.
            //BasicConfigurator.Configure();

            log4net.Config.XmlConfigurator.Configure();

            ILog log = LogManager.GetLogger("Main");

            try
            {
                new MainProgram().Do(args);

            }
            catch (Exception ex)
            {
                Console.WriteLine("###### Fatal Error #######");
                Console.WriteLine(ex);
                log.Error("fatal error", ex);
            }

            Console.WriteLine("Press any key to quit");

            Console.ReadLine(); //final
        }
    }
}

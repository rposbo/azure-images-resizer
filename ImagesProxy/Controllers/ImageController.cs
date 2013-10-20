using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace ImagesProxy.Controllers
{
    public class ImageController : ApiController
    {

        [HttpGet]
        public HttpResponseMessage Retrieve(int height, int width, string source)
        {
            try
            {
                var resizedFilename = BuildResizedFilenameFromParams(height, width, source);
                var imageBytes = GetFromCdn("resized", resizedFilename);
                return BuildImageResponse(imageBytes, "CDN", false);
            }
            catch (StorageException)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        var imageBytes = wc.DownloadData(
                            string.Format("{0}?height={1}&width={2}&source={3}",
                                          CloudConfigurationManager.GetSetting("Resizer_Endpoint"), height, width,
                                          source));

                        return BuildImageResponse(imageBytes, "Resizer", false);
                    }
                }
                catch (WebException)
                {
                    var imageBytes = GetFromCdn("origin", "404.jpg");
                    return BuildImageResponse(imageBytes, "CDN-Error", true);
                }
            }
        }

        private static string BuildResizedFilenameFromParams(int height, int width, string source)
        {
            return string.Format("{0}_{1}-{2}", height, width, source.Replace("/", string.Empty));
        }

        private static byte[] GetFromCdn(string path, string filename)
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.Storage.ConnectionString");
            var account = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = account.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(path);
            var blob = cloudBlobContainer.GetBlockBlobReference(filename);

            var m = new MemoryStream();
            blob.DownloadToStream(m);

            return m.ToArray();
        }

        private static HttpResponseMessage BuildImageResponse(byte[] imageBytes, string whereFrom, bool error)
        {
            var httpResponseMessage = new HttpResponseMessage { Content = new ByteArrayContent(imageBytes) };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            httpResponseMessage.Content.Headers.Add("WhereFrom", whereFrom);
            httpResponseMessage.StatusCode = error ? HttpStatusCode.NotFound : HttpStatusCode.OK;

            return httpResponseMessage;
        }
    }
}

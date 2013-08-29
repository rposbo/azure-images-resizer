using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ImageResizer;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace AzureImageResizer.Controllers
{
    public class ImageController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Resize(int width, int height, string source)
        {
            var imageBytes = GetFromCdn("origin", source);
            var newImageStream = ResizeImage(imageBytes, width, height);
            QueueNewImage(newImageStream, height, width, source);
            return BuildImageResponse(newImageStream);
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

        private static MemoryStream ResizeImage(byte[] downloaded, int width, int height)
        {
            var inputStream = new MemoryStream(downloaded);
            var memoryStream = new MemoryStream();

            var settings = string.Format("width={0}&height={1}", width, height);
            var i = new ImageJob(inputStream, memoryStream, new ResizeSettings(settings));
            i.Build();

            return memoryStream;
        }

        private static void QueueNewImage(MemoryStream memoryStream, int height, int width, string source)
        {
            var img = new ImageData
            {
                Name = source,
                Data = memoryStream.ToArray(), 
                Height = height,
                Width = width,
                Timestamp = DateTime.UtcNow
            };
            var message = new BrokeredMessage(img);
            QueueConnector.ImageQueueClient.BeginSend(message, SendComplete, img.FormattedName);
        }

        private static void SendComplete(IAsyncResult ar)
        {
            // Log the send thing
        }

        private static HttpResponseMessage BuildImageResponse(MemoryStream memoryStream)
        {
            var httpResponseMessage = new HttpResponseMessage { Content = new ByteArrayContent(memoryStream.ToArray()) };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            httpResponseMessage.StatusCode = HttpStatusCode.OK;

            return httpResponseMessage;
        }
    }
}

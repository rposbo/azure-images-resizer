using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using AzureImageResizer;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace UploadResized
{
    public class WorkerRole : RoleEntryPoint
    {
        const string QueueName = "azureimages";
        QueueClient _client;
        readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            _client.OnMessage(receivedMessage =>
            {
                try
                {
                    var receivedImage = receivedMessage.GetBody<ImageData>();
                    UploadBlob("resized", receivedImage);
                }
                catch (Exception e)
                {
                    // Handle any message processing specific exceptions here
                    Trace.WriteLine("Exception:" + e.Message);
                }
            }, new OnMessageOptions
            {
                AutoComplete = true,
                MaxConcurrentCalls = 1
            });

            _completedEvent.WaitOne();
        }

        public void UploadBlob(string path, ImageData image)
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.Storage.ConnectionString");
            var account = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = account.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(path);

            cloudBlobContainer.CreateIfNotExists();

            var blockref = image.FormattedName ?? Guid.NewGuid().ToString();
            var blob = cloudBlobContainer.GetBlockBlobReference(blockref);

            if (!blob.Exists())
                blob.UploadFromStream(new MemoryStream(image.Data));
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 2;

            // Create the queue if it does not exist already
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            _client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            _client.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace AzureImageResizer
{
    public static class QueueConnector
    {
        public static QueueClient ImageQueueClient;
        public const string QueueName = "azureimages";

        public static void Initialize()
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            var messagingFactory = MessagingFactory.Create(namespaceManager.Address, namespaceManager.Settings.TokenProvider);
            ImageQueueClient = messagingFactory.CreateQueueClient(QueueName);
        }
    }
}
using System.Linq;

namespace MsmqLib.Tests
{
    static internal class QueueTestHelper
    {
        public static void CreatePrivateQueue(string computerName, string testQueuePrefix, int queueSequenceNumber)
        {
            var queueName = testQueuePrefix + queueSequenceNumber;
            CreatePrivateQueue(computerName, queueName);
        }

        public static void CreatePrivateQueue(string computerName, string queueName)
        {
            var queuePath = CreateQueuePathForPrivateQueue(computerName, queueName);
            var queueService = new QueueService();
            queueService.CreateQueue(queuePath);
        }

        public static string CreateQueuePathForPrivateQueue(string computerName, string queueName)
        {
            return computerName + @"\private$\" + queueName;
        }

        public static void CleanupPrivateTestQueues(string computerName, string partialTestQueueName)
        {
            var queueService = new QueueService();
            var privateQueues = queueService.GetPrivateQueues(computerName);

            var testQueues = privateQueues
                .Where(q => q.QueueName.ToLower().Contains(@"private$\" + partialTestQueueName));

            foreach (var queue in testQueues)
            {
                queueService.DeleteQueue(queue.Path);
            }
        }
    }
}
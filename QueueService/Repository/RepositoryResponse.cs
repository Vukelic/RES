using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueueService.Service;

namespace QueueService.Repository
{
    public class RepositoryResponse : ServerResponse
    {
        string queueName;

        public RepositoryResponse(EResponseType responseType, string message, string queueName, string id) : base(id, responseType, message)
        {
            this.queueName = queueName;
        }

        public string QueueName { get => queueName; set => queueName = value; }
    }
}

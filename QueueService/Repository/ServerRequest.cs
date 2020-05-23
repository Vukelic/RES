using QueueService.Service;
using QueueService.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.Repository
{
    public class ServerRequest : ClientRequest
    {
        DataModel dataModel;
        string queueName;

        public ServerRequest(string id, DataModel dataModel, string queuename) : base(id, ERequestType.SERVER_UPDATE)
        {
            this.dataModel = dataModel;
            this.queueName = queuename;
        }

        public DataModel DataModel { get => dataModel; set => dataModel = value; }
        public string QueueName { get => queueName; set => queueName = value; }
    }
}

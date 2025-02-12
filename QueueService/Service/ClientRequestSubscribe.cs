﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.Service
{
    public class ClientRequestSubscribe : ClientRequest
    {
        private string queueName;
        private string jsonModel;

        public ClientRequestSubscribe(string queueName, string userId) : base(userId, ERequestType.SUBSCRIBE)
        {
            this.queueName = queueName;
        }

        public string QueueName { get => queueName; set => queueName = value; }
        public string JsonModel { get => jsonModel; set => jsonModel = value; }
    }
}

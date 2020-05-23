using QueueService.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueService.Repository
{
    public class RepositoryComponent
    {
        QueueModel repositoryQueues;
        UserDB dataBase;
        public RepositoryComponent()
        {
            dataBase = new UserDB("UsersDataBase");
            RepositoryQueues = new QueueModel("repositoryQueues");

            repositoryQueues = new QueueModel("repositoryQueues");

            Task serverRequestTask = new Task(() => AutomaticReadServerRequest());
            serverRequestTask.Start();
        }

        public QueueModel RepositoryQueues { get => repositoryQueues; set => repositoryQueues = value; }

        private async void AutomaticReadServerRequest()
        {
            while(true)
            {
                if(RepositoryQueues.QueueA.Count == 0)
                {
                    await Task.Delay(1000);
                }
                else
                {
                    var request = RepositoryQueues.QueueA.Dequeue();
                    if(request.Type == ERequestType.SERVER_UPDATE)
                    {
                        ServerRequest serverRequest = (ServerRequest)request;
                        if (dataBase.Clients.Any(x => x.Id == serverRequest.UserId))
                        {
                            var client = dataBase.Clients.First(x => x.Id == serverRequest.UserId);
                            if (client != null)
                            {
                                client.ModelData = serverRequest.DataModel;
                                dataBase.SaveChanges();
                                RepositoryResponse response = new RepositoryResponse(EResponseType.Ok, $"User{client.Id} Succesfully updated in database.", serverRequest.QueueName, serverRequest.UserId);
                                RepositoryQueues.QueueB.Enqueue(response);
                            }
                        }
                        else
                        {
                            RepositoryResponse response = new RepositoryResponse(EResponseType.Error, "User not in dabase.", serverRequest.QueueName, serverRequest.UserId);
                            RepositoryQueues.QueueB.Enqueue(response);
                        }
                    }
                    else
                    {
                        RepositoryResponse response = new RepositoryResponse(EResponseType.Error, "Request type not supported by repository.", "", "");
                        RepositoryQueues.QueueB.Enqueue(response);
                    }
                }
            }
        }
    }
}

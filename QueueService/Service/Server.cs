﻿using QueueService.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using QueueService.Repository;

namespace QueueService.Service
{
    public class Server
    {
        Dictionary<string, QueueModel> queues;
        object padLock = new object();
        QueueModel serverQueue;
        RepositoryComponent repository;

        public Server()
        {
            this.Queues = new Dictionary<string, QueueModel>();
            this.Queues.Add("clientQueue1", new QueueModel("clientQueue1"));
            ServerQueue = new QueueModel("serverQueue");

            repository = new RepositoryComponent();

            Task addUpdateTask = new Task(() => CreateSubscribeTask());
            addUpdateTask.Start();

            Task initialQueueTask = new Task(() => QueueTask(this.Queues["clientQueue1"]));
            initialQueueTask.Start();

            Task repositoryTask = new Task(() => AutomaticReadRepositoryResponses());
            repositoryTask.Start();
        }
        public Server(bool isTest)
        {
            if(isTest)
            {
                this.Queues = new Dictionary<string, QueueModel>();
                this.Queues.Add("testQueues", new QueueModel("testQueues"));
                ServerQueue = new QueueModel("serverQueue");
                
            }
        }

        public QueueModel ServerQueue { get => serverQueue; set => serverQueue = value; }

        public Dictionary<string, QueueModel> Queues { get => queues; set => queues = value; }

        public List<QueueModel> GetClientQueues()
        {
            return Queues.Values.ToList();
        }

        public async void CreateSubscribeTask()
        {
            while (true)
            {
                if(ServerQueue.QueueA.Count == 0)
                {
                    await Task.Delay(1000);
                    continue;
                }
                var request = ServerQueue.QueueA.Dequeue(); 
                if (request.Type == ERequestType.CREATE)
                {
                    ClientRequestCreate clientRequest = request as ClientRequestCreate;
                    if (Queues.ContainsKey(clientRequest.QueueName))
                    {
                        throw new Exception("A queue with that name alredy exsits");
                    }
                    else
                    {
                        QueueModel queueModel = new QueueModel(clientRequest.QueueName);
                        Queues.Add(clientRequest.QueueName, queueModel);
                        Task task = new Task(() => QueueTask(queueModel));
                        task.Start();

                        ServerResponseAS response = new ServerResponseAS(clientRequest.UserId, EResponseType.Ok, "Create succesfull");
                        response.Queues = queueModel;
                        ServerQueue.QueueB.Enqueue(response);
                        
                    }
                }
                else if (request.Type == ERequestType.SUBSCRIBE)
                {
                    ClientRequestSubscribe clientRequest = request as ClientRequestSubscribe;
                    if (Queues.ContainsKey(clientRequest.QueueName))
                    {
                        ServerResponseAS response = new ServerResponseAS(clientRequest.UserId, EResponseType.Ok, "Subscribe succesfull");
                        response.Queues = Queues[clientRequest.QueueName];
                        
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var dataModel = js.Deserialize<DataModel>(clientRequest.JsonModel);
                                                
                        ServerQueue.QueueB.Enqueue(response);
                    }
                    else
                    {
                        ServerResponse response = new ServerResponseAS(clientRequest.UserId, EResponseType.Error, "Subscribe unsuccesfull");
                        ServerQueue.QueueB.Enqueue(response);
                    }
                }
                else
                {
                    ServerResponse response = new ServerResponseAS(request.UserId, EResponseType.Error, "Request type unrecognized");
                    ServerQueue.QueueB.Enqueue(response);
                }

            }
        }

        public async void QueueTask(QueueModel model)
        {
            while(true)
            {
                if(model.QueueA.Count == 0)
                {
                    await Task.Delay(1000);
                }
                else
                {
                    var request = model.QueueA.Dequeue();
                    if (request.Type == ERequestType.UPDATE)
                    {
                        ClientRequestUpdate clientRequest = request as ClientRequestUpdate;
                        ServerRequest serverRequest = new ServerRequest(clientRequest.UserId, clientRequest.DataModel, model.QueueName);
                        repository.RepositoryQueues.QueueA.Enqueue(serverRequest);
                    }
                    else
                    {
                        ServerResponse response = new ServerResponseUpdate(request.UserId, EResponseType.Error, "Request type unrecognized");
                        model.QueueB.Enqueue(response);
                    }
                }
            }
        }

        public async void AutomaticReadRepositoryResponses()
        {
            while (true)
            {
                if (repository.RepositoryQueues.QueueB.Count == 0)
                {
                    await Task.Delay(1000);
                }
                else
                {
                    var response = repository.RepositoryQueues.QueueB.Dequeue();
                    if (response.Type == EResponseType.Ok)
                    {
                        RepositoryResponse repositoryResponse = (RepositoryResponse)response;
                        Console.WriteLine("[Server] Repository replied: " + repositoryResponse.Message);

                        if(queues.ContainsKey(repositoryResponse.QueueName))
                        {
                            var queueModel = queues[repositoryResponse.QueueName];

                            ServerResponse serverResponse = new ServerResponseUpdate(repositoryResponse.UserId, EResponseType.Ok, "User updated state.");
                            queueModel.QueueB.Enqueue(serverResponse);
                        }
                    }
                    else if (response.Type == EResponseType.Error)
                    {
                        RepositoryResponse repositoryResponse = (RepositoryResponse)response;
                        Console.WriteLine("[Server] Repository replied: " + repositoryResponse.Message);

                        if (queues.ContainsKey(repositoryResponse.QueueName))
                        {
                            var queueModel = queues[repositoryResponse.QueueName];

                            ServerResponse serverResponse = new ServerResponseUpdate(repositoryResponse.UserId, EResponseType.Error, "User couldnt updated state.");
                            queueModel.QueueB.Enqueue(serverResponse);
                        }
                    }
                }
            }
        }
            
    }
}

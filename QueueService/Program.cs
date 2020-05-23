using QueueService.Service;
using QueueService.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QueueService
{
    public class Program
    {
        static List<Client> clientList = new List<Client>();

        static Server server;
        public static void Main(string[] args)
        {
            clientList = new List<Client>();
            server = new Server();
           
            Client client1 = new Client(server);
            client1.ModelData.Items.Add(new Item("item1"));
            client1.ModelData.Items.Add(new Item("item2"));
            client1.ModelData.Items.Add(new Item("item3"));

            client1.ModelData.Positions.Add(new Position(11, 29, 33));
            client1.ModelData.Positions.Add(new Position(44, 51, 64));
            client1.ModelData.Positions.Add(new Position(71, 84, 96));

            client1.Server = server;

            client1.Queues = new QueueModel("afsdf");
            
            UserDB userdb = new UserDB("UsersDataBase");

            if (userdb.Clients.Count() == 0)
            {
                userdb.Clients.Add(client1);
                userdb.SaveChanges();
            }

            clientList = userdb.Clients.ToList();
            clientList.ForEach(x => x.Server = server);



            while (true)
            {
               // Console.Clear();
                Console.WriteLine("Choose option: ");
                Console.WriteLine("0. Exit.");
                Console.WriteLine("1. Choose a client from a list.");
                Console.WriteLine("2. Make new client.");
                
                int option = 0;
                Client client = null;
                if (!Int32.TryParse(Console.ReadLine(), out option))
                {
                    //Console.Clear();
                    Console.WriteLine("Error while processing option. Press any key to go back.");
                    Console.ReadKey();
                    continue;
                }
                if (option == 1)
                {
                    Console.WriteLine("Choose client: ");
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. Client: {clientList[i].Id}");
                    }
                    int index = Int32.Parse(Console.ReadLine());
                    client = clientList[index - 1];
                }
                else if (option == 2)
                {
                    string id;
                    string items;
                    string positions;
                    Console.WriteLine("Input client id: ");
                    id = Console.ReadLine();
                    Console.WriteLine("Input items separated with semicolons ('name','quantity','isActive','destructivePower';...)");
                    items = Console.ReadLine();
                    Console.WriteLine("Input positions separated with semicolon ('x','y','z';...)");
                    positions = Console.ReadLine();
                    DataModel dataModel = new DataModel();
                    var itemsList = items.Split(';');
                    var positionList = positions.Split(';');
                    dataModel.Id = id + "_model_" + Guid.NewGuid().ToString();
                    foreach (string item in itemsList)
                    {
                        var itemParts = item.Split(',');
                        Item newItem = new Item(itemParts[0], double.Parse(itemParts[1]), bool.Parse(itemParts[2]), double.Parse(itemParts[3]));
                        dataModel.Items.Add(newItem);
                    }
                    foreach (string position in positionList)
                    {
                        var posParts = position.Split(',');
                        Position newPosition = new Position(double.Parse(posParts[0]), double.Parse(posParts[1]), double.Parse(posParts[2]));
                        dataModel.Positions.Add(newPosition);
                    }
                    client = new Client(dataModel, id, server);
                    if(!userdb.Clients.Any(x => x.Id == client.Id))
                    {
                        try
                        {
                            userdb.Clients.Add(client);
                            userdb.SaveChanges();
                        }
                        catch { }
                    }
                    clientList.Add(client);

                }
                else if (option == 0)
                {
                    Console.WriteLine("Exit");
                    break;
                }
                else
                {
                    Console.WriteLine("You entered wrong choise. Press any key to go back.");
                    Console.ReadKey();
                }
                ClientMenu(client);
            }
        }

        private static void ClientMenu(Client client)
        {
            while (true)
            {
                //Console.Clear();
                Console.WriteLine("Choose option.");
                Console.WriteLine("0. Go back.");
                Console.WriteLine("1. Subscribe to existing queues.");
                Console.WriteLine("2. Make new pair of queues.");
                Console.WriteLine("3. Update state.");
                int i = Int32.Parse(Console.ReadLine());
                if (i == 0)
                {
                    return;
                }
                else if(i == 1)
                {
                    Console.Clear();
                    client.Subscribe();
                }
                else if(i == 2)
                {
                    Console.Clear();
                    client.Create();
                }
                else if(i == 3)
                {
                    Console.Clear();
                    client.Update();
                }
                else
                {
                    Console.WriteLine("You entered wrong choise. Press any key to go back");
                    Console.ReadKey();
                }
            }
        }
    }
}

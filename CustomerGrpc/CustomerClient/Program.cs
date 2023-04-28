﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using CustomerGrpc;
using Grpc.Core;
using Grpc.Net.Client;

namespace CustomerClient
{
	class Program
	{
		static async Task Main(string[] args)
		{
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

			if (args.Length > 2)
			{
                if (args[0].Equals("localhost") && args[1].Equals("5001"))
				{
                    var customer = new Customer
                    {
                        ColorInConsole = GetRandomChatColor(),
                        Id = Guid.NewGuid().ToString(),
                        Name = args[2]
                    };

                    var channel = GrpcChannel.ForAddress("http://localhost:5001", new GrpcChannelOptions { Credentials = ChannelCredentials.Insecure });
                    var client = new CustomerService.CustomerServiceClient(channel);
                    var joinCustomerReply = await client.JoinCustomerChatAsync(new JoinCustomerRequest
                    {
                        Customer = customer
                    });
                    Console.WriteLine(joinCustomerReply.RoomId);
					if (joinCustomerReply.RoomId !=  -1)
					{
                        using (var streaming = client.SendMessageToChatRoom(new Metadata { new Metadata.Entry("CustomerName", customer.Name) }))
                        {
                            var response = Task.Run(async () =>
                            {
                                while (await streaming.ResponseStream.MoveNext())
                                {
                                    Console.ForegroundColor = Enum.Parse<ConsoleColor>(streaming.ResponseStream.Current.Color);
                                    Console.WriteLine($"{streaming.ResponseStream.Current.CustomerName}: {streaming.ResponseStream.Current.Message}");
                                    Console.ForegroundColor = Enum.Parse<ConsoleColor>(customer.ColorInConsole);
                                }
                            });

                            await streaming.RequestStream.WriteAsync(new ChatMessage
                            {
                                CustomerId = customer.Id,
                                Color = customer.ColorInConsole,
                                Message = "",
                                RoomId = joinCustomerReply.RoomId,
                                CustomerName = customer.Name,
                            });
                            Console.ForegroundColor = Enum.Parse<ConsoleColor>(customer.ColorInConsole);
                            Console.WriteLine($"Joined the chat as {customer.Name}");
                            string maquina_destino = "";
                            string mensaje = "";
                            int pos_espacio = 0;
                            var line = Console.ReadLine();
                            DeletePrevConsoleLine();
                            while (!string.Equals(line.ToLower(), "quit", StringComparison.OrdinalIgnoreCase))
                            {
                                if (line.IndexOf("send",0,5) != -1)
                                {
                                    pos_espacio = line.IndexOf(" ", 5); // segundo espacio
                                    maquina_destino = line.Substring(4, pos_espacio - 4).Trim();
                                    mensaje = line.Substring(pos_espacio + 1);
                                    if (maquina_destino != args[2])
                                    {
                                        await streaming.RequestStream.WriteAsync(new ChatMessage
                                        {
                                            Color = customer.ColorInConsole,
                                            CustomerId = customer.Id,
                                            CustomerName = customer.Name,
                                            Message = mensaje,
                                            RoomId = joinCustomerReply.RoomId,
                                            CustomerDest = maquina_destino //Aqui debe ir el name del otro cliente
                                        });
                                    }
                                    else
                                    {
                                        Console.WriteLine("No puede enviar mensaje a si mismo");
                                    }
                                    line = Console.ReadLine();
                                    DeletePrevConsoleLine();
                                }
                                else
                                {
                                    Console.WriteLine("===========");
                                    line = Console.ReadLine();
                                    DeletePrevConsoleLine();
                                }
                            }
                            await streaming.RequestStream.WriteAsync(new ChatMessage
                            {
                                Color = customer.ColorInConsole,
                                CustomerId = customer.Id,
                                CustomerName = customer.Name,
                                Message = line,
                                RoomId = joinCustomerReply.RoomId,
                                CustomerDest = ""
                            });
                            //line = Console.ReadLine();
                            //DeletePrevConsoleLine();
                            await streaming.RequestStream.CompleteAsync();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Id invalido, ya existe");
                    }

                }
                else
                {
                    Console.WriteLine("Debe de mandar como parámetros: [ipserver port id_maquina]");
                }

            }
			Console.WriteLine("Has salido! Presione cualquier tecla para salir del programa");
			Console.ReadKey();
		}

		private static string GetRandomChatColor()
		{
			var colors = Enum.GetValues(typeof(ConsoleColor));
			var rnd = new Random();
			return colors.GetValue(rnd.Next(1, colors.Length - 1)).ToString();
		}

		private static void DeletePrevConsoleLine()
		{
			if (Console.CursorTop == 0) return;
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, Console.CursorTop - 1);
		}
	}
}
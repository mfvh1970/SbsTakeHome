using Core;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Text.Json;

namespace Server
{
    class Program
    {
        /// <summary>
        /// Server
        /// </summary>
        /// <param name="args">No arguments required</param>
        static void Main(string[] args)
        {
            var connection = "tcp://localhost:5555";

            try
            {
                using (var poller = new NetMQPoller())
                {
                    var socket = new ResponseSocket();

                    socket.ReceiveReady += Socket_ReceiveReady;
                    poller.Add(socket);
                    poller.RunAsync();

                    socket.Bind(connection);

                    Console.WriteLine("Server initialized.");
                    Console.WriteLine("Press <Enter> key to stop server ...");
                    Console.WriteLine("--------------------------------------");
                    Console.ReadLine();
                }
            }
            catch
            {
                Console.WriteLine("An unhandled error has ocurred. Program execution will be aborted.");
                Console.WriteLine("Program execution will be aborted ...");

                return;
            }
        }

        /// <summary>
        /// Handles socket 'ReceiveReady' event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private static void Socket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            try
            {
                var msg = e.Socket.ReceiveFrameString();

                if (string.IsNullOrEmpty(msg))
                {
                    e.Socket.SendFrame("Error: No data was received for this row.");
                }
                else
                {
                    //deserializes the object received
                    var row = JsonSerializer.Deserialize<Row>(msg);

                    Console.WriteLine($"Received: {row.FormatToString()}");
                    e.Socket.SendFrame("Received"); //response to client
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"An error ocurred during object deserialization. Error message was: {ex.Message}");
            }
            catch (NetMQException ex)
            {
                Console.WriteLine($"An error on the queuing system has ocurred while reading data from queue. Error message was: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has ocurred while reading data from queue. Error message was: {ex.Message}");
            }
        }
    }
}

using Core;
using CsvHelper;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Client
{
    class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Full path to file to be process location needs to be passed as argument</param>
        static void Main(string[] args)
        {
            try
            {
                //validate arguments
                if (args.Length == 0)
                {
                    Console.WriteLine("Fatal Error: Full path to file location needs to be passed as a parameter to the program.");
                    throw new ArgumentNullException();
                }

                var filePath = args[0];
                var connection = "tcp://localhost:5555";
                var rows = ReadFile(filePath);

                //file contains no rows
                if (!rows.Any())
                {
                    Console.WriteLine("File contains 0 rows. No processing required.");
                    throw new InvalidDataException();
                }

                using (var socket = new RequestSocket())
                {
                    socket.Connect(connection);

                    Console.WriteLine($"Rows: {rows.Count()}");

                    foreach (var row in rows)
                    {
                        //serializes the object to send to queue
                        var msg = JsonSerializer.Serialize(row);

                        socket.SendFrame(msg);
                        Console.WriteLine($"Sent to server: {msg}");

                        if (socket.TryReceiveFrameString(new TimeSpan(0, 0, 3), out msg)) //waits 3 seconds for confirmation from srvr
                            Console.WriteLine($"Server response: {msg}");
                        else
                            Console.WriteLine($"Server failed to notify data reception.");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine("File processing has been completed.");
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();

                return;
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();

                return;
            }
            catch (CsvHelperException ex)
            {
                Console.WriteLine($"An error while trying to process the CSV data has ocurred. Error message was: {ex.Message}");
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();

                return;
            }
            catch (IOException)
            {
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();

                return;
            }
            catch (NetMQException ex)
            {
                Console.WriteLine($"An error within the queuing system has ocurred. Error Message was: {ex.Message}");
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();

                return;
            }
            catch
            {
                Console.WriteLine("An unhandled exception/error has ocurred.");
                Console.WriteLine("Press <Enter> to terminate execution ...");
                Console.ReadLine();

                return;
            }
        }

        /// <summary>
        /// Reads from file and returns POCO objects list
        /// </summary>
        /// <param name="filePath">The full file path to the input file</param>
        /// <returns></returns>
        static List<Row> ReadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException();

                var reader = new StreamReader(filePath);
                var csvReader = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

                return csvReader.GetRecords<Row>().ToList();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"There was an error while trying to process input file. Error message was: {ex.Message}");
                throw;
            }
            catch
            {
                throw;
            }
        }
    }
}

using Fleck;
using System.Text.Json;
using System.Runtime.Versioning;
using System.Text;
using rF2SharedMemoryNet.RF2Data.Structs;
using Microsoft.Extensions.FileProviders;

namespace LMUTelemetryTest
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {
            object stateLock = new();
            MyMemoryReader? reader = null;
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            bool gameConnected = false;
            int prevLap = -1;
            List<string> lapData = new();
            bool sessionCreated = false;
            string FilePath = null;
            string sessionaDirectory = "sessions";
            Directory.CreateDirectory(sessionaDirectory);

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            var app = builder.Build();
            app.UseCors();

            app.MapGet("/api/sessions", () =>
            {
               string sessionDirectory = "sessions";
               if (!Directory.Exists(sessionDirectory))
               {
                   return Results.Ok("Array.Empty<object>()");
               } 

               var files = Directory.GetFiles(sessionaDirectory, "*.txt").Select(file => new
               {
                   Name = Path.GetFileName(file),
                   path = file
               }).ToList();

               return Results.Ok(files);
            });

            app.RunAsync("http://localhost:5000");


            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        lock (stateLock)
                        {
                            if (!gameConnected)
                            {
                                reader = new MyMemoryReader();
                                gameConnected = true;
                                Console.WriteLine("[✓] Game detected - LMU plugin connected!");
                            }
                        }
                    }
                    catch
                    {
                        lock (stateLock)
                        {
                            if (gameConnected)
                            {
                                Console.WriteLine("[✗] Game disconnected or plugin lost.");
                                gameConnected = false;
                                reader?.Close();
                                reader = null;
                            }
                            else
                            { 
                                Console.WriteLine("[⟳] Waiting for game to start and LMU plugin to be installed...");
                            }
                        }
                    }

                    await Task.Delay(2000); // Check every 2 seconds
                }
            });

            server.Start(connection =>
            {
                bool connectionActive = true;

                connection.OnOpen = () =>
                    // Console.WriteLine($"OnOpen {connection.ConnectionInfo.Id}");

                connection.OnClose = () =>
                {
                    connectionActive = false;  // Signal loop to stop
                    
                    // Console.WriteLine($"OnClose {connection.ConnectionInfo.Id}");
                };

                connection.OnMessage = message =>
                    Console.WriteLine($"OnMessage:{message}");

                Task.Run(async () =>
                {
                    // Console.WriteLine($"Client connected: {connection.ConnectionInfo.Id}");
                    while (connectionActive)  // Stop when connection closes
                    {
                        try
                        {
                            VehicleTelemetry? data;

                            lock (stateLock)
                            {
                                if (!gameConnected || reader == null)
                                {
                                    data = null;
                                }
                                else
                                {
                                    data = reader.GetPlayerTelemetry();
                                }
                            }

                            if (!data.HasValue)
                            {
                                Console.WriteLine("No telemetry yet...");
                                await Task.Delay(16);
                                continue;
                            }

                            var telemetry = data.Value;
                            var v = telemetry.LocalVelocity;

                            double speed = Math.Sqrt(
                                v.X * v.X +
                                v.Y * v.Y +
                                v.Z * v.Z
                            );

                            double speedKmh = Math.Round(speed * 3.6, 2);
                            double throttle = Math.Round(telemetry.UnfilteredThrottle, 2) * 100;
                            double brake = Math.Round(telemetry.UnfilteredBrake, 2) * 100;
                            string trackName = Encoding.UTF8.GetString(telemetry.TrackName).TrimEnd('\0');
                            double elapsedTime = telemetry.ElapsedTime;
                            int lapNumber = telemetry.LapNumber;

                            if (!sessionCreated)
                                {
                                    Directory.CreateDirectory(sessionaDirectory);
                                    FilePath = trackName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                                    FilePath = Path.Combine(sessionaDirectory, FilePath);
                                    File.Create(FilePath).Close();
                                    Console.WriteLine($"Session file created: {FilePath}");
                                    sessionCreated = true;
                                }

                            string json = JsonSerializer.Serialize(new
                            {
                                speed = speedKmh,
                                throttle = throttle,
                                brake = brake,
                                elapsedTime = elapsedTime,
                                lap = lapNumber + 1
                            });

                            lapData.Add(json);

                            if (prevLap != -1 && lapNumber != prevLap)
                            {
                                Console.WriteLine("new lap started");
                                await File.AppendAllLinesAsync(FilePath, lapData);
                                lapData.Clear();
                            }
                            prevLap = lapNumber;

                            Console.Clear();
                            Console.WriteLine($"Speed: {speedKmh}");
                            Console.WriteLine($"Throttle Input: {throttle}");
                            Console.WriteLine($"Brake Input: {brake}");
                        }
                        catch (Exception e)
                        {
                            if (gameConnected)
                            {
                                Console.WriteLine($"Telemetry error: {e.Message}");
                            }
                            await Task.Delay(100); // Brief delay before retry
                        }

                        await Task.Delay(16);
                    }
                });
            });
            Console.ReadLine();
        }
    }
}
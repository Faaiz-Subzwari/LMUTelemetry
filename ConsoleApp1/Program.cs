using Fleck;
using System.Text.Json;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;

namespace LMUTelemetryTest
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new MyMemoryReader();
            var server = new  WebSocketServer("ws://0.0.0.0:8181");
            
            server.Start(connection =>
            {
                connection.OnOpen = () => 
                    Console.WriteLine("OnOpen");
                connection.OnClose = () =>
                    Console.WriteLine("OnClose");
                connection.OnMessage = message =>
                    Console.WriteLine($"OnMessage:{message}");

                Task.Run(async () =>
                {
                    while (true)
                    {
                        var data = reader.GetPlayerTelemetry();

                        if (data.HasValue)
                        {
                            var telemetry = data.Value;
                            var v = telemetry.LocalVelocity;

                            double speed = Math.Sqrt(
                                v.X * v.X +
                                v.Y * v.Y +
                                v.Z * v.Z
                            );

                            double speedKmh = Math.Round(speed * 3.6, 2);
                            double throttle = Math.Round(data?.UnfilteredThrottle ?? 0, 2) * 100;
                            double brake = Math.Round(data?.UnfilteredBrake ?? 0, 2) * 100;

                            string json = JsonSerializer.Serialize(new
                            {
                                speed = speedKmh,
                                throttle = throttle,
                                brake = brake
                            });

                            await connection.Send(json);

                            // Console.Clear();

                            Console.WriteLine($"Speed: {speedKmh}");
                            Console.WriteLine($"Throttle Input: {throttle}");
                            Console.WriteLine($"Throttle Input: {brake}");
                            await Task.Delay(1000);
                        }
                        else
                        {
                            Console.WriteLine("No telemetry yet...");
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                });
            });    
            Console.ReadLine();   
        }
    }
}
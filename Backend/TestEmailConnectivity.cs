using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace fundoo_notes.Tests
{
    /// <summary>
    /// Test class to check email connectivity
    /// </summary>
    public static class EmailConnectivityTest
    {
        public static async Task TestSmtpConnectivity()
        {
            Console.WriteLine("Testing SMTP connectivity...");
            
            // Test 1: Ping Gmail's SMTP server
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("smtp.gmail.com", 5000);
                Console.WriteLine($"Ping to smtp.gmail.com: {reply.Status}");
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"Round trip time: {reply.RoundtripTime}ms");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ping failed: {ex.Message}");
            }

            // Test 2: Try to connect to SMTP port
            try
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync("smtp.gmail.com", 587);
                var timeoutTask = Task.Delay(10000); // 10 second timeout
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == connectTask && tcpClient.Connected)
                {
                    Console.WriteLine(" Successfully connected to smtp.gmail.com:587");
                    tcpClient.Close();
                }
                else
                {
                    Console.WriteLine("Failed to connect to smtp.gmail.com:587 (timeout or connection refused)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP connection failed: {ex.Message}");
            }

            // Test 3: Try alternative ports
            var alternativePorts = new[] { 465, 25 };
            foreach (var port in alternativePorts)
            {
                try
                {
                    using var tcpClient = new TcpClient();
                    var connectTask = tcpClient.ConnectAsync("smtp.gmail.com", port);
                    var timeoutTask = Task.Delay(5000);
                    
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
                    if (completedTask == connectTask && tcpClient.Connected)
                    {
                        Console.WriteLine($" Alternative port {port} is accessible");
                        tcpClient.Close();
                    }
                    else
                    {
                        Console.WriteLine($"Alternative port {port} is not accessible");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Port {port} connection failed: {ex.Message}");
                }
            }
        }

        public static void Main(string[] args)
        {
            TestSmtpConnectivity().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

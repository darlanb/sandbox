using Microsoft.AspNetCore.SignalR.Client;
using SampleWebApp.Hubs;
using SampleWebApp.Model;
using System;
using System.Timers;

namespace SampleWebApp.Simulation
{
    public class NotificationGenerator
    {
        private readonly Random _random;
        private readonly Timer _sendEventTimer;
        private readonly HubConnection _connection;


        // https://msdn.microsoft.com/en-us/library/hh300224(v=vs.110).aspx
        // https://docs.microsoft.com/en-us/aspnet/core/signalr/get-started-signalr-core
        // https://docs.microsoft.com/en-us/aspnet/signalr/overview/security/hub-authorization
        public NotificationGenerator()
        {
            _random = new Random();

            _connection = new HubConnectionBuilder()
                          .WithUrl("http://localhost:5000/notification")
                          .WithConsoleLogger()
                          .Build();
            
            var startResult = _connection.StartAsync();
            startResult.Wait();

            _sendEventTimer = new Timer(500);
            _sendEventTimer.Elapsed += GenerateNotifications;
            _sendEventTimer.AutoReset = true;
            _sendEventTimer.Start();
        }

        private void GenerateNotifications(object sender, ElapsedEventArgs e)
        {
            var severity = _random.Next(1, 5);

            var notification = new NotificationMessage()
            {
                Severity = severity,
                Message = $"New severity {severity} notification. Timestamp: {DateTime.Now.ToString("o")}"
            };

            var invokeResult = _connection.InvokeAsync("SendNotification", notification);
            invokeResult.Wait();
        }
    }
}

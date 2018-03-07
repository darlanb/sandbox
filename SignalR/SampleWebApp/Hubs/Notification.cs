using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SampleWebApp.Model;
using SampleWebApp.Simulation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SampleWebApp.Hubs
{
    public class Notification : Hub
    {
        private readonly ILogger<Notification> _logger;
        private readonly ConcurrentDictionary<string, int[]> _subscriptions;

        public Notification(ILogger<Notification> logger)
        {
            _logger = logger;
            _subscriptions = new ConcurrentDictionary<string, int[]>();
        }

        public Task Subscribe(string severities)
        {
            var severitiesArray = severities.Split(',');
            var severitiesList = new List<int>();

            foreach(var severity in severitiesArray)
            {
                severitiesList.Add(int.Parse(severity));
            }
            
            _subscriptions.AddOrUpdate(Context.ConnectionId, severitiesList.ToArray(), (connId, oldValue) => severitiesList.ToArray());

            _logger.LogInformation("New subscription: clientId({0}), severities({1})", Context.ConnectionId, severities);

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogDebug("Client disconnected. clientId({0}), exception({1}).", Context.ConnectionId, exception?.Message);

            var removeResult = _subscriptions.TryRemove(Context.ConnectionId, out var severities);

            _logger.LogInformation("Removed subscription. clientId({0}), removeResult({1})", Context.ConnectionId, removeResult);

            return base.OnDisconnectedAsync(exception);
        }

        public Task SendNotification(NotificationMessage message)
        {
            _logger.LogDebug("New notification received: severity({0}), message({1}).", message.Severity, message.Message);

            Parallel.ForEach(_subscriptions.Keys, (key) =>
            {
                var getResult = _subscriptions.TryGetValue(key, out var severities);

                if (getResult && severities.Contains(message.Severity))
                {
                    Clients.Client(key).SendAsync("SendNotification", message);

                    _logger.LogDebug("Notification sent to clientId({0}). severity({1}), message({2})",
                            key, message.Severity, message.Message);
                }
            });

            return Task.CompletedTask;
        }
    }
}

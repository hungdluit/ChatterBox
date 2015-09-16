using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using ChatterBox.Shared.Communication.Messages.Registration;
using Common.Logging;

namespace ChatterBox.Server
{
    public class Domain
    {
        private ILog Logger => LogManager.GetLogger(string.IsNullOrWhiteSpace(Name) ? nameof(Domain) : $"Domain[{Name}]");
        public string Name { get; set; }

        public ConcurrentDictionary<string, RegisteredClient> Clients { get; } = new ConcurrentDictionary<string, RegisteredClient>();


        public bool HandleRegistration(UnregisteredConnection unregisteredConnection, RegistrationMessage message)
        {
            Logger.Info($"Handling the registration of connection {unregisteredConnection}");
            RegisteredClient registeredClient;
            if (Clients.ContainsKey(message.UserId))
            {
                if (Clients.TryGetValue(message.UserId, out registeredClient))
                {
                    Logger.Debug($"Client identified. {registeredClient}");
                }
                else
                {
                    Logger.Warn("Error in retrieving client.");
                    return false;
                }
            }
            else
            {
                registeredClient = new RegisteredClient
                {
                    UserId = message.UserId,
                    Domain = Name,
                    Name = message.Name,
                    PushToken = message.PushToken,
                };
                if (Clients.TryAdd(registeredClient.UserId, registeredClient))
                {
                    Logger.Info($"Registered new client. {registeredClient}");
                }
                else
                {
                    Logger.Warn("Could not register new client.");
                    return false;
                }
            }

            registeredClient.SetActiveConnection(unregisteredConnection, message);
            return true;
        }

        public void OnHeartBeat()
        {
            var clients = Clients.Select(s => s.Value).ToList();
            foreach (var client in clients)
            {
                client.ServerHeartBeat();
            }

        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ChatterBox.Common.Communication.Messages.Interfaces;
using ChatterBox.Common.Communication.Messages.Peers;
using ChatterBox.Common.Communication.Messages.Registration;
using ChatterBox.Common.Communication.Shared.Messages.Relay;
using Common.Logging;

namespace ChatterBox.Server
{
    public class Domain
    {
        public ConcurrentDictionary<string, RegisteredClient> Clients { get; } =
            new ConcurrentDictionary<string, RegisteredClient>();

        private ILog Logger
            => LogManager.GetLogger(string.IsNullOrWhiteSpace(Name) ? nameof(Domain) : $"Domain[{Name}]");

        public string Name { get; set; }

        private PeerInformation GetClientInformation(RegisteredClient registeredClient)
        {
            return new PeerInformation
            {
                UserId = registeredClient.UserId,
                Name = registeredClient.Name,
                IsOnline = registeredClient.IsOnline,
                SentDateTimeUtc = DateTime.UtcNow
            };
        }

        private List<RegisteredClient> GetPeers(RegisteredClient sender)
        {
            return Clients.Where(s => s.Key != sender.UserId).Select(s => s.Value).ToList();
        }

        public bool HandleRegistration(UnregisteredConnection unregisteredConnection, Registration message)
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
                    PushToken = message.PushToken
                };
                registeredClient.OnConnected += RegisteredClient_OnConnected;
                registeredClient.OnDisconnected += RegisteredClient_OnDisconnected;
                registeredClient.OnGetPeerList += RegisteredClient_OnGetPeerList;
                registeredClient.OnRelayMessage += RegisteredClient_OnRelayMessage;

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

        private void RegisteredClient_OnConnected(RegisteredClient sender)
        {
            var peers = GetPeers(sender);
            foreach (var registeredClient in peers)
            {
                registeredClient.OnPeerPresence(GetClientInformation(sender));
            }
        }

        private void RegisteredClient_OnDisconnected(RegisteredClient sender)
        {
            var peers = GetPeers(sender);
            foreach (var registeredClient in peers)
            {
                registeredClient.OnPeerPresence(GetClientInformation(sender));
            }
        }

        private void RegisteredClient_OnGetPeerList(RegisteredClient sender, IMessage message)
        {
            sender.OnPeerList(new PeerList
            {
                ReplyFor = message.Id,
                Peers = GetPeers(sender).Select(GetClientInformation).ToArray()
            });
        }

        private void RegisteredClient_OnRelayMessage(RegisteredClient sender, RelayMessage message)
        {
            RegisteredClient receiver;
            if (!Clients.TryGetValue(message.ToUserId, out receiver))
            {
                return;
            }
            message.FromUserId = sender.UserId;
            receiver.ServerRelay(message);
        }
    }
}
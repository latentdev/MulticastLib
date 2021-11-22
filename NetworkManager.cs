using Microsoft.Extensions.Logging;
using MulticastLib.Interfaces;
using MulticastLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLib
{
    /// <summary>
    /// NetworkManager is a class used for controlling when a message is sent to a multicast group and 
    /// notifies subscribers when a message has been received from a multicast group.
    /// </summary>
    public class NetworkManager
    {
        private ILogger<NetworkManager> _logger;
        private MulticastEndpoint _multicastEndpoint = null;
        private IPAddress _localIP;
        private IPAddress _multicastGroupIP;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Bool indicating if the Network Manager is currently running the listener
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Event fired every time a message is received at the multicast endpoint.
        /// </summary>
        /// <returns>Message containing the message contents</returns>
        public event EventHandler<IMessage> MessageReceived;

        /// <summary>
        /// Network Manager Constructor
        /// </summary>
        /// <param name="multicastEndpoint">The multicast enpoint to manage</param>
        /// <param name="logger">Logger for this class</param>
        public NetworkManager(ILogger<NetworkManager> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Starts the Network Manager Listener. This listener spins in the background waiting a message to be broadcasted to the multicast group.
        /// </summary>
        /// <param name="localIP">IPv4 address of the local adapter that will bind to the multicast group IP</param>
        /// <param name="multicastGroupIP">The IP address of the multicast group to join</param>
        /// <param name="port">The port on which to listen</param>
        public void StartListener(IPAddress localIP, IPAddress multicastGroupIP, int port = 6100)
        {
            _localIP = localIP;
            _logger?.LogInformation("Creating multicast endpoint");
            _multicastEndpoint = MulticastEndpoint.Create(localIP, multicastGroupIP, port); //create a socket and join the multicast group
            _logger?.LogInformation($"Listening on {multicastGroupIP}:{port}");
            _cancellationTokenSource = new CancellationTokenSource();
            var cancelationToken = _cancellationTokenSource.Token;
            Task.Run(async () => // start background listening task
            {
                while (!cancelationToken.IsCancellationRequested)
                {
                    await ListenForMessage();
                }
            });
            IsListening = true;
        }
        /// <summary>
        /// Sends a message using the multicast endpoint
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns></returns>
        public async Task SendMessage(Message message)
        {
            await _multicastEndpoint.Send(message.ToBytes());
        }

        /// <summary>
        /// Waits for message to be received and fires an event containing message for any subscribers
        /// </summary>
        private async Task ListenForMessage()
        {
            try
            {
                var response = await _multicastEndpoint.ReceiveAsync(); // wait till a message is received
                var responseMessage = Message.GetMessage(response); // decrypt message from byte array to an object
                _logger?.LogInformation($"Local IP: {_localIP}\nMulticast Group: {_multicastGroupIP}\nReceived {response.Length} byte message from {responseMessage.IP}.");
                MessageReceived?.Invoke(this, responseMessage); // fire event notifying subscribers that a message has been received.
            }
            catch (Exception ex)
            {
                _logger?.LogError($"{ex.Message}\n{ex.StackTrace}"); // log the exception
                throw; // rethrow
            }
        }
    }
}

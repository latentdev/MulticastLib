using Microsoft.Extensions.Logging;
using MulticastLib.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace MulticastLib
{
    /// <summary>
    /// Class representing a multicast endpoint. 
    /// </summary>
    internal class MulticastEndpoint
    {
        private ILogger<MulticastEndpoint> _logger;
        private UdpClient? _udpClient;
        private IPEndPoint? _localEndpoint;
        private IPEndPoint? _multicastGroupEndpoint;
        private bool _isInitialized = false;

        public bool IsInitialized { get => _isInitialized; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MulticastEndpoint(ILogger<MulticastEndpoint> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// This method creates the socket, joins it to the given multicast groups, and initializes
        /// the send/receive buffer.
        /// </summary>
        /// <param name="port">Local port to bind socket to</param>
        /// <param name="bufferLength">Length of the send/recv buffer to create</param>
        private void CreateSocket(IPAddress localIP, IPAddress multicastGroupIP, int port)
        {
            _localEndpoint = new IPEndPoint(IPAddress.Any, port);
            _multicastGroupEndpoint = new IPEndPoint(multicastGroupIP, port);
            _udpClient = new UdpClient(_localEndpoint);
            _udpClient.MulticastLoopback = false;
            _udpClient.Ttl = 1;
            _udpClient.JoinMulticastGroup(multicastGroupIP, localIP);
            _isInitialized = true;
        }

        /// <summary>
        /// This method creates and joins a multicast endpoint
        /// </summary>
        /// <param name="localIP">IP of the local network adapter to join a multicast group</param>
        /// <param name="multicastGroupIP">IP of the multicast group to join</param>
        /// <param name="port">port to listen on</param>
        /// <returns>Multicast Endpoint</returns>
        public static MulticastEndpoint Create(IPAddress localIP, IPAddress multicastGroupIP, int port)
        {
            var endpoint = new MulticastEndpoint();
            endpoint.CreateSocket(localIP, multicastGroupIP, port);
            return endpoint;
        }


        /// <summary>
        /// Send a message to the multicast group.
        /// </summary>
        /// <param name="packet">byte array to send to the multicast group</param>
        public async Task Send(byte[] packet)
        {
            try
            {
                await _udpClient?.SendAsync(packet, packet.Length, _multicastGroupEndpoint);
            }
            catch (SocketException se)
            {
                if (_logger == null)
                    Debug.WriteLine("Error: No Logger provided");
                _logger?.LogError($"Error encountered while sending: {packet}.\n{se.Message}");
            }
        }

        /// <summary>
        /// Reads any bytes that socket has received.
        /// </summary>
        /// <returns>byte array containing bytes read by socket</returns>
        public byte[] Receive()
        {
            return _udpClient?.Receive(ref _multicastGroupEndpoint);
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var received = await _udpClient?.ReceiveAsync();
            return received.Buffer;
        }

        /// <summary>
        /// This method drops membership to any joined groups. Not necessary as closing the socket 
        /// automatically leaves any multicast groups the socket was in.
        /// </summary>
        public void LeaveGroups()
        {
            _udpClient?.DropMulticastGroup(_multicastGroupEndpoint?.Address);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~MulticastEndpoint()
        {
            LeaveGroups();
        }
    }
}
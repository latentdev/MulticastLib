using MulticastLib.Interfaces;
using System.Net;
using System.Text;

namespace MulticastLib.Models
{
    /// <summary>
    /// Message Object. A message is used for easily creating and sending messages over the network.
    /// They contain the source IP describing where the message came from and a payload in the form of a string
    /// </summary>
    public class Message : IMessage
    {
        #region Private Variables

        private static int _headerLength = 4;

        #endregion

        #region Public Properties

        /// <summary>
        /// Message source IP
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// Message payload
        /// </summary>
        public string Payload { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor. Creates a new message with an ip and a default empty payload
        /// </summary>
        /// <param name="ip">IP representing the source of the message</param>
        /// <param name="payload">message payload in the form of a string</param>
        public Message(string ip, string payload = "")
        {
            IP = ip;
            Payload = payload;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Static method that returns a decrypted message object from a given byte array
        /// </summary>
        /// <param name="messageBytes">a byte array representing a message object</param>
        /// <returns>Message object representing a message either sent or received over a network</returns>
        public static Message GetMessage(byte[] messageBytes)
        {
            byte[] ipBytes = GetSubArray(messageBytes, 0, 4);

            var payloadLength = messageBytes.Length - _headerLength;

            if (payloadLength > 0) // check if there is a payload present
            {
                try
                {
                    byte[] payloadBytes = GetSubArray(messageBytes, _headerLength, payloadLength);
                    string payload = Encoding.UTF8.GetString(payloadBytes, 0, payloadLength); // convert payload bytes to a string
                    return new Message(new IPAddress(ipBytes).ToString(), payload); // create new message object
                }
                catch (Exception e)
                {
                    //todo: figure out logging method
                    throw e;
                }
            }
            else
            {
                return new Message(new IPAddress(ipBytes).ToString()); // if there is no payload return a new message with only the ip address set
            }
        }

        /// <summary>
        /// Converts a message into a string for logging
        /// </summary>
        /// <returns>A string representing the message</returns>
        public string GetMessageString()
        {
            StringBuilder messageString = new StringBuilder();
            messageString.AppendLine($"Source IP: {this.IP}");
            if (Payload != string.Empty)
                messageString.AppendLine($"Payload: {Payload}");
            return messageString.ToString();
        }

        /// <summary>
        /// Setter for the payload property
        /// </summary>
        /// <param name="payload">payload to be set</param>
        public void SetPayload(string payload)
        {
            Payload = payload;
        }

        /// <summary>
        /// Converts this message object into its byte array representation ready to be sent over a network in a packet. 
        /// </summary>
        /// <returns>a byte array representing the message</returns>
        public byte[] ToBytes()
        {
            byte[] messageBytes = new byte[4];
            IPAddress.Parse(IP).GetAddressBytes().CopyTo(messageBytes, 0);
            if (!Payload.Equals(string.Empty))
            {
                var payloadBytes = Encoding.UTF8.GetBytes(Payload);
                var headerBytes = messageBytes;
                messageBytes = new byte[headerBytes.Length + payloadBytes.Length];
                headerBytes.CopyTo(messageBytes, 0);
                payloadBytes.CopyTo(messageBytes, headerBytes.Length);
            }
            return messageBytes;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Gets a sub-array of a given byte array by copying bytes starting from the given index
        /// </summary>
        /// <param name="originalBytes">The original byte array</param>
        /// <param name="index">Starting index of the desired sub-array</param>
        /// <param name="count">Number of bytes from the index to copy</param>
        /// <returns>A new byte array that is the desired sub-array</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        private static byte[] GetSubArray(byte[] originalBytes, int index, int count)
        {
            if (index >= originalBytes.Length || index < 0) 
                throw new IndexOutOfRangeException($"Index is out of range. Index = {index}. Array length = {originalBytes.Length}");
            
            if ((index + count) > originalBytes.Length) 
                throw new IndexOutOfRangeException($"Attempted to copy bytes outside of byte array boundaries. Index + max_bytes_to_copy = {index + count}. Byte array length = {originalBytes.Length} ");

            byte[] bytes = new byte[count];
            for (int i = 0; i < count && (index + i) < originalBytes.Length; i++)
            {
                bytes[i] = originalBytes[index + i];
            }
            return bytes;
        }

        #endregion
    }
}

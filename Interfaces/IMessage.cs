namespace MulticastLib.Interfaces
{
    /// <summary>
    /// Interface used to hide implementation details of a network message. See <see cref="Models.Message"/> for interface implementation.
    /// </summary>
    public interface IMessage
    {
        #region Properties

        string IP { get; }
        string Payload { get; }

        #endregion

        #region Public Functions

        string GetMessageString();
        void SetPayload(string payload);
        byte[] ToBytes();

        #endregion
    }
}

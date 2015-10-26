namespace ChatterBox.Client.Common.Signaling
{
    public interface ISignalingSocketService
    {
        ISignalingSocketOperation SocketOperation { get; }
    }
}
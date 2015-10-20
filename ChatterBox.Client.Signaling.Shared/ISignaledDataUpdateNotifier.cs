namespace ChatterBox.Client.Signaling.Shared
{
    public interface ISignaledDataUpdateNotifier
    {
        void RaiseSignaledDataUpdated();
    }
}
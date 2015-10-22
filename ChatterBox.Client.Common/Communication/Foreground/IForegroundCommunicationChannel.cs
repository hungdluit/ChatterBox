namespace ChatterBox.Client.Common.Communication.Foreground
{
    public interface IForegroundCommunicationChannel
    {
        void OnSignaledDataUpdated();
    }
}
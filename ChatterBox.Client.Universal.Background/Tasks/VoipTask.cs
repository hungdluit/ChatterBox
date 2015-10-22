using Windows.ApplicationModel.Background;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Serialization;
using ChatterBox.Common.Communication.Shared.Messages.Relay;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class VoipTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (s, e) => _deferral.Complete();

            //Hub.Instance.SignalingClient.Relay(new RelayMessage
            //{
            //    FromUserId = RegistrationSettings.UserId,
            //    ToUserId = "",
            //    Tag = RelayMessageTags.InstantMessage,
            //});
        }
    }
}
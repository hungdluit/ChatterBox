using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;

namespace ChatterBox.Client.Win8._1.Helpers
{
    public class SignalingTaskHelper
    {
        private const string channelId = "ChatterBoxChannelId";
        private const int keepAliveInterval = 30;
        private ControlChannelTrigger _channel;

        public async Task<IBackgroundTaskRegistration> RegisterTask()
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();

            try
            {
                _channel = new ControlChannelTrigger(channelId, keepAliveInterval,
                       ControlChannelTriggerResourceType.RequestHardwareSlot);
            }
            catch (UnauthorizedAccessException ex)
            {

            }

            //var channelTaskBuilder = new BackgroundTaskBuilder();
            //channelTaskBuilder.Name = "ChannelTaskBuilder";
            //channelTaskBuilder.TaskEntryPoint = "BackgroundTaskHelper.KATask";
            //channelTaskBuilder.SetTrigger(_channel.KeepAliveTrigger);
            //channelTaskBuilder.Register();

            return null;
        }

    }
}

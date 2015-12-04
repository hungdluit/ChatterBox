using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class VoipTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        #region IBackgroundTask Members

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                if (Hub.Instance.VoipTaskInstance != null)
                {
                    Debug.WriteLine("VoipTask already started.");
                    return;
                }
                _deferral = taskInstance.GetDeferral();
                Hub.Instance.VoipTaskInstance = this;
                Debug.WriteLine($"{DateTime.Now} VoipTask started.");
                taskInstance.Canceled += (s, e) => CloseVoipTask();
            }
            catch (Exception e)
            {
                Hub.Instance.RTCStatsManager.TrackException(e);
            }
        }
        #endregion

        public void CloseVoipTask()
        {
            Debug.WriteLine($"{DateTime.Now} VoipTask closed.");
            Hub.Instance.VoipTaskInstance = null;
            _deferral.Complete();
        }
    }
}
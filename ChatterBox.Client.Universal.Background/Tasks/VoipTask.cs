using System.Diagnostics;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class VoipTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;


        

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (Hub.Instance.VoipTaskInstance != null)
            {
                Debug.WriteLine("VoipTask already started.");
                return;
            }

            _deferral = taskInstance.GetDeferral();
            Hub.Instance.VoipTaskInstance = this;
            Debug.WriteLine("VoipTask started.");
            taskInstance.Canceled += (s, e) => CloseVoipTask();
        }

        

        

        public void CloseVoipTask()
        {
            Debug.WriteLine("VoipTask closed.");
            Hub.Instance.VoipTaskInstance = null;
            _deferral.Complete();
        }
    }
}
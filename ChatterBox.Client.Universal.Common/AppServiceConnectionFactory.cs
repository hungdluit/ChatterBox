using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;

namespace ChatterBox.Client.Universal.Common
{
    public static class AppServiceConnectionFactory
    {
        public static AppServiceConnection NewForegroundConnection()
        {
            return new AppServiceConnection
            {
                AppServiceName = "ForegroundAppServiceTask",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
        }

        public static AppServiceConnection NewSignalingConnection()
        {
            return new AppServiceConnection
            {
                AppServiceName = "SignalingAppServiceTask",
                PackageFamilyName = Package.Current.Id.FamilyName
            };
        }
    }
}
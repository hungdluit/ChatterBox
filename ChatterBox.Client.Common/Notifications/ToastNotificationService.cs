using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ChatterBox.Client.Common.Notifications
{
    public sealed class ToastNotificationService
    {
        public static void ShowInstantMessageNotification(string fromName, string imageUri, string message)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // Set Text
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(fromName));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(message));

            // Set image
            var toastImageAttribute = toastXml.GetElementsByTagName("image").Select(s => ((XmlElement) s)).First();
            toastImageAttribute.SetAttribute("src", imageUri);
            toastImageAttribute.SetAttribute("alt", "logo");

            // toast duration
            var toastNode = toastXml.SelectSingleNode("/toast");
            var xmlElement = (XmlElement) toastNode;
            xmlElement?.SetAttribute("duration", "short");

            ShowNotification(toastXml);
        }

        private static void ShowNotification(XmlDocument toastXml)
        {
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
        }

        public static void ShowPresenceNotification(string name, string imageUri, bool isOnline)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // Set Text
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(name));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(isOnline ? "is now Online" : "is now Offline"));

            // Set image
            var toastImageAttribute = toastXml.GetElementsByTagName("image").Select(s => ((XmlElement) s)).First();
            toastImageAttribute.SetAttribute("src", imageUri);
            toastImageAttribute.SetAttribute("alt", "logo");

            // toast duration
            var toastNode = toastXml.SelectSingleNode("/toast");
            var xmlElement = (XmlElement) toastNode;
            xmlElement?.SetAttribute("duration", "short");

            ShowNotification(toastXml);
        }

        public static void ShowToastNotification(string message)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes.First().AppendChild(toastXml.CreateTextNode(message));

            ShowNotification(toastXml);
        }
    }
}
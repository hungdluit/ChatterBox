using System.Collections.Generic;
using ChatterBox.Client.Common.Helpers;
using System.Runtime.Serialization;

namespace ChatterBox.Client.Common.Notifications
{
    public enum ArgumentType
    {
        FromId
    }

    public enum NotificationType
    {
        InstantMessage
    }

    [DataContract]
    public sealed class ToastNotificationLaunchArguments
    {

        [DataMember]
        public NotificationType type { get; private set; }
        [DataMember]
        public IDictionary<ArgumentType, object> arguments { get; set; }

        public ToastNotificationLaunchArguments()
        {
            //XmlSerialization needs empty constructor
        }

        public ToastNotificationLaunchArguments(NotificationType type)
        {
            this.type = type;
            arguments = new Dictionary<ArgumentType, object>();
        }

        public string ToXmlString()
        {
            return XmlDataContractSerializationHelper.ToXml(this);
        }

        public static ToastNotificationLaunchArguments FromXmlString(string xmlString)
        {
            return XmlDataContractSerializationHelper.FromXml<ToastNotificationLaunchArguments>(xmlString);
        }
    }
}

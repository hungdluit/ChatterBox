namespace ChatterBox.Common.Communication.Shared.Messages.Relay
{
    public static class RelayMessageTags
    {
        public static string InstantMessage { get; } = nameof(InstantMessage);
        public static string SdpAnswer { get; } = nameof(SdpAnswer);
    }
}
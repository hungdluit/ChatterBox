namespace ChatterBox.Common.Communication.Shared.Messages.Relay
{
    public static class RelayMessageTags
    {
        public static string InstantMessage { get; } = nameof(InstantMessage);

        #region Voip tags
        public static string VoipCall { get; } = nameof(VoipCall);
        public static string VoipAnswer { get; } = nameof(VoipAnswer);
        public static string VoipReject { get; } = nameof(VoipReject);
        public static string SdpOffer { get; } = nameof(SdpOffer);
        public static string SdpAnswer { get; } = nameof(SdpAnswer);
        public static string IceCandidate { get; } = nameof(IceCandidate);
        public static string VoipHangup { get; } = nameof(VoipHangup);
        #endregion
    }
}
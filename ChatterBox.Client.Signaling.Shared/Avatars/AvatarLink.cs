namespace ChatterBox.Client.Signaling.Shared.Avatars
{
    public static class AvatarLink
    {
        public static string For(int avatar)
        {
            if (avatar >= 1 && avatar <= 10)
            {
                return $"ms-appx:///ChatterBox.Client.Signaling/Avatars/{avatar}.jpg";
            }
            return "ms-appx:///ChatterBox.Client.Signaling/Avatars/0.jpg";
        }
    }
}
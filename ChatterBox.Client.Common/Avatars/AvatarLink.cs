namespace ChatterBox.Client.Common.Avatars
{
    public static class AvatarLink
    {
        public static string For(int avatar)
        {
            if (avatar >= 1 && avatar <= 10)
            {
                return $"ms-appx:///ChatterBox.Client.Common/Avatars/{avatar}.jpg";
            }
            return "ms-appx:///ChatterBox.Client.Common/Avatars/0.jpg";
        }
    }
}
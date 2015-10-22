using ChatterBox.Client.Common.Communication.Voip.Dto;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public interface IVoipChannel
    {
        void HandleSdpAnswer(SdpAnswer sdpAnswer);
    }
}

﻿using System;

namespace ChatterBox.Client.Presentation.Shared.Services
{
    public interface ISignalingUpdateService
    {
        event Action OnUpdate;
        void RaiseUpdate();
    }
}
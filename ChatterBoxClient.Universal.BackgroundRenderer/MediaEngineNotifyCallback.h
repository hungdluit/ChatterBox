#pragma once
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

namespace ChatterBoxClient { namespace Universal { namespace BackgroundRenderer {

public interface struct MediaEngineNotifyCallback
{
    void OnMediaEngineEvent(uint32 meEvent, uintptr_t param1, uint32 param2) = 0;
};

}}}

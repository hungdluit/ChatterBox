#pragma once

#include <Mfmediaengine.h>
#include "MediaEngineNotifyCallback.h"
#include <wrl.h>
#include <wrl\client.h>
#include <wrl\implements.h>
#include <wrl\ftm.h>
#include <wrl\event.h> 
#include <wrl\wrappers\corewrappers.h>
#include <wrl\module.h>

namespace ChatterBoxClient { namespace Universal { namespace BackgroundRenderer {
            
class MediaEngineNotify :
    public Microsoft::WRL::RuntimeClass<
    Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::RuntimeClassType::ClassicCom>,
    IMFMediaEngineNotify>
{
public:
    // MediaEngineNotify
    void SetCallback(MediaEngineNotifyCallback^ callback);
    // IMFMediaEngineNotify
    IFACEMETHOD(EventNotify)(DWORD evt, DWORD_PTR param1, DWORD param2);
private:
    virtual ~MediaEngineNotify();
    MediaEngineNotifyCallback^ _callback;
};

}}}

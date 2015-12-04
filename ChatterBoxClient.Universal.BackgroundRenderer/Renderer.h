#pragma once
#include "MediaEngineNotifyCallback.h"
#include <collection.h>
#include <ppltasks.h>
#include <d3d11_2.h>
#include <d2d1_2.h>
#include <mfidl.h>
#include <mfapi.h>
#include <mferror.h>

#include <windows.foundation.h>
#include <windows.foundation.collections.h>
#include <windows.media.h>
#include <windows.media.capture.h>
#include <windows.media.mediaproperties.h>
#include <Mfmediaengine.h>
#include <wrl\wrappers\corewrappers.h>

namespace ChatterBoxClient { namespace Universal { namespace BackgroundRenderer {

public delegate void RenderFormatUpdateHandler(int64 swapChainHandle, uint32 width, uint32 height);

[Windows::Foundation::Metadata::WebHostHidden]
public ref class Renderer sealed :
    public MediaEngineNotifyCallback
{
public:
    Renderer();
    void Teardown();
    virtual ~Renderer();
    void SetupRenderer(uint32 foregroundProcessId, Windows::Media::Core::IMediaSource^ streamSource);
    static uint32 GetProcessId();
    event RenderFormatUpdateHandler^ RenderFormatUpdate;
    // MediaEngineNotifyCallback
    virtual void OnMediaEngineEvent(uint32 meEvent, uintptr_t param1, uint32 param2);
private:
    void SetupSchemeHandler();
    void SetupDirectX();
    void CreateDXDevice();
    void SendSwapChainHandle(HANDLE swapChain, bool forceNewHandle);

    bool _useHardware;
    Microsoft::WRL::ComPtr<ABI::Windows::Media::IMediaExtensionManager> _mediaExtensionManager;
    Microsoft::WRL::ComPtr<ABI::Windows::Foundation::Collections::IMap<HSTRING, IInspectable*>> _extensionManagerProperties;
    Microsoft::WRL::ComPtr<ID3D11Device> _device;
    Microsoft::WRL::ComPtr<ID3D11DeviceContext> _dx11DeviceContext;
    Microsoft::WRL::ComPtr<IMFDXGIDeviceManager> _dxGIManager;
    Microsoft::WRL::ComPtr<IMFMediaEngine> _mediaEngine;
    Microsoft::WRL::ComPtr<IMFMediaEngineEx> _mediaEngineEx;
    DWORD _foregroundProcessId;
    HANDLE _foregroundSwapChainHandle;
    Windows::Media::Core::IMediaSource^ _streamSource;
};

}}}

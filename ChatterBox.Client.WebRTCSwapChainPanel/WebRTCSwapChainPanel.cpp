﻿#define NOMINMAX
#include <collection.h>
#include <ppltasks.h>
#include <Windows.ui.xaml.media.dxinterop.h>
#include "WebRTCSwapChainPanel.h"
#include <algorithm>

using ChatterBox::Client::WebRTCSwapChainPanel::WebRTCSwapChainPanel;
using Microsoft::WRL::ComPtr;
using Platform::COMException;
using Platform::String;
using Windows::Foundation::Size;

WebRTCSwapChainPanel::WebRTCSwapChainPanel() : _handle(nullptr)
{
    _controlSize.Width = 0.0f;
    _controlSize.Height = 0.0;
    _nativeVideoSize.Width = 0.0f;
    _nativeVideoSize.Height = 0.0f;
    SizeChanged += ref new Windows::UI::Xaml::SizeChangedEventHandler(this, 
        &ChatterBox::Client::WebRTCSwapChainPanel::WebRTCSwapChainPanel::OnSizeChanged);
}

WebRTCSwapChainPanel::~WebRTCSwapChainPanel()
{
    if (_handle != nullptr)
    {
        CloseHandle(_handle);
    }
}

Windows::UI::Xaml::DependencyProperty^ WebRTCSwapChainPanel::_swapChainPanelHandleProperty = Windows::UI::Xaml::DependencyProperty::Register(
    L"SwapChainPanelHandle", __int64::typeid, WebRTCSwapChainPanel::typeid,
  ref new Windows::UI::Xaml::PropertyMetadata(nullptr,
    ref new Windows::UI::Xaml::PropertyChangedCallback(&WebRTCSwapChainPanel::OnSwapChainPanelHandleChanged)));

Windows::UI::Xaml::DependencyProperty^ WebRTCSwapChainPanel::_intSizeProperty = Windows::UI::Xaml::DependencyProperty::Register(
    ref new String(L"NativeVideoSize"), Size::typeid, WebRTCSwapChainPanel::typeid,
    ref new Windows::UI::Xaml::PropertyMetadata(nullptr,
        ref new Windows::UI::Xaml::PropertyChangedCallback(&WebRTCSwapChainPanel::OnNativeVideoSizeChanged)));

void WebRTCSwapChainPanel::UpdateHandle(int64 handle) {
  ComPtr<ISwapChainPanelNative2> nativePanel;
  HRESULT hr;
  if ((FAILED(hr = reinterpret_cast<IUnknown*>(this)->QueryInterface(IID_PPV_ARGS(&nativePanel)))) ||
    (!nativePanel))
  {
    throw ref new COMException(hr,
      ref new String(L"Failed to create native panel"));
  }

  HANDLE dupHandle = nullptr;

  if (handle != 0LL) {
    if (!DuplicateHandle(GetCurrentProcess(), (HANDLE)handle,
      GetCurrentProcess(), &dupHandle, 0, TRUE, DUPLICATE_SAME_ACCESS))
    {
      DWORD error = GetLastError();
      throw ref new COMException(HRESULT_FROM_WIN32(error),
        ref new String(L"Failed to duplicate foreground swap chain handle"));
    }
    hr = nativePanel->SetSwapChainHandle(dupHandle);
    if (FAILED(hr))
    {
      CloseHandle(dupHandle);
      throw ref new COMException(hr,
        ref new String(L"Failed to set swap chain panel"));
    }
  }
  else {
    hr = nativePanel->SetSwapChainHandle(0);
  }

  OutputDebugString((L"Setting swap chain handle to: " + handle.ToString() + L"->" + ((int64)dupHandle).ToString() + L"\n")->Data());
  if (_handle != nullptr)
  {
    CloseHandle(_handle);
    _handle = nullptr;
  }

  _handle = dupHandle;
  OutputDebugString((L"Setting swap chain handle done: " + ((int32)(void*)this).ToString() + "\n")->Data());
}

void WebRTCSwapChainPanel::SwapChainPanelHandle::set(int64 handle)
{
    SetValue(SwapChainPanelHandleProperty, handle);
}

int64 WebRTCSwapChainPanel::SwapChainPanelHandle::get()
{
    return (int64)GetValue(SwapChainPanelHandleProperty);
}

uint32 WebRTCSwapChainPanel::CurrentProcessId::get()
{
    return ::GetCurrentProcessId();
}

void WebRTCSwapChainPanel::NativeVideoSize::set(Size s)
{
    _nativeVideoSize = s;
    OnScaleChange();
    SetValue(SizeProperty, s);
}

Size WebRTCSwapChainPanel::NativeVideoSize::get()
{
    return (Size)GetValue(SizeProperty);
}

void WebRTCSwapChainPanel::OnScaleChange()
{
    if ((_nativeVideoSize.Width <= 0.0f) || (_nativeVideoSize.Height <= 0.0f)
        || (_controlSize.Width <= 0.0f) || (_controlSize.Height <= 0.0f))
    {
        return;
    }

    float scaleX = _controlSize.Width / _nativeVideoSize.Width;
    float scaleY = _controlSize.Height / _nativeVideoSize.Height;

    auto scaleTrans = ref new Windows::UI::Xaml::Media::ScaleTransform();
    scaleTrans->ScaleX = std::min(scaleX, scaleY);
    scaleTrans->ScaleY = std::min(scaleX, scaleY);

    RenderTransform = scaleTrans;
}

void WebRTCSwapChainPanel::OnSwapChainPanelHandleChanged(Windows::UI::Xaml::DependencyObject^ d,
    Windows::UI::Xaml::DependencyPropertyChangedEventArgs^ e)
{
    WebRTCSwapChainPanel^ control = (WebRTCSwapChainPanel^)d;
    int64 val = (int64)(e->NewValue);
    control->UpdateHandle(val);
}

void WebRTCSwapChainPanel::OnNativeVideoSizeChanged(Windows::UI::Xaml::DependencyObject^ d,
    Windows::UI::Xaml::DependencyPropertyChangedEventArgs^ e)
{
    WebRTCSwapChainPanel^ control = (WebRTCSwapChainPanel^)d;
    Size s = (Size)(e->NewValue);
    control->NativeVideoSize = s;
}

void WebRTCSwapChainPanel::OnSizeChanged(Platform::Object ^sender, Windows::UI::Xaml::SizeChangedEventArgs ^e)
{
    _controlSize.Width = (float)e->NewSize.Width;
    _controlSize.Height = (float)e->NewSize.Height;
    OnScaleChange();
}

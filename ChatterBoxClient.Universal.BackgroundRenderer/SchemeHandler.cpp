﻿#include "SchemeHandler.h"

using namespace ChatterBoxClient::Universal::BackgroundRenderer;
using namespace Microsoft::WRL::Wrappers;
using namespace Microsoft::WRL;

SchemeHandler::SchemeHandler()
{
  OutputDebugString(L"SchemeHandler::SchemeHandler()");
}

SchemeHandler::~SchemeHandler()
{
  OutputDebugString(L"SchemeHandler::~SchemeHandler()");
}

// IMediaExtension methods
IFACEMETHODIMP SchemeHandler::SetProperties(ABI::Windows::Foundation::Collections::IPropertySet *pConfiguration)
{
    _extensionManagerProperties = pConfiguration;
    return S_OK;
}

// IMFSchemeHandler methods
IFACEMETHODIMP SchemeHandler::BeginCreateObject(
    _In_ LPCWSTR pwszURL,
    _In_ DWORD dwFlags,
    _In_ IPropertyStore *pProps,
    _COM_Outptr_opt_  IUnknown **ppIUnknownCancelCookie,
    _In_ IMFAsyncCallback *pCallback,
    _In_ IUnknown *punkState)
{
    if (ppIUnknownCancelCookie != nullptr)
    {
        *ppIUnknownCancelCookie = nullptr;
    }

    if ((pwszURL == nullptr) || (pCallback == nullptr))
    {
        return E_INVALIDARG;
    }

    if ((dwFlags & MF_RESOLUTION_MEDIASOURCE) == 0)
    {
        return E_INVALIDARG;
    }
    ComPtr<ABI::Windows::Foundation::Collections::IMap<HSTRING, IInspectable*>> propMap;
    HRESULT hr = _extensionManagerProperties.As(&propMap);
    if (FAILED(hr))
    {
        return hr;
    }
    ComPtr<IInspectable> frameSourceInspectable;

    hr = propMap->Lookup(HStringReference(pwszURL).Get(),
        frameSourceInspectable.GetAddressOf());
    if (FAILED(hr))
    {
        unsigned int size;
        hr = propMap->get_Size(&size);
        if (size == 0)
        {
            return E_FAIL;
        }
        return hr;
    }
    propMap->Clear();
    ComPtr<IMFAsyncResult> result;
    hr = MFCreateAsyncResult(frameSourceInspectable.Get(), pCallback, punkState, &result);
    if (FAILED(hr))
    {
        return hr;
    }
    hr = pCallback->Invoke(result.Get());
    if (FAILED(hr))
    {
        return hr;
    }
    return result->GetStatus();
}

IFACEMETHODIMP SchemeHandler::EndCreateObject(
    _In_ IMFAsyncResult *pResult,
    _Out_  MF_OBJECT_TYPE *pObjectType,
    _Out_  IUnknown **ppObject)
{
    if ((pResult == nullptr) || (pObjectType == nullptr) || (ppObject == nullptr))
    {
        return E_INVALIDARG;
    }
    *pObjectType = MF_OBJECT_INVALID;
    *ppObject = nullptr;
    HRESULT hr = pResult->GetStatus();
    if (FAILED(hr))
    {
        return hr;
    }
    ComPtr<IUnknown> source;
    hr = pResult->GetObject(&source);
    if (FAILED(hr))
    {
        return hr;
    }
    *ppObject = source.Get();
    (*ppObject)->AddRef();
    *pObjectType = MF_OBJECT_MEDIASOURCE;
    return S_OK;
}

IFACEMETHODIMP SchemeHandler::CancelObjectCreation(
    _In_ IUnknown *pIUnknownCancelCookie)
{
    return E_NOTIMPL;
}

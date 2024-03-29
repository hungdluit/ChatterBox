#include <wrl\module.h>
#include <roapi.h>
#include <Windows.ApplicationModel.Core.h>

using namespace ABI::Windows::ApplicationModel::Core;
using namespace Microsoft::WRL;

class ExeServerGetActivationFactory : public RuntimeClass<
	ABI::Windows::Foundation::IGetActivationFactory,
	FtmBase>
{
public:
	IFACEMETHODIMP GetActivationFactory(_In_ HSTRING activatableClassId, _COM_Outptr_ IInspectable **factory)
	{
		*factory = nullptr;
		ComPtr<IActivationFactory> activationFactory;
		auto &module = Microsoft::WRL::Module<Microsoft::WRL::InProc>::GetModule();
		HRESULT hr = module.GetActivationFactory(activatableClassId, &activationFactory);
		if (SUCCEEDED(hr))
		{
			*factory = activationFactory.Detach();
		}
		return hr;
	}
};

int CALLBACK WinMain(_In_  HINSTANCE, _In_  HINSTANCE, _In_  LPSTR, _In_  int)
{
	HRESULT hr = Windows::Foundation::Initialize(RO_INIT_MULTITHREADED);
	if (SUCCEEDED(hr))
	{
		{
			ComPtr<ICoreApplication> spApplicationFactory;
			hr = Windows::Foundation::GetActivationFactory(Wrappers::HStringReference(RuntimeClass_Windows_ApplicationModel_Core_CoreApplication).Get(), &spApplicationFactory);
			if (SUCCEEDED(hr))
			{
				ComPtr<ABI::Windows::Foundation::IGetActivationFactory> spGetActivationFactory = Make<ExeServerGetActivationFactory>();
				spApplicationFactory->RunWithActivationFactories(spGetActivationFactory.Get());
			}
		}
		Windows::Foundation::Uninitialize();
	}
}

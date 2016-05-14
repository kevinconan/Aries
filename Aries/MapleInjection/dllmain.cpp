// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include <windows.h>
#include "stdafx.h"
#include "MSCorEE.h"
#include "metahost.h"
#pragma comment(lib, "mscoree.lib")
DWORD CALLBACK StartTheDotNetRuntime(LPVOID lp)
{

	HRESULT hr = S_OK;
	ICLRMetaHost    *m_pMetaHost = NULL;
	ICLRRuntimeInfo *m_pRuntimeInfo = NULL;
	ICLRRuntimeHost    *pClrHost = NULL;

	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&m_pMetaHost);
	if (hr != S_OK)
		return hr;
	hr = m_pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&m_pRuntimeInfo);
	if (hr != S_OK)
		return hr;
	hr = m_pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pClrHost);
	if (FAILED(hr)) return hr;
	HRESULT hrStart = pClrHost->Start();

	DWORD dwRet = 0;
	hr = pClrHost->ExecuteInDefaultAppDomain(
		L"C:\\Users\\LYHan\\Documents\\Aries\\Aries.Injection.dll",
		L"Aries.Injection.Testinjection", L"Start", L"", &dwRet);

	hr = pClrHost->Stop();
	
	pClrHost->Release();

	return S_OK;
}


BOOL WINAPI DllMain(
	HINSTANCE hinstDLL,
	DWORD fdwReason,
	LPVOID lpvReserved
)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
		CreateThread(0, 0, StartTheDotNetRuntime, 0, 0, 0);
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		break;
	default:
		break;
	}
	return true;
}


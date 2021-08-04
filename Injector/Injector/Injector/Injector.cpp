#include <Windows.h>
#include <TlHelp32.h>
#include <malloc.h>
#include <Tchar.h>
#include <vector>
#include <filesystem>
#include "Injector.h"
#include "EnsureCleanup.h"

static auto get_throw_error(std::tstring* Error)
{
	return [Error](const std::tstring& Message) { if (Error) *Error = Message; };
}

bool InjectLib(
	const DWORD ProcID, 
	const std::filesystem::path& Path,
	std::tstring* Error)
{
	const auto throw_error = get_throw_error(Error);

	HadesMem::Detail::EnsureCloseHandle Process(OpenProcess(
		PROCESS_QUERY_INFORMATION |  // Required by Alpha
		PROCESS_CREATE_THREAD     |  // For CreateRemoteThread
		PROCESS_VM_OPERATION      |  // For VirtualAllocEx/VirtualFreeEx
		PROCESS_VM_WRITE,            // For WriteProcessMemory  
		FALSE, ProcID));

	if (!Process)
	{
		throw_error(TEXT("Could not get handle to process."));
		return false;
	};

	const size_t Size = (Path.tstring().length() + 1) * sizeof(Path.tstring().front());

	HadesMem::Detail::EnsureReleaseRegionEx LibFileRemote(
		VirtualAllocEx(
			Process, 
			nullptr, 
			Size, 
			MEM_COMMIT, 
			PAGE_READWRITE),
		Process);

	if (!LibFileRemote)
	{
		throw_error(TEXT("Could not allocate memory in remote process."));
		return false;
	}

	if (!WriteProcessMemory(
		Process,
		LibFileRemote,
		Path.c_str(),
		Size, nullptr))
	{
		throw_error(TEXT("Could not write to memory in remote process."));
		return false;
	}

	HMODULE hKernel32 = GetModuleHandle(TEXT("Kernel32"));
	if (!hKernel32)
	{
		throw_error(TEXT("Could not get handle to Kernel32."));
		return false;
	}

	PTHREAD_START_ROUTINE pfnThreadRtn = reinterpret_cast<PTHREAD_START_ROUTINE>
		(GetProcAddress(hKernel32, "LoadLibraryW"));

	if (!pfnThreadRtn)
	{
		throw_error(TEXT("Could not get pointer to LoadLibraryW."));
		return false;
	}

	HadesMem::Detail::EnsureCloseHandle Thread(CreateRemoteThread(
		Process, 
		nullptr, 
		NULL, 
		pfnThreadRtn,
		LibFileRemote, 
		NULL, nullptr));

	if (!Thread)
	{
		throw_error(TEXT("Could not create thread in remote process."));
		return false;
	}

	WaitForSingleObject(Thread, INFINITE);

	DWORD ExitCode = NULL;
	if (!GetExitCodeThread(Thread, &ExitCode))
	{
		throw_error(TEXT("Could not get thread exit code."));
		return false;
	}

	if (!ExitCode)
	{
		throw_error(TEXT("Call to LoadLibraryW in remote process failed."));
		return false;
	}

	return true;
}

DWORD GetProcessIdByName(
	const std::tstring& Name, 
	std::tstring* Error)
{
	const auto throw_error = get_throw_error(Error);

	HadesMem::Detail::EnsureCloseHandle Snap(CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0));
	if (Snap == INVALID_HANDLE_VALUE)
	{
		throw_error(TEXT("Could not get process snapshot."));
		return NULL;
	}

	PROCESSENTRY32 ProcEntry = { sizeof(ProcEntry) };
	bool Found = false;
	BOOL MoreMods = Process32First(Snap, &ProcEntry);
	for (; MoreMods; MoreMods = Process32Next(Snap, &ProcEntry))
	{
		std::tstring CurrentProcess(ProcEntry.szExeFile);
		CurrentProcess = /*toLower*/(CurrentProcess);
		Found = (CurrentProcess == Name);
		if (Found) break;
	}

	if (!Found)
	{
		throw_error(TEXT("Could not find process."));
		return NULL;
	}

	return ProcEntry.th32ProcessID;
}

DWORD GetProcessIdByWindow(
	const std::tstring& Name, 
	std::tstring* Error)
{
	const auto throw_error = get_throw_error(Error);

	HWND MyWnd = FindWindow(NULL, Name.c_str());
	if (!MyWnd)
	{
		throw_error(TEXT("Could not find window."));
		return NULL;
	}

	DWORD ProcID;
	GetWindowThreadProcessId(MyWnd, &ProcID);
	if (!ProcID)
	{
		throw_error(TEXT("Could not get process id from window."));
		return NULL;
	}

	return ProcID;
}

#include <string>
#include <filesystem>
#include "Injector.h"

static std::filesystem::path get_application_directory()
{
	TCHAR buffer[MAX_PATH] = { 0 };
	GetModuleFileName(NULL, buffer, MAX_PATH);
	return std::filesystem::path(buffer).parent_path();
}

bool inject_dll(
	const std::filesystem::path& dll_name,
	const std::tstring& executable_name, 
	const std::tstring& window_name)
{
	auto file_path = get_application_directory() / dll_name;
	auto hProc = GetProcessIdByName(executable_name);
	if (!hProc) hProc = GetProcessIdByWindow(window_name);
	if (!hProc) return false;
	return InjectLib(hProc, file_path.wstring());
}

int main(int argc, char* argv[])
{
	inject_dll(
		TEXT("Multiplayer.dll"), 
		TEXT("GeometryDash.exe"), 
		TEXT("Geometry Dash"));
}
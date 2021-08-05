#ifndef INJECTOR_H
#define INJECTOR_H

#include <Windows.h>
#include <string>
#include "StringWrap.h"

bool InjectLib(
	const DWORD ProcID,
	const std::filesystem::path& Path,
	std::tstring* Error = nullptr);

DWORD GetProcessIdByName(
	const std::tstring& Name,
	std::tstring* Error = nullptr);

DWORD GetProcessIdByWindow(
	const std::tstring& Name,
	std::tstring* Error = nullptr);

#endif // INJECTOR_H
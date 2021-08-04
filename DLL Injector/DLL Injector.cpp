#include <iostream>
#include <Windows.h>

/// adaf do your magic
int main()
{
    std::cout << "Multiplayer.dll Injector\r\n";

    LPCSTR DllPath = ""; // full path to Multiplayer.dll

    HWND hwnd = FindWindowA(NULL, "Geometry Dash"); // get gd window handle

    std::cout << "Done\r\n";
}
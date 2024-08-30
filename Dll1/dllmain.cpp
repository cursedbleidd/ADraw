// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"

extern "C"
{
    __declspec(dllexport) void __cdecl MName(char* in)
    {
        const char name[] = "Invert Color";
        strcpy_s(in, strlen(name) + 1, name);
    }

    __declspec(dllexport) void __cdecl MFunc(int* bitmap, int width, int height)
    {
        for (int i = 0; i < width * height; i++)
        {
            auto pixel = bitmap[i];
            
            int a = pixel >> 24;
            int r = (pixel >> 16) & 0xff;
            int g = (pixel >> 8) & 0xff;
            int b = (pixel & 0xff);
            
            bitmap[i] = (a << 24) | ((255 - r) << 16) | ((255 - g) << 8) | ((255 - b));
        }
    }
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


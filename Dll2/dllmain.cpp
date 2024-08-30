// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"

extern "C"
{

    void MirrorV(int* bitmap, int width, int height)
    {
        for (int i = 0; i < height / 2; i++)
            for (int j = 0; j < width; j++)
            {
                int temp = bitmap[width * i + j];
                bitmap[width * i + j] = bitmap[width * (height - i - 1) + j];
                bitmap[width * (height - i - 1) + j] = temp;
            }
    }
    void MirrorH(int* bitmap, int width, int height)
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width / 2; j++)
            {
                int temp = bitmap[width * i + j];
                bitmap[width * i + j] = bitmap[width * i + (width - j - 1)];
                bitmap[width * i + (width - j - 1)] = temp;
            }
    }


    __declspec(dllexport) void __cdecl MName(char* in)
    {
        const char name[] = "Mirror image";

        strcpy_s(in, strlen(name) + 1, name);
    }

    __declspec(dllexport) void __cdecl MFunc(int* bitmap, int width, int height)
    {
        MirrorV(bitmap, width, height);
        MirrorH(bitmap, width, height);
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

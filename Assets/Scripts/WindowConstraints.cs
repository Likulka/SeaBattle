using System;
using System.Runtime.InteropServices;
using UnityEngine;
public class WindowConstraints : MonoBehaviour
{
    private const int MF_BYCOMMAND = 0x00000000;
    private const int SC_MAXIMIZE = 0xF030;
    private const int SC_SIZE = 0xF000;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(System.IntPtr hMenu, int wIDEnableItem, int wEnable);

    private void Start()
    {
        // Установить размеры окна
        int width = 390;
        int height = 844;
        Screen.SetResolution(width, height, false);

        // Получить хендл окна
        var hwnd = GetActiveWindow();
        var hMenu = GetSystemMenu(hwnd, false);

        // Отключить возможность изменения размера и максимизации окна
        EnableMenuItem(hMenu, SC_SIZE, MF_BYCOMMAND | 0x1);
        EnableMenuItem(hMenu, SC_MAXIMIZE, MF_BYCOMMAND | 0x1);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetActiveWindow();
}

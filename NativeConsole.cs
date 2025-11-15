using System.Runtime.InteropServices;

public static class NativeConsole
{
    public enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(StdHandle std);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hdl);

    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();
}
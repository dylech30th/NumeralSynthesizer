using System.Runtime.InteropServices;

namespace NumeralSynthesizer;

public partial class NativeInterops
{
    public const int StdInputHandle = -10;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CancelIoEx(nint handle, nint lpOverlapped);
}
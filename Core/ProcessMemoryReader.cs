using System;
using System.Runtime.InteropServices;

public interface IProcessMemoryReader
{
    bool TryRead(IntPtr processHandle, IntPtr address, byte[] buffer, int count,
        out int bytesRead, out int errorCode);
}

public sealed class WindowsProcessMemoryReader : IProcessMemoryReader
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr address,
        [Out] byte[] buffer, UIntPtr count, out UIntPtr bytesRead);

    public bool TryRead(IntPtr processHandle, IntPtr address, byte[] buffer, int count,
        out int bytesRead, out int errorCode)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (count < 0 || count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));
        UIntPtr nativeBytesRead;
        bool success = ReadProcessMemory(processHandle, address, buffer,
            new UIntPtr((uint)count), out nativeBytesRead);
        errorCode = success ? 0 : Marshal.GetLastWin32Error();
        bytesRead = (int)Math.Min((ulong)count, nativeBytesRead.ToUInt64());
        return success;
    }
}

public sealed class MemoryReadFailure
{
    public IntPtr Address { get; }
    public int RequestedBytes { get; }
    public int BytesRead { get; }
    public int ErrorCode { get; }

    public MemoryReadFailure(IntPtr address, int requestedBytes, int bytesRead, int errorCode)
    {
        Address = address;
        RequestedBytes = requestedBytes;
        BytesRead = bytesRead;
        ErrorCode = errorCode;
    }
}

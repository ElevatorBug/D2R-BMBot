using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>Compatibility facade for existing decoders, independent of the UI.</summary>
public class Mem
{
    private readonly IProcessMemoryReader reader;
    private readonly Func<IntPtr> processHandle;
    private readonly Func<IntPtr> baseAddress;

    public event Action<MemoryReadFailure> ReadFailed;

    public Mem(IProcessMemoryReader reader, Func<IntPtr> processHandle, Func<IntPtr> baseAddress)
    {
        this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        this.processHandle = processHandle ?? throw new ArgumentNullException(nameof(processHandle));
        this.baseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
    }

    // Retained for legacy compatibility; the application opens the process for reading.
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WriteProcessMemory(IntPtr handle, IntPtr address,
        byte[] buffer, UIntPtr count, out UIntPtr bytesWritten);

    public void WriteRawMemory(IntPtr address, byte[] buffer, int writesize)
    {
        ValidateBuffer(buffer, writesize);
        UIntPtr written;
        WriteProcessMemory(processHandle(), address, buffer, new UIntPtr((uint)writesize), out written);
    }

    private static void ValidateBuffer(byte[] buffer, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (count < 0 || count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));
    }

    private bool ReadBuffer(IntPtr address, byte[] buffer, int count, out int bytesRead)
    {
        ValidateBuffer(buffer, count);
        bytesRead = 0;
        if (count == 0) return true;

        int errorCode;
        bool success = reader.TryRead(processHandle(), address, buffer, count, out bytesRead, out errorCode);
        // Bulk scans retain a valid prefix, but never reuse an unread suffix.
        if (bytesRead < 0 || bytesRead > count)
        {
            bytesRead = 0;
            success = false;
        }
        Array.Clear(buffer, bytesRead, count - bytesRead);
        if (!success || bytesRead != count)
            ReadFailed?.Invoke(new MemoryReadFailure(address, count, bytesRead, errorCode));
        return success && bytesRead == count;
    }

    private bool TryReadExact(IntPtr address, int count, out byte[] buffer)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        buffer = new byte[count];
        int bytesRead;
        if (ReadBuffer(address, buffer, count, out bytesRead)) return true;
        Array.Clear(buffer, 0, buffer.Length);
        return false;
    }

    public void ReadMemory(IntPtr address, ref byte[] buffer, int bytes, ref int pBytesRead)
    {
        ReadBuffer(address, buffer, bytes, out pBytesRead);
    }

    public void ReadRawMemory(long address, ref byte[] buffer, int bytes = 4, params int[] aOffsets)
    {
        // aOffsets remains unused, as in the original API; callers supply absolute addresses.
        TryReadExact(new IntPtr(address), bytes, out buffer);
    }

    public string ReadMemString(long address)
    {
        var name = new StringBuilder(16);
        for (int i = 0; i < 16; i++)
        {
            byte value = ReadByteRaw(new IntPtr(address + i));
            if (value == 0) break;
            name.Append((char)value);
        }
        return name.ToString();
    }

    private IntPtr RelativeAddress(IntPtr offset)
    {
        return new IntPtr(baseAddress().ToInt64() + offset.ToInt64());
    }

    public int ReadInt(IntPtr offset) => ReadInt32Raw(RelativeAddress(offset));
    public ushort ReadUInt16(IntPtr offset) => ReadUInt16Raw(RelativeAddress(offset));
    public uint ReadUInt32(IntPtr offset) => ReadUInt32Raw(RelativeAddress(offset));
    public int ReadInt32(IntPtr offset) => ReadInt32Raw(RelativeAddress(offset));
    public long ReadInt64(IntPtr offset) => ReadInt64Raw(RelativeAddress(offset));
    public char ReadUChar(IntPtr offset) => ReadUCharRaw(RelativeAddress(offset));
    public int ReadIntRaw(IntPtr address) => ReadInt32Raw(address);
    public char ReadUCharRaw(IntPtr address) => (char)ReadByteRaw(address);

    public byte ReadByteRaw(IntPtr address)
    {
        byte[] buffer;
        return TryReadExact(address, 1, out buffer) ? buffer[0] : (byte)0;
    }

    public ushort ReadUInt16Raw(IntPtr address)
    {
        byte[] buffer;
        return TryReadExact(address, 2, out buffer) ? BitConverter.ToUInt16(buffer, 0) : (ushort)0;
    }

    public uint ReadUInt32Raw(IntPtr address)
    {
        byte[] buffer;
        return TryReadExact(address, 4, out buffer) ? BitConverter.ToUInt32(buffer, 0) : 0;
    }

    public int ReadInt32Raw(IntPtr address)
    {
        byte[] buffer;
        return TryReadExact(address, 4, out buffer) ? BitConverter.ToInt32(buffer, 0) : 0;
    }

    public long ReadInt64Raw(IntPtr address)
    {
        byte[] buffer;
        return TryReadExact(address, 8, out buffer) ? BitConverter.ToInt64(buffer, 0) : 0;
    }

    private static IntPtr ToIntPtr(UIntPtr address)
    {
        return new IntPtr(unchecked((long)address.ToUInt64()));
    }

    public ulong ReadUInt64(UIntPtr address)
    {
        byte[] buffer;
        return TryReadExact(ToIntPtr(address), 8, out buffer) ? BitConverter.ToUInt64(buffer, 0) : 0;
    }

    public uint ReadUIntFromBuffer(byte[] bytes, uint offset, int size)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        if (size < 1 || size > 4) throw new ArgumentOutOfRangeException(nameof(size));
        if ((ulong)offset + (uint)size > (ulong)bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        uint result = 0;
        for (int i = 0; i < size; i++)
            result |= (uint)bytes[(int)offset + i] << (8 * i);
        return result;
    }

    public byte[] ReadBytesFromMemory(IntPtr address, int size)
    {
        byte[] buffer;
        TryReadExact(address, size, out buffer);
        return buffer;
    }

    public byte[] ReadBytesFromMemory(UIntPtr address, uint size)
    {
        return ReadBytesFromMemory(ToIntPtr(address), checked((int)size));
    }

    public Dictionary<Enums.Attribute, int> GetMonsterStats(uint statCount, UIntPtr statPtr)
    {
        var stats = new Dictionary<Enums.Attribute, int>();
        if (statCount == 0) return stats;

        int size = checked((int)((ulong)statCount * 8));
        byte[] buffer;
        IntPtr address = ToIntPtr(UIntPtr.Add(statPtr, 2));
        if (!TryReadExact(address, size, out buffer)) return stats;

        for (int i = 0; i < (int)statCount; i++)
        {
            uint offset = (uint)(i * 8);
            ushort statEnum = (ushort)ReadUIntFromBuffer(buffer, offset, 2);
            uint statValue = ReadUIntFromBuffer(buffer, offset + 2, 4);
            if (Enum.IsDefined(typeof(Enums.Attribute), (int)statEnum))
                stats[(Enums.Attribute)statEnum] = unchecked((int)statValue);
        }
        return stats;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal static class RuntimeTests
{
    private static int passed;

    private static void Main()
    {
        Test("exact reads and relative addressing", ExactReads);
        Test("short and failed typed reads", FailedTypedReads);
        Test("bulk reads preserve prefix and clear stale suffix", BulkReads);
        Test("invalid byte counts are rejected", InvalidCounts);
        Test("parallel reads own their buffers", ParallelReads);
        Test("strings stop at NUL and at sixteen bytes", Strings);
        Test("buffer bounds and little endian decoding", BufferBounds);
        Test("monster stats reject incomplete buffers and overflow", MonsterStats);
        Test("priority, one step per tick and dynamic eligibility", RunOrder);
        Test("empty sequences, defensive copy and exceptions", RunEdges);
        Test("player snapshot retains captured values", Snapshot);
        Console.WriteLine("Passed {0} runtime tests.", passed);
    }

    private static void Test(string name, Action test)
    {
        test();
        passed++;
        Console.WriteLine("PASS " + name);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    private static Mem Create(FakeReader reader)
    {
        return new Mem(reader, () => new IntPtr(0x123456789L), () => new IntPtr(0x100000000L));
    }

    private static void ExactReads()
    {
        var reader = new FakeReader { Data = BitConverter.GetBytes(-123456789L) };
        Mem mem = Create(reader);
        Check(mem.ReadInt64(new IntPtr(32)) == -123456789L, "64-bit relative read");
        Check(reader.Address.ToInt64() == 0x100000020L, "Base address was lost");
        Check(reader.Handle.ToInt64() == 0x123456789L, "Process handle was truncated");
        reader.Data = BitConverter.GetBytes(0xfedcba98U);
        Check(mem.ReadUInt32Raw(new IntPtr(1)) == 0xfedcba98U, "uint decode");
        Check(mem.ReadInt32Raw(new IntPtr(1)) == unchecked((int)0xfedcba98U), "signed decode");
        Check(mem.ReadUInt16Raw(new IntPtr(1)) == 0xba98, "ushort decode");
        Check(mem.ReadByteRaw(new IntPtr(1)) == 0x98, "byte decode");
        reader.Data = BitConverter.GetBytes(ulong.MaxValue);
        Check(mem.ReadUInt64(new UIntPtr(1)) == ulong.MaxValue, "ulong decode");
    }

    private static void FailedTypedReads()
    {
        var reader = new FakeReader { Data = new byte[] { 1, 2, 3, 4 }, Count = 2, Error = 299 };
        Mem mem = Create(reader);
        MemoryReadFailure failure = null;
        mem.ReadFailed += value => failure = value;
        Check(mem.ReadUInt32Raw(new IntPtr(42)) == 0, "Partial value accepted");
        Check(failure != null && failure.Address.ToInt64() == 42 && failure.RequestedBytes == 4 &&
            failure.BytesRead == 2 && failure.ErrorCode == 299, "Missing failure details");
        Check(mem.ReadBytesFromMemory(new IntPtr(42), 4).All(value => value == 0), "Partial structure accepted");
        reader.Count = 4;
        reader.Success = false;
        Check(mem.ReadUInt32Raw(new IntPtr(42)) == 0, "Native failure accepted");
        reader.Count = 0;
        Check(mem.ReadInt32Raw(new IntPtr(42)) == 0, "Failed read retained stale bytes");
    }

    private static void BulkReads()
    {
        var reader = new FakeReader { Data = new byte[] { 1, 2 }, Count = 2, Success = false };
        Mem mem = Create(reader);
        byte[] buffer = { 99, 99, 99, 99 };
        int count = -1;
        mem.ReadMemory(new IntPtr(1), ref buffer, 4, ref count);
        Check(count == 2 && buffer.SequenceEqual(new byte[] { 1, 2, 0, 0 }), "Lost prefix or stale suffix");
        reader.Count = 0;
        mem.ReadMemory(new IntPtr(1), ref buffer, 4, ref count);
        Check(count == 0 && buffer.All(value => value == 0), "Failed bulk read retained old data");
    }

    private static void InvalidCounts()
    {
        var reader = new FakeReader { Data = new byte[] { 1, 2, 3, 4 }, Count = 5 };
        Mem mem = Create(reader);
        Check(mem.ReadInt32Raw(new IntPtr(1)) == 0, "Impossible byte count accepted");
        reader.Count = -2;
        Check(mem.ReadInt32Raw(new IntPtr(1)) == 0, "Negative byte count accepted");
        Throws<ArgumentOutOfRangeException>(() => mem.ReadBytesFromMemory(new IntPtr(1), -1));
        Check(mem.ReadBytesFromMemory(new IntPtr(1), 0).Length == 0, "Empty read");
    }

    private static void ParallelReads()
    {
        var reader = new FakeReader { UseAddressAsValue = true };
        Mem mem = Create(reader);
        Parallel.For(1, 2000, i =>
            Check(mem.ReadInt32Raw(new IntPtr(i)) == i, "Another read replaced this buffer"));
    }

    private static void Strings()
    {
        var reader = new FakeReader { Data = new byte[] { (byte)'a', (byte)'b', 0, (byte)'c' }, StringMode = true };
        Mem mem = Create(reader);
        Check(mem.ReadMemString(100) == "ab", "NUL did not terminate the name");
        reader.Data = Enumerable.Repeat((byte)'x', 20).ToArray();
        Check(mem.ReadMemString(100).Length == 16, "Name limit changed");
    }

    private static void BufferBounds()
    {
        Mem mem = Create(new FakeReader());
        byte[] data = { 0, 0x78, 0x56, 0x34, 0x12 };
        Check(mem.ReadUIntFromBuffer(data, 1, 4) == 0x12345678, "Little endian decode");
        Throws<ArgumentOutOfRangeException>(() => mem.ReadUIntFromBuffer(data, uint.MaxValue, 4));
        Throws<ArgumentOutOfRangeException>(() => mem.ReadUIntFromBuffer(data, 4, 2));
        Throws<ArgumentOutOfRangeException>(() => mem.ReadUIntFromBuffer(data, 0, 5));
        Throws<ArgumentOutOfRangeException>(() => mem.ReadUIntFromBuffer(data, 0, -1));
        Throws<ArgumentNullException>(() => mem.ReadUIntFromBuffer(null, 0, 1));
    }

    private static void MonsterStats()
    {
        var data = new byte[8];
        Array.Copy(BitConverter.GetBytes((ushort)Enums.Attribute.Life), data, 2);
        Array.Copy(BitConverter.GetBytes(123U), 0, data, 2, 4);
        var reader = new FakeReader { Data = data };
        Mem mem = Create(reader);
        Check(mem.GetMonsterStats(1, new UIntPtr(100))[Enums.Attribute.Life] == 123, "Stat decode");
        Check(reader.Address.ToInt64() == 102, "Stat header offset changed");
        reader.Count = 2;
        Check(mem.GetMonsterStats(1, new UIntPtr(100)).Count == 0, "Partial stats accepted");
        Throws<OverflowException>(() => mem.GetMonsterStats(uint.MaxValue, new UIntPtr(100)));
    }

    private static void RunOrder()
    {
        bool firstEnabled = true, firstDone = false;
        var executions = new List<int>();
        var sequence = new RunSequence(
            new RunStep(() => firstEnabled && !firstDone, () => executions.Add(1)),
            new RunStep(() => true, () => executions.Add(2)));
        Check(sequence.TryExecuteNext() && executions.SequenceEqual(new[] { 1 }), "Priority or one-per-tick changed");
        firstDone = true;
        Check(sequence.TryExecuteNext() && executions.SequenceEqual(new[] { 1, 2 }), "Done flag ignored");
        firstDone = false;
        firstEnabled = false;
        sequence.TryExecuteNext();
        Check(executions.Last() == 2, "Enable flag was captured at construction");
        firstEnabled = true;
        sequence.TryExecuteNext();
        Check(executions.Last() == 1, "Re-enabled first step did not regain priority");
    }

    private static void RunEdges()
    {
        Check(!new RunSequence().TryExecuteNext(), "Empty sequence ran");
        Check(!new RunSequence(new RunStep(() => false, () => { })).TryExecuteNext(), "Ineligible sequence ran");
        bool ran = false;
        var steps = new[] { new RunStep(() => true, () => ran = true) };
        var sequence = new RunSequence(steps);
        steps[0] = null;
        sequence.TryExecuteNext();
        Check(ran, "Caller changed sequence storage");
        bool fallbackRan = false;
        var failing = new RunSequence(new RunStep(() => true, () => { throw new InvalidOperationException(); }),
            new RunStep(() => true, () => fallbackRan = true));
        Throws<InvalidOperationException>(() => failing.TryExecuteNext());
        Check(!fallbackRan, "Exception caused another step to run");
    }

    private static void Snapshot()
    {
        long life = 25;
        var snapshot = new PlayerStateSnapshot("test", 10, 20, life, 100, 30, 50, 1, 2, 3);
        life = 0;
        Check(snapshot.Life == 25 && snapshot.X == 10 && snapshot.Mana == 30, "Snapshot values changed");
        Check(snapshot.CapturedAtUtc.Kind == DateTimeKind.Utc, "Snapshot time is not UTC");
        Check(typeof(PlayerStateSnapshot).GetProperties().All(p => !p.CanWrite), "Snapshot is mutable");
    }

    private sealed class FakeReader : IProcessMemoryReader
    {
        public byte[] Data = new byte[8];
        public int? Count;
        public int Error;
        public bool Success = true;
        public bool UseAddressAsValue;
        public bool StringMode;
        public IntPtr Address, Handle;

        public bool TryRead(IntPtr handle, IntPtr address, byte[] buffer, int count,
            out int bytesRead, out int errorCode)
        {
            Handle = handle;
            Address = address;
            byte[] source = UseAddressAsValue ? BitConverter.GetBytes(address.ToInt32()) : Data;
            if (StringMode) buffer[0] = source[(int)(address.ToInt64() - 100)];
            else Array.Copy(source, buffer, Math.Min(count, source.Length));
            bytesRead = Count ?? count;
            errorCode = Error;
            return Success;
        }
    }
}

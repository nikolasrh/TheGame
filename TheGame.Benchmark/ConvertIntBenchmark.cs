using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;

namespace TheGame.Benchmark;

[MemoryDiagnoser]
public class ConvertIntBenchmark
{
    [Benchmark]
    public void ToInt_MemoryMarshal()
    {
        Span<byte> buffer = stackalloc byte[4];

        MemoryMarshal.AsRef<int>(buffer);
    }

    [Benchmark]
    public void ToInt_BitConverter()
    {
        Span<byte> buffer = stackalloc byte[4];

        BitConverter.ToInt32(buffer);
    }

    [Benchmark]
    public void FromInt_MemoryMarshal()
    {
        var number = 1234567890;
        Span<byte> buffer = stackalloc byte[4];

        MemoryMarshal.Write(buffer, ref number);
    }

    [Benchmark]
    public void FromInt_BitConverter()
    {
        var number = 1234567890;
        Span<byte> buffer = stackalloc byte[4];

        BitConverter.TryWriteBytes(buffer, number);
    }
}

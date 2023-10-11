using System.Runtime.CompilerServices;
using System.Text;

partial class Program
{
    private const uint Marker2ByteUtf8Char = 0b011000000u;

    private const uint Marker3ByteUtf8Char = 0b011100000u;

    private const uint Marker4ByteUtf8Char = 0b011110000u;

    private const uint MarkerUtf8Supplementary = 0b010000000u;

    private static Encoding Iso88592 { get; }

    private static UTF8Encoding Utf8NoBOM { get; } = new(false);

    static Program()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Iso88592 = Encoding.GetEncoding("ISO-8859-2");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasMarker(byte source, uint marker)
        => unchecked((source & marker) == marker);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAscii7Bit(byte source)
        => (source & 0x080u) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Has2ByteUtf8Marker(byte source)
        => HasMarker(source, Marker2ByteUtf8Char);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Has3ByteUtf8Marker(byte source)
        => HasMarker(source, Marker3ByteUtf8Char);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Has4ByteUtf8Marker(byte source)
        => HasMarker(source, Marker4ByteUtf8Char);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasUtf8SupplementaryMarker(byte source)
        => HasMarker(source, MarkerUtf8Supplementary);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static bool IsValidUtf8(byte[] data, int i, out int size)
    {
        if (IsAscii7Bit(data[i]))
        {
            size = 1;
            return true;
        }
        if (Has4ByteUtf8Marker(data[i]))
        {
            // 4 byte utf-8
            size = 4;
            return i + 3 < data.Length && HasUtf8SupplementaryMarker(data[i + 1])
                && HasUtf8SupplementaryMarker(data[i + 2])
                && HasUtf8SupplementaryMarker(data[i + 3]);
        }
        if (Has3ByteUtf8Marker(data[i]))
        {
            // 3 byte utf-8
            size = 3;
            return i + 2 < data.Length && HasUtf8SupplementaryMarker(data[i + 1])
                && HasUtf8SupplementaryMarker(data[i + 2]);
        }
        if (Has2ByteUtf8Marker(data[i]))
        {
            // 2 byte utf-8
            size = 2;
            return i + 1 < data.Length && HasUtf8SupplementaryMarker(data[i + 1]);
        }
        size = 1;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Encoding DetectEncoding(string path)
    {
        var data = File.ReadAllBytes(path);
        // utf-8 BOM
        if (data.Length >= 3 && data[0] == 239 && data[1] == 187 && data[2] == 191)
        {
            return Encoding.UTF8;
        }
        // utf-16 BE BOM
        if (data.Length >= 2 && data[0] == 254 && data[1] == 255)
        {
            return Encoding.BigEndianUnicode;
        }
        // utf-16 LE BOM
        if (data.Length >= 2 && data[0] == 255 && data[1] == 254)
        {
            return Encoding.Unicode;
        }
        var i = 0;
        while (i < data.Length)
        {
            // is valid utf8 character?
            if (!IsValidUtf8(data, i, out var size))
            {
                return Iso88592;
            }
            i += size;
        }
        return Utf8NoBOM;
    }
}
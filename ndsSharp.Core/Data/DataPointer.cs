using System.Runtime.InteropServices;

namespace ndsSharp.Core.Data;

[StructLayout(LayoutKind.Sequential)]
public struct DataPointer
{
    public int Offset;
    public int Length;
    
    public DataPointer(BaseReader reader, DataPointerType pointerType = DataPointerType.OffsetLength)
    {
        switch (pointerType)
        {
            case DataPointerType.OffsetLength:
            {
                Offset = reader.Read<int>();
                Length = reader.Read<int>();
                break;
            }
            case DataPointerType.StartEnd:
            {
                Offset = reader.Read<int>();
                
                var endOffset = reader.Read<int>();
                Length = endOffset - Offset;
                break;
            }
        }
    }
}

public enum DataPointerType
{
    OffsetLength,
    StartEnd
}
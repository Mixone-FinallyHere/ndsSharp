using ndsSharp.Core.Data;
using ndsSharp.Core.Objects;
using ndsSharp.Core.Objects.Exports;
using ndsSharp.Core.Objects.Files;
using ndsSharp.Core.Objects.Rom;
using Serilog;

namespace ndsSharp.Core.Providers;

public class NdsFileProvider : IFileProvider
{
    public Dictionary<string, RomFile> Files { get; set; } = [];
    
    public RomHeader Header;

    private AllocationTable _allocationTable;
    private NameTable _nameTable;
    
    private BaseReader _reader;
    
    public NdsFileProvider(FileInfo romFile) : this(romFile.FullName)
    {
    }

    public NdsFileProvider(string filePath)
    {
        _reader = new BaseReader(File.ReadAllBytes(filePath));
    }

    public void Initialize()
    {
        Header = new RomHeader(_reader);

        _allocationTable = new AllocationTable(_reader.LoadPointer(Header.FatPointer));
        _nameTable = new NameTable(_reader.LoadPointer(Header.FntPointer));
        
        Mount(_allocationTable, _nameTable);
    }

    protected void Mount(AllocationTable allocationTable, NameTable nameTable)
    {
        for (ushort id = 0; id < allocationTable.Pointers.Count; id++)
        {
            var pointer = allocationTable.Pointers[id];
            if (pointer.Length <= 0) continue;
            
            if (id >= nameTable.FirstId)
            {
                var fileName = nameTable.FilesById[id];
                if (!fileName.Contains('.')) // detect extension
                {
                    var extension = _reader.PeekString(4, pointer.Offset).ToLower();
                    if (FileTypeRegistry.Contains(extension))
                    {
                        fileName += $".{extension}";
                    }
                    else
                    {
                        fileName += ".bin";
                    }
                }

                Files[fileName] = new RomFile(fileName, pointer);
            }
            else
            {
                var fileName = $"overlays/{id}.bin";
                Files[fileName] = new RomFile(fileName, pointer);
            }
        }
    }
    
    public T LoadObject<T>(string path) where T : BaseDeserializable, new() => LoadObject<T>(Files[path]);
    
    public T LoadObject<T>(RomFile file) where T : BaseDeserializable, new() => CreateReader(file).ReadObject<T>();
    
    public bool TryLoadObject<T>(string path, out T data) where T : BaseDeserializable, new() => TryLoadObject(Files[path], out data);
    
    public bool TryLoadObject<T>(RomFile file, out T data) where T : BaseDeserializable, new()
    {
        data = null!;
        try
        {
            data = LoadObject<T>(file);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            return false;
        }
    }

    public BaseReader CreateReader(RomFile file)
    {
        return _reader.LoadPointer(file.Pointer);
    }
    
    public BaseReader CreateReader(string path) => CreateReader(Files[path]);
}
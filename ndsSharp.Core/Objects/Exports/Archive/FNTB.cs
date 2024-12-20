using ndsSharp.Core.Data;
using ndsSharp.Core.Objects.Files;

namespace ndsSharp.Core.Objects.Exports.Archive;

public class FNTB : NdsBlock
{
    public Dictionary<ushort, string> FilesById = new();
    public ushort FirstId;
    
    public override string Magic => "FNTB";

    public override void Deserialize(BaseReader reader)
    {
        base.Deserialize(reader);

        var nameTable = new NameTable(reader);
        FilesById = nameTable.FilesById;
        FirstId = nameTable.FirstId;
    }
}
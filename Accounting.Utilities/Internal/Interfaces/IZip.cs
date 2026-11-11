using Accounting.Entities.Internal;

namespace Accounting.Utilities.Internal.Interfaces
{
    public interface IZip
    {
        public byte[]? Compress(List<DocumentFileInfo> documentFileInfoList);
    }
}

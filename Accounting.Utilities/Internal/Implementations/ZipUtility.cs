using Accounting.Entities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using System.IO.Compression;

namespace Accounting.Utilities.Internal.Implementations
{
    public class ZipUtility : IZip
    {
        public byte[]? Compress(List<DocumentFileInfo> documentFileInfoList)
        {
            var compressedFileStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, true))
            {
                foreach (var deliveryFile in documentFileInfoList)
                {
                    var zipEntry = zipArchive.CreateEntry(deliveryFile.Name);

                    using (var originalFileStream = new MemoryStream(deliveryFile.Bytes))
                    using (var zipEntryStream = zipEntry.Open())
                    {
                        originalFileStream.CopyTo(zipEntryStream);
                    }
                }
            }
            return compressedFileStream.ToArray();
        }
    }
}

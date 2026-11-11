using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal
{
    public class DocumentFileInfo
    {
        public string Name { get; set; }
        public string Base64String { get; set; }
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public byte FileType_Id { get; set; }
        public long Agent_Id {  get; set; }
    }
}

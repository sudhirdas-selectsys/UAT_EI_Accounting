using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public  class TrustedDevice
    {
        public long TrustedDeviceId { get; set; }
        public long UserId { get; set; }
        public Guid DeviceId { get; set; }
        public byte[] TokenHash { get; set; }           // 32 bytes SHA-256
        public string MachineName { get; set; }

        public string Browser { get; set; }
        public string DeviceLabel { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool Status { get; set; }
        public long CreatedBy { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class UserOtpVerification
    {
        public long OtpId { get; set; }
        public int UserId { get; set; }
        public string OtpHash { get; set; }
        public string OtpType { get; set; }  // "Email" or "Phone"
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class CommissionRules : BaseEntity
    {
        public long CommissionRule_Id { get; set; }
        public int EntityType_Id { get; set; }
        public long Entity_Id { get; set; }
        public string Entity { get; set; } = string.Empty;
        public int? SubmissionType_Id { get; set; }
        public string? SubmissionType { get; set; }
        public string? SubmissionTypeIds { get; set; } = string.Empty;
        public int CommissionType_Id { get; set; }
        public string CommissionType { get; set; }
        public decimal? Value { get; set; }
        public bool? ApplyOnAllTransactions { get; set; }
        public int CommissionBasisType_Id { get; set; }
        public string CommissionBasisType { get; set; }
        public byte? LOB_Id { get; set; }
        public string LOB_Name { get; set; }
        public decimal? Referral_Commission_SplitPct { get; set; }

        public List<int> SubmissionType_Ids { get; set; } = new();
    }

}

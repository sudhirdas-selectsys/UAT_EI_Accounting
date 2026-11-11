using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class EntityDetails : BaseEntity
    {
        public long Entity_Id { get; set; }
        public string EntityName { get; set; }
        public string EntityAddress { get; set; }
        public string EntityEmail { get; set; }
        public string Email { get; set; }
        public string EntityCity { get; set; }
        public string EntityState { get; set; }
        public string EntityZipCode { get; set; }
        public string EntityTypeName { get; set; }
    }
}

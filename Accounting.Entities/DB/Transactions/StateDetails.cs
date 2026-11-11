using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class StateDetails :BaseEntity
    {
        public int StateDetail_Id { get; set; }
        public long Client_Id { get; set; }
        public string Name { get; set; }
    }
}

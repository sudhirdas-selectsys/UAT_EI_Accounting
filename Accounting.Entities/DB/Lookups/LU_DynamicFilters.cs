using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_DynamicFilters
    {
        public int DynamicFilter_Id { get; set; }
        public int SQLQuery_Id { get; set; }
        public string ExpressionIdentifier { get; set; }
        public bool Add_WhereClause { get; set; }
        public bool Add_HavingClause { get; set; }
    }
}

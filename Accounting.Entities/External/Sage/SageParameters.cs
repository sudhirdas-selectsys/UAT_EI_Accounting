using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.Sage
{
    public class SageParameters
    {
        public string BaseURL { get; set; }
        public string Company_Id { get; set; }
        public string Sender_Id { get; set; }
        public string Client_Id { get; set; }
        public string Sender_Password { get; set; }
        public string User_Id { get; set; }
        public string User_Password { get; set; }
    }
}

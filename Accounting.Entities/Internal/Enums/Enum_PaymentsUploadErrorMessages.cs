using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_PaymentsUploadErrorMessages
    {
        public Enum_PaymentsUploadErrorMessages()
        {
            Required = "Please enter a value.";
            InvalidData = "Invalid format. Please check your input.";
        }

        public string Required { get; set; }
        public string InvalidData { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DummyPayIntegration.Models
{
    public class Payment3DS
    {
        public string MD { get; set; }
        public string TermUrl { get; set; }
        public string PaReq { get; set; }
    }
}

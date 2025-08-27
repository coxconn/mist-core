using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Framework.Config
{
    internal class EncryptorInfo
    {

        public string ASEKey { get; set; }
        public string ASEIV { get; set; }
        public string DESKey { get; set; }
        public string DESIV { get; set; }

    }
}

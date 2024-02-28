using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SagePayMvc.Internal
{
    public class ThreeDSecureTransactionRegistration
    {
        public ThreeDSecureTransactionRegistration(string mD, string paRes)
        {
            MD = mD;
            PARes = paRes;

        }

        public string MD
        {
            get;
            private set;
        }
        public string PARes
        {
            get;
            private set;
        }
    }
}

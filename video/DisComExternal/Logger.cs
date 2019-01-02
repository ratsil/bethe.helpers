using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTL.Merging
{
    partial class DisComExternal
    {
        class Logger : helpers.Logger
        {
            public Logger()
                : base("discom_external")
            { }
            public Logger(string sCategory)
                : base(sCategory)
            { }
        }
    }
}

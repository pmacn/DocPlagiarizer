using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocPlagiarizer.TestProject
{
    public struct TheStruct : TheInterface
    {
        public void Method(string argument) { throw new NotImplementedException(); }

        public string Property { get; set; }

        public event EventHandler TheEvent;
    }
}

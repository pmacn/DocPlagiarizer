using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocPlagiarizer.TestProject
{
    public class TheOtherClass : TheOtherInterface
    {
        public void Method(string argument) { throw new NotImplementedException(); }

        public int Property { get; set; }

        public class TheNestedClass : TheInterface
        {
            public void Method(string argument) { throw new NotImplementedException(); }

            public string Property { get; set; }

            public event EventHandler TheEvent
            {
                add { throw new NotImplementedException(); }
                remove { throw new NotImplementedException(); }
            }
        }
    }
}

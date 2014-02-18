using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulldownComments.TestProject
{
    public interface TheUndocumentedInterface
    {
        string Property { get; set; }

        void Method();

        event EventHandler Event;
    }
}

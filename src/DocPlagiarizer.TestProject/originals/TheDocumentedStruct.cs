using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulldownComments.TestProject
{
    /// <summary>
    /// Struct documentation
    /// </summary>
    public struct TheDocumentedStruct : TheUndocumentedInterface
    {
        /// <summary>
        /// Property documentation
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Method documentation
        /// </summary>
        public void Method()
        {
        }

        /// <summary>
        /// Event documentation
        /// </summary>
        public event EventHandler Event;
    }
}

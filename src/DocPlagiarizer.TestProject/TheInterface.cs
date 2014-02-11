using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocPlagiarizer.TestProject
{
    /// <summary>
    /// This is a type
    /// </summary>
    public interface TheInterface
    {
        /// <summary>
        /// This method does things
        /// </summary>
        /// <param name="argument">This is the argument that things are done with.</param>
        void Method(string argument);

        /// <summary>
        /// This is a property, it's a string.
        /// </summary>
        string Property { get; set; }

        /// <summary>
        /// Don't think about The Event. And Please REMAIN INDOORS!
        /// </summary>
        event EventHandler TheEvent;
    }
}

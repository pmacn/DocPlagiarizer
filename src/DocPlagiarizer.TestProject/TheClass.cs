using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocPlagiarizer.TestProject
{
    /// <summary>
    /// This is a type
    /// </summary>
    public class TheClass : TheInterface
    {
        /// <summary>
        /// This method does things
        /// </summary>
        /// <param name="argument">This is the argument that things are done with.</param>
        public void Method(string argument) { throw new NotImplementedException(); }

        /// <summary>
        /// This is a property, it's a string.
        /// </summary>
        public string Property { get; set; }
    }
}

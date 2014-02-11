using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocPlagiarizer.TestProject
{
    /// <summary>
    /// This is another type
    /// </summary>
    public interface TheOtherInterface
    {
        /// <summary>
        /// This method does some things
        /// </summary>
        /// <param name="argument">This argument is used in doing things.</param>
        void Method(string argument);

        /// <summary>
        /// This is a property, it's an int.
        /// </summary>
        int Property { get; set; }
    }
}

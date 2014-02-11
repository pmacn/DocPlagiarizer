using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocPlagiarizer.TestProject
{
    /// <summary>
    /// This is another type
    /// </summary>
    public class TheOtherClass : TheOtherInterface
    {
        /// <summary>
        /// This method does some things
        /// </summary>
        /// <param name="argument">This argument is used in doing things.</param>
        public void Method(string argument) { throw new NotImplementedException(); }

        /// <summary>
        /// This is a property, it's an int.
        /// </summary>
        public int Property { get; set; }

        /// <summary>
        /// This is a type
        /// </summary>
        public class TheNestedClass : TheInterface
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
}

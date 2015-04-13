using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.extensions
{
    public static class Assertions
    {
        public class AssertionException : Exception
        {
            public AssertionException() : base("Assertion Failed!") { }
        }

        public static void assertEquals<T>(this T compareThis, T compareTo)
        {
            var comparision = compareThis.Equals(compareTo);
            if (comparision == false)
                throw new AssertionException();
        }

        public static void assert(this bool comparision)
        {
            if (!comparision)
                throw new AssertionException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess.entries
{
    public class OutputEntry<TIn, TOut> : DataEntry<TOut>
    {
        public OutputEntry(DataEntry<TIn> input, long id, TOut data)
            : base(id, data)
        {
            this.Input = input;
        }
        public DataEntry<TIn> Input { get; private set; }
    }
}

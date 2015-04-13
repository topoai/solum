using solum.core.dataprocess.entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace solum.core.dataprocess.activities
{
    public class GatherActivity<T> : DataActivity<T, T[]>
    {
        public GatherActivity(DataProcess process, int size) : base(process)
        {
            this.Block = new BatchBlock<T>(size);
        }

        public override OutputEntry<T, T[]> ProcessEntry(DataEntry<T> entry)
        {
            throw new NotImplementedException();
        }
    }
}

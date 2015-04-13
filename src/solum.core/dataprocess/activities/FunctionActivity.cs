using solum.core.dataprocess.entries;
using solum.core.dataprocess.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess.activities
{
    public class FunctionActivity<TInput, TOutput> : DataActivity<TInput, TOutput>
    {
        public FunctionActivity(DataProcess process, Func<TInput, TOutput> function) : base(process)
        {
            this.m_function = function;
        }

        Func<TInput, TOutput> m_function;

        public override OutputEntry<TInput, TOutput> ProcessEntry(DataEntry<TInput> input)
        {
            var data = input.Data;
            var result = m_function(data);

            var id = Process.NextEntryId();
            var output = new OutputEntry<TInput, TOutput>(input, id, result);

            return output;
        }
    }
}

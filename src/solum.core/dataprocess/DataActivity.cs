using System;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using solum.core.dataprocess.entries;
using solum.core.dataprocess.interfaces;

namespace solum.core.dataprocess
{
    public abstract class DataActivity<TInput, TOutput> : Component, IDataActivity
    {
        public DataActivity(DataProcess dataProcess)
        {
            this.Process = dataProcess;
        }

        public DataProcess Process { get; private set; }

        /// <summary>
        /// Internal dataflow block used to chain activities together
        /// </summary>
        internal IPropagatorBlock<TInput, TOutput> Block { get; set; }

        public abstract OutputEntry<TInput, TOutput> ProcessEntry(DataEntry<TInput> entry);

        IDataEntry IDataActivity.ProcessEntry(IDataEntry entry)
        {
            var input = entry as DataEntry<TInput>;
            if (input == null)
                throw new ArgumentNullException();

            var output = ProcessEntry(input);
            return output;
        }

        IDataProcess IDataActivity.Process
        {
            get { return this.Process; }
        }
    }
}

using solum.core.dataprocess.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess
{
    public class DataEntry<T> : IDataEntry
    {
        public DataEntry(long id, T data)
        {
            this.Data = data;
        }

        public long Id { get; private set; }
        public T Data { get; private set; }
    }
}

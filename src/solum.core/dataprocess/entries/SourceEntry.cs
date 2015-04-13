using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess.entries
{
    public class SourceEntry<T> : DataEntry<T>
    {
        public SourceEntry(DataSource<T> source, long id, T data)
            : base(id, data)
        {
            this.m_source = source;
        }

        DataSource<T> m_source;
    }
}

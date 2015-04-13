using solum.core.dataprocess.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess
{
    public class DataSource<T> : IDataSource
    {
        public DataSource(DataProcess process, IEnumerable<T> data)
        {
            this.Process = process;
            this.m_data = data;            
        }
        public DataProcess Process { get; private set; }

        IEnumerable<T> m_data;        

        public IEnumerable<T> Read()
        {
            foreach (var data in m_data)
                yield return data;
        }

        IDataProcess IDataSource.Process
        {
            get { return this.Process; }
        }
    }
}

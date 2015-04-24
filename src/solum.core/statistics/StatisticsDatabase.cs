using solum.core.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.statistics
{
    public class StatisticsDatabase
    {
        public StatisticsDatabase(KeyValueStore storage)
        {
            this.m_storage = storage;
        }

        KeyValueStore m_storage;        

        public void Set<TValue>(string name, TValue value)
        {
            m_storage.Set(name, value);
        }

        public void Increment(string name)
        {
            int currentValue;
                        
            if (!m_storage.Get(name, out currentValue))
                currentValue = 0;

            m_storage.Update(name, currentValue + 1);
        }
        public void IncrementL(string name)
        {
            long currentValue;

            if (!m_storage.Get(name, out currentValue))
                currentValue = 0;

            m_storage.Update(name, currentValue + 1);            
        }
    }
}

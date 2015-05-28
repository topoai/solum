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

        public void SetValue(string name, long value)
        {
            m_storage.Set(name, value.ToString());
        }
        public bool GetValue(string name, out long value)
        {
            value = 0;
            // TODO: Read Write Lock needs to happen here around Get and Update statements
            string sValue;

            if (m_storage.Get(name, out sValue) == false)
                return false;

            value = long.Parse(sValue);
            return true;
        }

        #region Increment a Counter
        public void Increment(string name)
        {            
            long currentValue;

            if (!m_storage.Get(name, out currentValue))
                currentValue = 0;

            //m_storage.Update(name, currentValue + 1);            
            m_storage.Set(name, currentValue + 1);            
        }
        public void Increment(string name, out long value)
        {
            long currentValue;

            // TODO: Read Write Lock needs to happen here around Get and Update statements
            if (!m_storage.Get(name, out currentValue))
                currentValue = 0;

            var newValue = currentValue + 1;
            //m_storage.Update(name, newValue);
            m_storage.Set(name, currentValue + 1);            

            value = newValue;
        }
        public void Increment(string name, out int value)
        {
            long newValue;

            Increment(name, out newValue);

            if (newValue > Int16.MaxValue)
                throw new IndexOutOfRangeException("The key {0} was incremented but the value {1} is too large to return as an integer (Int32)".format(name, newValue));

            value = (int)newValue;
        }
        #endregion
    }
}

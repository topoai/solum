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
            m_storage.Set(name, value);
        }
        public bool GetValue(string name, out long value)
        {
            // TODO: Read Write Lock needs to happen here around Get and Update statements
            return !m_storage.Get(name, out value);
        }

        #region Increment a Counter
        public void Increment(string name)
        {
            long currentValue;

            if (!m_storage.Get(name, out currentValue))
                currentValue = 0;

            m_storage.Update(name, currentValue + 1);            
        }
        public void Increment(string name, out long value)
        {
            long currentValue;

            // TODO: Read Write Lock needs to happen here around Get and Update statements
            if (!m_storage.Get(name, out currentValue))
                currentValue = 0;

            var newValue = currentValue + 1;
            m_storage.Update(name, newValue);

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

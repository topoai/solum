using solum.core.storage;
using solum.extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.core.stats
{
    /// <summary>
    /// A statistic that is stored in the database
    /// 
    /// TODO: Create a struct specialized version that doesn't use locking
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class DBStatistic<TValue> : Statistic<TValue>
    {
        protected DBStatistic(KeyValueStore statisticsDB, string name, TValue initialValue)
            : base(name)
        {
            this.m_statitisticsDB = statisticsDB;
            this.m_currentValue = initialValue;
        }

        KeyValueStore m_statitisticsDB;
        long m_currentValueRecordId;
        TValue m_currentValue;
        ReaderWriterLockSlim m_dataReadWriteLock = new ReaderWriterLockSlim();

        #region Current Value
        public override TValue GetCurrentValue()
        {
            lock (new ReaderLocker(m_dataReadWriteLock))
            {
                if (m_currentValue == null)
                {
                    // ** Read 
                }

                return m_currentValue;
            }
        }
        #endregion

        #region Updates
        protected override void OnUpdateCurrentValue(TValue newValue)
        {
            // ** Write the new value to the underlying storage
            lock (new WriteLocker(m_dataReadWriteLock))
            {
                // var bytes = newValue.ToBytes();
                throw new NotImplementedException();

                m_currentValue = newValue;
            }
        }
        #endregion

        #region Historical Values
        public override TValue[] GetHistoricalValues()
        {
            throw new NotImplementedException();
        }
        public override TValue[] GetHistoricalValues(DateTime since)
        {
            throw new NotImplementedException();
        }
        public override TValue[] GetHistoricalValues(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }
        public override TValue[] GetHistoricalValues(int take)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

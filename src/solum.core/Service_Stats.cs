using solum.core.stats;
using solum.core.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    partial class Service_Stats
    {
        KeyValueStore m_statsDB;


        public Statistic<TValue> GetStatistic<TValue>(string name)
        {
            throw new NotImplementedException();
        }

        protected void UpdateStatistic<TValue>(string name, TValue value)
        {
            var statistic = GetStatistic<TValue>(name);
            statistic.UpdateCurrentValue(value);
        }
    }
}

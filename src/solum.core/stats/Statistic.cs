using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.stats
{
    /// <summary>
    /// Generic Abstract implementation to use as a base for most statistic backing implementations.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class Statistic<TValue> : IStatistic
    {
        /// <summary>
        /// Constructor used for deserialization
        /// </summary>
        Statistic() { }

        protected Statistic(string name)
        {
            this.Name = name;
        }

        #region Events
        public event EventHandler<TValue> OnUpdateValue;
        void FireOnUpdateValue(TValue value)
        {
            if (OnUpdateValue != null)
                OnUpdateValue(this, value);
        }
        #endregion

        [JsonProperty("$name")]
        public string Name { get; private set; }
        public void UpdateCurrentValue(TValue value)
        {
            OnUpdateCurrentValue(value);
            FireOnUpdateValue(value);
        }
        protected abstract void OnUpdateCurrentValue(TValue value);
        public abstract TValue GetCurrentValue();
        public abstract TValue[] GetHistoricalValues();
        public abstract TValue[] GetHistoricalValues(DateTime since);
        public abstract TValue[] GetHistoricalValues(DateTime start, DateTime end);
        public abstract TValue[] GetHistoricalValues(int take);        

        #region Explicit Implementation        
        object IStatistic.GetCurrentValue()
        {
            return GetCurrentValue();
        }
        object[] IStatistic.GetHistoricalValues()
        {
            return GetHistoricalValues().Cast<object>().ToArray();
        }
        object[] IStatistic.GetHistoricalValues(DateTime since)
        {
            return GetHistoricalValues(since).Cast<object>().ToArray();
        }
        object[] IStatistic.GetHistoricalValues(DateTime start, DateTime end)
        {
            return GetHistoricalValues(start, end).Cast<object>().ToArray();
        }
        object[] IStatistic.GetHistoricalValues(int take)
        {
            return GetHistoricalValues(take).Cast<object>().ToArray();
        }        
        #endregion
    }
}

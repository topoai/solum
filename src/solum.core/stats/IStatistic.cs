using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.stats
{
    public interface IStatistic
    {
        string Name { get; }
        object GetCurrentValue();
        object[] GetHistoricalValues();
        object[] GetHistoricalValues(DateTime since);
        object[] GetHistoricalValues(DateTime start, DateTime end);
        object[] GetHistoricalValues(int take);
    }
}

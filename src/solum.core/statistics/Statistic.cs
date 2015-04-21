using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.statistics
{
    public abstract class Statistic : NamedComponent, IStatistic
    {
        protected Statistic(string name, Type valueType)
            : base(name)
        {
            this.ValueType = valueType;
        }

        public Type ValueType { get; private set; }
    }

    public class Statistic<TValue> : Statistic
        where TValue : struct
    {
        public Statistic(string name) : this(name, default(TValue)) { }

        public Statistic(string name, TValue initialValue)
            : base(name, typeof(TValue))
        {
            this.Value = initialValue;
        }

        public TValue Value { get; private set; }
    }
}

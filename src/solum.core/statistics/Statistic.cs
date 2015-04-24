using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.statistics
{
    /// <summary>
    /// Base class that provides a NamedComponent implementation of a statistic
    /// </summary>
    public class Statistic : NamedComponent, IStatistic
    {
        /// <summary>
        /// Used for Deserialization
        /// </summary>
        protected Statistic() { }
        protected Statistic(string name, Type valueType)
            : base(name)
        {
            this.ValueType = valueType;
        }

        public Type ValueType { get; protected set; }

        public abstract byte[] ToBytes();
    }
}

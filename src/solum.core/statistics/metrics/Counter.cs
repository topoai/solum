using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.statistics.metrics
{
    public class Counter : Statistic
    {
        /// <summary>
        /// Used for deserializers
        /// </summary>
        protected Counter() { }

        public Counter(string name, long initialValue = 0) : base(name, typeof(long))
        {
            this.Value = initialValue;
        }

        public long Value { get; protected set; }

        public override byte[] ToBytes()
        {
            var memoryStream = new MemoryStream();

            throw new NotImplementedException();
        }
    }
}

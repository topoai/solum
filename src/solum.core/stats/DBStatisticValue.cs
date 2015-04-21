using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.stats
{
    public abstract class DBStatisticValue<TValue>
    {
        public byte[] ToBytes()
        {
            // TODO: Add error handling
            var bytes = OnToBytes();

            return bytes;
        }
        protected abstract byte[] OnToBytes();

        public void FromBytes()
        {
            OnFromBytes();
        }
        protected abstract void OnFromBytes();
    }
}

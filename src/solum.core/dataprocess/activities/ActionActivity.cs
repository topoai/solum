using solum.core.dataprocess.entries;
using solum.core.dataprocess.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess.activities
{
    public class ActionActivity<T> : DataActivity<T, T>
    {
        public ActionActivity(DataProcess dataProcess, Action<T> action)
            : base(dataProcess)
        {
            m_action = action;
        }

        Action<T> m_action;

        public override OutputEntry<T, T> ProcessEntry(DataEntry<T> input)
        {
            var data = input.Data;
            m_action(data);

            var id = Process.NextEntryId();
            var output = new OutputEntry<T, T>(input, id, data);

            return output;
        }
    }
}

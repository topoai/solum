using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.dataprocess.interfaces
{
    public interface IDataProcess
    {
        long NextEntryId();
        void Run();
    }
}

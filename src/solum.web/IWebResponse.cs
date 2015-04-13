using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web
{
    public interface IWebResponse
    {
        string ContentType { get; }
        long ContentLength { get; }
        Stream Content { get; }
    }
}

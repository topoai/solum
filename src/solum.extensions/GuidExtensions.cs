using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.extensions
{
    public static class GuidExtensions
    {
        public const int GUID_SIZE_OF = 16; // 16 byte array

        public static Guid ReadGuid(Stream stream)
        {
            var buffer = new byte[GUID_SIZE_OF];

            var bytesRead = 0;
            while (bytesRead < buffer.Length)
            {
                bytesRead = stream.Read(buffer, bytesRead, buffer.Length);
            }

            var guid = new Guid(buffer);
            return guid;
        }
    }
}

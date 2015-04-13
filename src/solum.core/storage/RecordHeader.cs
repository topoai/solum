using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    public class RecordHeader
    {
        public const int SIZE_OF = sizeof(long) + sizeof(long) + sizeof(int) + sizeof(bool);

        public RecordHeader(long id, long offset, int length, bool isDeleted = false)
        {
            this.Id = id;
            this.Offset = offset;
            this.Length = length;
            this.IsDeleted = isDeleted;
        }

        public bool IsDeleted { get; private set; }
        public long Id { get; private set; }
        public long Offset { get; private set; }
        public int Length { get; private set; }        

        public void Write(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, SystemSettings.Encoding, leaveOpen: true))
                Write(writer);
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(IsDeleted);
            writer.Write(Id);
            writer.Write(Offset);
            writer.Write(Length);            
        }

        public static RecordHeader Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream, SystemSettings.Encoding, leaveOpen: true))            
                return Read(reader);                        
        }        
        public static RecordHeader Read(BinaryReader reader)
        {
            var isDeleted = reader.ReadBoolean();
            var id = reader.ReadInt64();
            var offset = reader.ReadInt64();
            var length = reader.ReadInt32();

            return new RecordHeader(id, offset, length, isDeleted);
        }
    }
}

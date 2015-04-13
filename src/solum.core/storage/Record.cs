using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    public class Record
    {
        public Record(long id, byte[] data)
            : this(id, data, isDeleted: false)
        {

        }

        public Record(long id, byte[] data, bool isDeleted)
        {
            this.Id = id;
            this.IsDeleted = isDeleted;
            this.Data = data;
        }

        #region Properties
        public long Id { get; private set; }
        public bool IsDeleted { get; private set; }
        [JsonIgnore]
        public byte[] Data { get; private set; }
        public string DataString
        {
            get { return SystemSettings.Encoding.GetString(Data); }
        }
        #endregion

        public int SizeOf
        {
            get
            {
                return sizeof(long) // ID
                     + sizeof(bool) // IsDeleted
                     + sizeof(int) // Length
                     + Data.Length; // Data
            }
        }

        public void Write(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, SystemSettings.Encoding, leaveOpen: true))
                Write(writer);
        }
        public void Write(BinaryWriter writer)
        {
            // ** Write the raw  data
            writer.Write(IsDeleted);
            writer.Write(Id);
            writer.Write(Data.Length);
            writer.Write(Data, 0, Data.Length);
        }

        public static Record Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream, SystemSettings.Encoding, leaveOpen: true))
            {
                return Read(reader);
            }
        }
        public static Record Read(BinaryReader reader)
        {
            var isDeleted = reader.ReadBoolean();
            var id = reader.ReadInt64();
            var length = reader.ReadInt32();
            var data = reader.ReadBytes(length);

            return new Record(id, data, isDeleted);
        }
    }
}

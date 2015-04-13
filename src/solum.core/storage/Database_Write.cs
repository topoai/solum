using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    partial class Database
    {
        public Record Store(string data)
        {
            var bytes = SystemSettings.Encoding.GetBytes(data);
            return Store(bytes);
        }
        public Record Store(byte[] data)
        {
            using (headerWriteLock)
            using (dataWriteLock)
            {
                // TODO: Aquire locks appropriately
                // var id = Interlocked.Increment(ref numRecords);
                // var fileLength = Interlocked.Add(ref dataLength, data.Length);

                var newNumRecords = numRecords + 1; 
                var id = newNumRecords;                

                // Set the position of this record to the current length of this files
                var dataPosition = dataLength; // (end of the file)
                var header = new RecordHeader(id, dataPosition, data.Length);
                var record = new Record(id, data);

                // Log.Trace("Writing record #{0} - {1} bytes", header.Id, header.Length);
                // Position data stream at the end
                dataStream.Position = DataPositions.DATA_OFFSET + dataPosition;
                record.Write(dataAppender);

                // Determine what the new length of the data file should be
                // if all writes are successfull
                var newDataLength = dataLength + record.SizeOf;

                // Update the data file meta data
                dataMetaData.Write(DataPositions.NUM_RECORDS_POS, newNumRecords);
                dataMetaData.Write(DataPositions.DATA_LENGTH_POS, newDataLength);

                // Position the header stream at the end
                var headerPosition = getHeaderPosition(id);
                headerStream.Position = headerPosition;
                header.Write(headerAppender);

                // Update the header file meta data
                headerMetaData.Write(HeaderPositions.NUM_RECORDS_POS, newNumRecords);

                // ** Increment the values since we are successful
                // TODO: If we were not successful, we should reposition the append cursor to where we started
                numRecords = id;
                dataLength = newDataLength;

                return record;
            }
        }
        public void Delete(long id)
        {
            using (headerWriteLock)
            using (dataWriteLock)
            {
                // ** Read the header
                var headerPosition = getHeaderPosition(id);
                headerStream.Position = headerPosition;
                using (var writer = new BinaryWriter(headerStream, SystemSettings.Encoding, leaveOpen: true))
                    writer.Write(true);

                // ** Get the data position from the header
                headerStream.Position += sizeof(long); // Skip the id
                long recordOffset;
                using (var reader = new BinaryReader(headerStream, SystemSettings.Encoding, leaveOpen: true))
                    recordOffset = reader.ReadInt64();

                dataStream.Position = DataPositions.DATA_OFFSET + recordOffset;
                using (var writer = new BinaryWriter(dataStream, SystemSettings.Encoding, leaveOpen: true))
                    writer.Write(true);
            }
        }
    }
}

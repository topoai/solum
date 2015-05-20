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
        /// <summary>
        /// Store a string currentValue as a new record
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Record Store(string data, bool autoFlush = false)
        {
            var bytes = SystemSettings.Encoding.GetBytes(data);
            return Store(bytes, autoFlush);
        }
        /// <summary>
        /// Store some binary data as a new record
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Record Store(byte[] data, bool autoFlush = false)
        {
            using (headerWriteLock)
            using (dataWriteLock)
            {
                // var id = Interlocked.Increment(ref m_numRecords);                
                var newNumRecords = m_numRecords + 1;
                var id = newNumRecords;

                // SetValue the position of this record to the current length of this files
                // var m_dataLength = Interlocked.Add(ref m_dataLength, data.Length);
                var dataPosition = m_dataLength; // (end of the file)
                var header = new RecordHeader(id, dataPosition, data.Length);
                var record = new Record(id, data);

                // Log.Verbose("Writing record #{0} - {1} bytes", header.Id, header.Length);
                // Position data stream at the end
                m_dataStream.Position = DataPositions.DATA_OFFSET + dataPosition;
                record.Write(m_dataWriter);

                // Determine what the new length of the data file should be
                // if all writes are successfull
                var newDataLength = m_dataLength + record.SizeOf;

                // SetValue the data file meta data
                m_dataMetaData.Write(DataPositions.NUM_RECORDS_POS, newNumRecords);
                m_dataMetaData.Write(DataPositions.DATA_LENGTH_POS, newDataLength);

                // Position the header stream at the end
                var headerPosition = getHeaderPosition(id);
                m_headerStream.Position = headerPosition;
                header.Write(m_headerWriter);

                // SetValue the header file meta data
                m_headerMetaData.Write(HeaderPositions.NUM_RECORDS_POS, newNumRecords);

                // ** Increment the values since we are successful
                m_numRecords = newNumRecords;
                m_dataLength = newDataLength;

                if (autoFlush) {
                    m_dataStream.Flush();
                    m_headerStream.Flush();
                }

                return record;
            }
        }
        /// <summary>
        /// Mark a record as deleted by it's id
        /// </summary>
        /// <param name="id"></param>
        public void Delete(long id)
        {
            using (headerWriteLock)
            using (dataWriteLock)
            {
                // ** Read the header
                var headerPosition = getHeaderPosition(id);
                m_headerStream.Position = headerPosition;
                using (var writer = new BinaryWriter(m_headerStream, SystemSettings.Encoding, leaveOpen: true))
                    writer.Write(true);

                // ** Get the data position from the header
                m_headerStream.Position += sizeof(long); // Skip the id
                long recordOffset;
                using (var reader = new BinaryReader(m_headerStream, SystemSettings.Encoding, leaveOpen: true))
                    recordOffset = reader.ReadInt64();

                m_dataStream.Position = DataPositions.DATA_OFFSET + recordOffset;
                using (var writer = new BinaryWriter(m_dataStream, SystemSettings.Encoding, leaveOpen: true))
                    writer.Write(true);
            }
        }
        /// <summary>
        /// SetValue the underlying data for the record.
        /// 
        /// NOTE:
        /// Writing more data than was previously allocated is not allow.
        /// Writing less data is allowed.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public void Update(long id, byte[] data)
        {
            using (headerWriteLock)
            using (dataWriteLock)                
            {
                // ** Get the header which describes the posistion and length
                //    of the data block for specified the record id
                var header = ReadHeader(id);

                // ** Check if the data blocks are the same length
                if (data.Length > header.Length)
                    throw new ArgumentOutOfRangeException("Writing more data than was originally written is not supported.  Length must be <= {0}".format(header.Length));
                                                
                // ** Create a header and record using the previous data position
                var dataPosition = header.Offset;
                var newHeader = new RecordHeader(id, dataPosition, data.Length);
                var newRecord = new Record(id, data);

                // Log.Verbose("Writing record #{0} - {1} bytes", header.Id, header.Length);
                // Position the data stream
                m_dataStream.Position = DataPositions.DATA_OFFSET + dataPosition;
                newRecord.Write(m_dataWriter);

                var headerPosition = getHeaderPosition(id);
                m_headerStream.Position = headerPosition;
                header.Write(m_headerWriter);
            }
        }
    }
}

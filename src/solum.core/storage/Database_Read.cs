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
        #region iterators
        public IEnumerable<Record> Records(bool includeDeleted = false)
        {
            using (dataReadLock)
            {
                m_dataStream.Position = DataPositions.DATA_OFFSET;
                long recordCount = 0;
                var numRecords = NumRecords;
                using (var reader = new BinaryReader(m_dataStream, SystemSettings.Encoding, leaveOpen: true))
                {
                    while (recordCount < numRecords)
                    {
                        // ** Read the next Record
                        var record = Record.Read(reader);

                        if (includeDeleted || record.IsDeleted == false)
                            yield return record;

                        recordCount++;
                    }
                }
            }
        }
        public IEnumerable<RecordHeader> Headers(bool includeDeleted = false)
        {
            using (headerReadLock)
            {
                m_headerStream.Position = HeaderPositions.DATA_OFFSET;
                long recordCount = 0;
                var numRecords = NumRecords;
                using (var reader = new BinaryReader(m_headerStream, SystemSettings.Encoding, leaveOpen: true))
                {
                    while (recordCount < numRecords)
                    {
                        // ** Read the next Record
                        var header = RecordHeader.Read(reader);

                        if (includeDeleted || header.IsDeleted == false)
                            yield return header;

                        recordCount++;
                    }
                }
            }
        }
        #endregion

        RecordHeader ReadHeader(long id)
        {
            m_headerStream.Position = getHeaderPosition(id);

            var header = RecordHeader.Read(m_headerStream);
            return header;
        }
        public Record ReadRecord(long id)
        {
            //using (headerReadLock)
            //using (dataReadLock)
            {
                // ** Read the header
                var header = ReadHeader(id);

                // ** Reposition the data stream
                m_dataStream.Position = DataPositions.DATA_OFFSET + header.Offset;
                Log.Verbose("Reading record id={0} length={1}", id, header.Length);

                var record = Record.Read(m_dataStream);
                return record;
            }
        }
    }
}

using solum.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace solum.core.storage
{
    public partial class Database : Component
    {
        #region Constants
        public const long MAX_HEADER_FILE_SIZE = 1024 * 1024 * 100; // 1 GB
        public const long MAX_DATA_FILE_SIZE = 1024 * 1024 * 2000; // 20 GB

        static class DataPositions
        {
            public const int NUM_RECORDS_POS = 0;
            public const int NUM_RECORDS_SIZE = sizeof(UInt32);
            public const int DATA_LENGTH_POS = NUM_RECORDS_SIZE;
            public const int DATA_LENGTH_SIZE = sizeof(UInt64);
            public const int DATA_OFFSET = 1024;
        }
        static class HeaderPositions
        {
            public const int NUM_RECORDS_POS = 0;
            public const int DATA_OFFSET = 1024;
        }
        #endregion

        public Database(DirectoryInfo dataDirectory, string databaseName)
        {
            // ** Debug/Testing only - REMOVE!
            //if (File.Exists(dataFilePath))
            //    File.Delete(dataFilePath);

            //if (File.Exists(headerFilePath))
            //    File.Delete(headerFilePath);

            this.DataDirectory = dataDirectory;
            this.Name = databaseName;
            this.IsOpened = false;

            var dataFilePath = Path.Combine(DataDirectory.FullName, "{0}.dat".format(databaseName));
            var headerFilePath = Path.Combine(DataDirectory.FullName, "{0}.hdr".format(databaseName));
            this.dataFileInfo = new FileInfo(dataFilePath);
            this.headerFileInfo = new FileInfo(headerFilePath);
        }

        #region Public Properties
        public string Name { get; private set; }
        public long NumRecords { get { return numRecords; } }
        public bool IsOpened { get; private set; }
        public DirectoryInfo DataDirectory { get; private set; }
        #endregion        
        
        /// <summary>
        /// The total number of records stored in the database
        /// </summary>
        int numRecords;
        /// <summary>
        /// The length of the stored data in the data file.
        /// Note: This value is measured from the DATA_OFFSET, not the beginning of the file.
        /// </summary>
        long dataLength;

        #region File system resources
        FileInfo dataFileInfo;
        FileInfo headerFileInfo;

        MemoryMappedFile dataFile;
        MemoryMappedFile headerFile;

        MemoryMappedViewAccessor dataMetaData;
        MemoryMappedViewAccessor headerMetaData;

        MemoryMappedViewStream dataStream;
        MemoryMappedViewStream headerStream;

        BinaryWriter dataAppender;
        BinaryWriter headerAppender;
        #endregion

        /// <summary>
        /// Helper method to ensure the database is Open() before writing or reading
        /// </summary>
        void ensureOpened()
        {
            if (!IsOpened)
            {
                Log.Error("Database is not opened. name={0}", Name);
                throw new Exception("The database is not opened.");
            }
        }
        
        /// <summary>
        /// Calculates the position in the header file for a give id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        long getHeaderPosition(long id)
        {
            return HeaderPositions.DATA_OFFSET + RecordHeader.SIZE_OF * (id - 1);
        }
    }
}

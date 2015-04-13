using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    partial class Database : IDisposable
    
    {
        public void Open()
        {
            if (IsOpened)
            {
                Log.Debug("Database already opened...");
                return;
            }
                                
            //using (dataWriteLock)
            //using (headerWriteLock)
            {
                openDataResources();
                openHeaderResources();
            }

            IsOpened = true;    
        }
        public void Close()
        {
            if (IsOpened == false)
            {
                Log.Debug("Database is not opened...");
                return;
            }

            using (dataWriteLock)
            using (headerWriteLock)
            {
                // ** Indicate close immediately to prevent future writes/reads while we close resources
                IsOpened = false;

                closeDataResources();
                closeHeaderResources();
            }
        }

        private void openDataResources()
        {
            bool dataFileExits = dataFileInfo.Exists;

            if (dataFileExits == false)
            {
                // Fix for MONO as CreateFromFile (OpenOrCreate) mode doesn't work.
                FileStream fs = new FileStream(dataFileInfo.FullName, FileMode.CreateNew);
                fs.Seek(MAX_DATA_FILE_SIZE, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
                fs.Dispose();
            }

            Log.Debug("Opening data file...   {0}", dataFileInfo.FullName);
            //dataFile = MemoryMappedFile.CreateFromFile(dataFileInfo.FullName, FileMode.Open, "{0}-data".format(Name));
            dataFile = MemoryMappedFile.CreateFromFile(dataFileInfo.FullName, FileMode.Open, "{0}-data".format(Guid.NewGuid()));

            dataMetaData = dataFile.CreateViewAccessor(0, DataPositions.DATA_OFFSET);

            if (dataFileExits)
            {
                // ** Read num records and data size
                numRecords = dataMetaData.ReadInt32(DataPositions.NUM_RECORDS_POS);
                dataLength = dataMetaData.ReadInt64(DataPositions.DATA_LENGTH_POS);
            }
            else
            {
                dataMetaData.Write(DataPositions.NUM_RECORDS_POS, 0);
                dataMetaData.Write(DataPositions.DATA_LENGTH_POS, 0L);
            }

            dataStream = dataFile.CreateViewStream();

            // ** Open the stream that will store/append new writes
            //dataWriteStream = dataFile.CreateViewStream();
            //dataWriteStream.Seek(DataPositions.DATA_OFFSET + dataLength, SeekOrigin.Begin);

            //dataAppender = new BinaryWriter(dataWriteStream, SystemSettings.Encoding, leaveOpen: true);
            dataAppender = new BinaryWriter(dataStream, SystemSettings.Encoding, leaveOpen: true);
        }
        private void openHeaderResources()
        {
            Log.Debug("Opening header file... {0}", headerFileInfo.FullName);
            var headerFileExits = headerFileInfo.Exists;

            if (headerFileExits == false)
            {
                FileStream fs = new FileStream(headerFileInfo.FullName, FileMode.CreateNew);
                fs.Seek(MAX_HEADER_FILE_SIZE, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
                fs.Dispose();
            }

            //headerFile = MemoryMappedFile.CreateFromFile(headerFileInfo.FullName, FileMode.Open, "{0}-headers".format(Name));
            headerFile = MemoryMappedFile.CreateFromFile(headerFileInfo.FullName, FileMode.Open, "{0}-headers".format(Guid.NewGuid()));

            headerMetaData = headerFile.CreateViewAccessor(0, HeaderPositions.DATA_OFFSET);
            if (headerFileExits)
            {
                // ** Double check the headerfile and datafile are in check
                var headerNumRecords = headerMetaData.ReadInt64(HeaderPositions.NUM_RECORDS_POS);
                if (headerNumRecords != numRecords)
                    throw new Exception("Error: header file is out of sync with the data file.");
            }
            else
            {
                // ** Initialize the header file metadata                    
                headerMetaData.Write(HeaderPositions.NUM_RECORDS_POS, 0);
            }

            // ** Position header stream to append
            headerStream = headerFile.CreateViewStream();
            headerStream.Seek(getHeaderPosition(numRecords), SeekOrigin.Begin);

            // ** Open the stream that will store/append new writes
            //headerWriteStream = headerFile.CreateViewStream();
            //headerWriteStream.Seek(getHeaderPosition(numRecords), SeekOrigin.Begin);

            //headerAppender = new BinaryWriter(headerWriteStream, SystemSettings.Encoding, leaveOpen: true);
            headerAppender = new BinaryWriter(headerStream, SystemSettings.Encoding, leaveOpen: true);
        }        
        private void closeDataResources()
        {
            Log.Trace("Flushing data resources...");            
            dataAppender.Flush();
            dataMetaData.Flush();
            dataStream.Flush();
            //dataWriteStream.Flush();            

            Log.Trace("Disposing data resources...");
            if (dataAppender != null)
            {
                dataAppender.Dispose();
                dataAppender = null;
            }
            if (dataStream != null)
            {
                dataStream.Dispose();
                dataStream = null;
            }            
            if (dataMetaData != null)
            {
                dataMetaData.Dispose();
                dataMetaData = null;
            }
            //if (dataWriteStream != null)
            //{
            //    dataWriteStream.Dispose();
            //    dataWriteStream = null;
            //}            
            if (dataFile != null)
            {
                dataFile.Dispose();
                dataFile = null;
            }
        }
        private void closeHeaderResources()
        {
            Log.Trace("Flushing header resources...");            
            headerAppender.Flush();
            headerMetaData.Flush();
            headerStream.Flush();
            //headerWriteStream.Flush();

            Log.Trace("Closing header resources...");
            if (headerAppender != null)
            {
                headerAppender.Dispose();
                headerAppender = null;
            }
            if (headerStream != null)
            {
                headerStream.Dispose();
                headerStream = null;
            }            
            if (headerMetaData != null)
            {
                headerMetaData.Dispose();
                headerMetaData = null;
            }
            //if (headerWriteStream != null)
            //{
            //    headerWriteStream.Dispose();
            //    headerWriteStream = null;
            //}            
            if (headerFile != null)
            {
                headerFile.Dispose();
                headerFile = null;
            }
        }

        #region IDisposable
        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion
    }
}

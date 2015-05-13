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
            bool dataFileExits = m_dataFileInfo.Exists;

			if (dataFileExits == false) {
				Log.Debug ("Creating a new data file... {0}", m_dataFileInfo.FullName);
				// Fix for MONO as CreateFromFile (OpenOrCreate) mode doesn't work.
				FileStream fs = new FileStream (m_dataFileInfo.FullName, FileMode.CreateNew);
				fs.Seek (MAX_DATA_FILE_SIZE, SeekOrigin.Begin);
				fs.WriteByte (0);
				fs.Close ();
				fs.Dispose ();
			} else {
				Log.Trace ("Using existing data file... {0}", m_dataFileInfo.FullName);
			}

            Log.Debug("Opening data file...   {0}", m_dataFileInfo.FullName);
            //m_dataFile = MemoryMappedFile.CreateFromFile(m_dataFileInfo.FullName, FileMode.Open, "{0}-data".format(Name));
            m_dataFile = MemoryMappedFile.CreateFromFile(m_dataFileInfo.FullName, FileMode.Open, "{0}-data".format(Guid.NewGuid()));

            m_dataMetaData = m_dataFile.CreateViewAccessor(0, DataPositions.DATA_OFFSET);

            if (dataFileExits)
            {
                // ** Read num records and data size
                m_numRecords = m_dataMetaData.ReadInt32(DataPositions.NUM_RECORDS_POS);
                m_dataLength = m_dataMetaData.ReadInt64(DataPositions.DATA_LENGTH_POS);
            }
            else
            {
                m_dataMetaData.Write(DataPositions.NUM_RECORDS_POS, 0);
                m_dataMetaData.Write(DataPositions.DATA_LENGTH_POS, 0L);
            }

            m_dataStream = m_dataFile.CreateViewStream();

            // ** Open the stream that will store/append new writes
            //dataWriteStream = m_dataFile.CreateViewStream();
            //dataWriteStream.Seek(DataPositions.DATA_OFFSET + m_dataLength, SeekOrigin.Begin);

            //m_dataWriter = new BinaryWriter(dataWriteStream, SystemSettings.Encoding, leaveOpen: true);
            m_dataWriter = new BinaryWriter(m_dataStream, SystemSettings.Encoding, leaveOpen: true);
        }
        private void openHeaderResources()
        {
            Log.Debug("Opening header file... {0}", m_headerFileInfo.FullName);
            var headerFileExits = m_headerFileInfo.Exists;

            if (headerFileExits == false)
            {
                FileStream fs = new FileStream(m_headerFileInfo.FullName, FileMode.CreateNew);
                fs.Seek(MAX_HEADER_FILE_SIZE, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
                fs.Dispose();
            }

            //m_headerFile = MemoryMappedFile.CreateFromFile(m_headerFileInfo.FullName, FileMode.Open, "{0}-headers".format(Name));
            m_headerFile = MemoryMappedFile.CreateFromFile(m_headerFileInfo.FullName, FileMode.Open, "{0}-headers".format(Guid.NewGuid()));

            m_headerMetaData = m_headerFile.CreateViewAccessor(0, HeaderPositions.DATA_OFFSET);
            if (headerFileExits)
            {
                // ** Double check the headerfile and datafile are in check
                var headerNumRecords = m_headerMetaData.ReadInt64(HeaderPositions.NUM_RECORDS_POS);
                if (headerNumRecords != m_numRecords)
                    throw new Exception("Error: header file is out of sync with the data file.");
            }
            else
            {
                // ** Initialize the header file metadata                    
                m_headerMetaData.Write(HeaderPositions.NUM_RECORDS_POS, 0);
            }

            // ** Position header stream to append
            m_headerStream = m_headerFile.CreateViewStream();
            m_headerStream.Seek(getHeaderPosition(m_numRecords), SeekOrigin.Begin);

            // ** Open the stream that will store/append new writes
            //headerWriteStream = m_headerFile.CreateViewStream();
            //headerWriteStream.Seek(getHeaderPosition(m_numRecords), SeekOrigin.Begin);

            //m_headerWriter = new BinaryWriter(headerWriteStream, SystemSettings.Encoding, leaveOpen: true);
            m_headerWriter = new BinaryWriter(m_headerStream, SystemSettings.Encoding, leaveOpen: true);
        }        
        private void closeDataResources()
        {
            Log.Trace("Flushing data resources...");            
            m_dataWriter.Flush();
            m_dataMetaData.Flush();
            m_dataStream.Flush();
            //dataWriteStream.Flush();            

            Log.Trace("Disposing data resources...");
            if (m_dataWriter != null)
            {
                m_dataWriter.Dispose();
                m_dataWriter = null;
            }
            if (m_dataStream != null)
            {
                m_dataStream.Dispose();
                m_dataStream = null;
            }            
            if (m_dataMetaData != null)
            {
                m_dataMetaData.Dispose();
                m_dataMetaData = null;
            }
            //if (dataWriteStream != null)
            //{
            //    dataWriteStream.Dispose();
            //    dataWriteStream = null;
            //}            
            if (m_dataFile != null)
            {
                m_dataFile.Dispose();
                m_dataFile = null;
            }
        }
        private void closeHeaderResources()
        {
            Log.Trace("Flushing header resources...");            
            m_headerWriter.Flush();
            m_headerMetaData.Flush();
            m_headerStream.Flush();
            //headerWriteStream.Flush();

            Log.Trace("Closing header resources...");
            if (m_headerWriter != null)
            {
                m_headerWriter.Dispose();
                m_headerWriter = null;
            }
            if (m_headerStream != null)
            {
                m_headerStream.Dispose();
                m_headerStream = null;
            }            
            if (m_headerMetaData != null)
            {
                m_headerMetaData.Dispose();
                m_headerMetaData = null;
            }
            //if (headerWriteStream != null)
            //{
            //    headerWriteStream.Dispose();
            //    headerWriteStream = null;
            //}            
            if (m_headerFile != null)
            {
                m_headerFile.Dispose();
                m_headerFile = null;
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

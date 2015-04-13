using solum.extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.core.storage
{
    /// <summary>
    /// This partial class contains the locking strategy for a database
    /// </summary>
    public partial class Database
    {
        #region Data Locks
        /// <summary>
        /// Lock to use when reading/writing to the data file
        /// </summary>
        ReaderWriterLockSlim dataReadWriteLock = new ReaderWriterLockSlim();        

        /// <summary>
        /// Return a WriteLocker class instance for this database
        /// </summary>
        /// <returns></returns>
        WriteLocker dataWriteLock
        {
            get
            {
                // ** Make sure the database is opened, or throw an exception
                ensureOpened();
                return new WriteLocker(dataReadWriteLock);
            }
        }
        /// <summary>
        /// Returns a ReadLocker class instance for this database
        /// </summary>
        /// <returns></returns>
        ReaderLocker dataReadLock
        {
            get
            {
                // ** Make sure the database is opened, or throw an exception
                ensureOpened();
                return new ReaderLocker(dataReadWriteLock);
            }
        }
        #endregion

        #region Header Locks
        /// <summary>
        /// Lock to use when reading/writing to the header file
        /// </summary>
        ReaderWriterLockSlim headerReadWriteLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Return a WriteLocker class instance for this database
        /// </summary>
        /// <returns></returns>
        WriteLocker headerWriteLock
        {
            get
            {
                // ** Make sure the database is opened, or throw an exception
                ensureOpened();
                return new WriteLocker(headerReadWriteLock);
            }
        }
        /// <summary>
        /// Returns a ReadLocker class instance for this database
        /// </summary>
        /// <returns></returns>
        ReaderLocker headerReadLock
        {
            get
            {
                // ** Make sure the database is opened, or throw an exception
                ensureOpened();
                return new ReaderLocker(headerReadWriteLock);
            }
        }
        #endregion

    }
}

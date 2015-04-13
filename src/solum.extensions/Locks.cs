using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.extensions
{

    #region Disposable Read/Write Locks
    /// <summary>
    /// Helper class to allow using() to obtain and release a read lock automatically
    /// </summary>
    public class WriteLocker : IDisposable
    {
        public WriteLocker(ReaderWriterLockSlim locker)
        {
            m_locker = locker;

            // ** Aquire write lock
            //Console.WriteLine("Aquiring write lock...");
            m_locker.EnterWriteLock();
            //Console.WriteLine("Aquired write lock...");
        }

        ReaderWriterLockSlim m_locker;

        public void Dispose()
        {            
            m_locker.ExitWriteLock();
            //Console.WriteLine("Released write lock...");
        }
    }

    /// <summary>
    /// Helper class to allow using() to obtain and release a read lock automatically
    /// </summary>
    public class ReaderLocker : IDisposable
    {
        public ReaderLocker(ReaderWriterLockSlim locker)
        {
            m_locker = locker;

            // ** Aquire read lock
            //Console.WriteLine("Aquiring read lock...");
            m_locker.EnterReadLock();
            //Console.WriteLine("Aquired read lock...");
        }

        ReaderWriterLockSlim m_locker;

        public void Dispose()
        {
            m_locker.ExitReadLock();
            //Console.WriteLine("Released read lock...");
        }
    }
    #endregion

}

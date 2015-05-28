using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.extensions
{
    public static class FileExtensions
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            int dwIoControlCode,
            IntPtr InBuffer,
            int nInBufferSize,
            IntPtr OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            [In] ref NativeOverlapped lpOverlapped
        );

        static void MarkAsSparseFile(SafeFileHandle fileHandle)
        {
            int bytesReturned = 0;
            NativeOverlapped lpOverlapped = new NativeOverlapped();
            bool result =
                DeviceIoControl(
                    fileHandle,
                    590020, //FSCTL_SET_SPARSE,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    ref bytesReturned,
                    ref lpOverlapped);
            
            if (result == false)
                throw new Win32Exception();
        }

        public static void MarkAsSparseFile(this FileStream file)
        {
            MarkAsSparseFile(file.SafeFileHandle);
        }

        public static void MarkAsSparseFile(string filePath)
        {
            using (var f = File.Open(filePath, FileMode.Open))
            {
                MarkAsSparseFile(f);
            }
        }

        public static long Gigabytes(this int num)
        {
            return 1024 * 1024 * 1024L * num;
        }

        public static void CreateSparseFile(string path, long size)
        {
            using (var f = File.Create(path))
            {
                MarkAsSparseFile(f.SafeFileHandle);
                f.SetLength(size);
            }
        }
    }
}

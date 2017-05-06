﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

/// <summary>
///   Iron Rure - .NET Bindings to the Rust Regex Crate
/// </summary>
namespace IronRure
{
    /// <summary>
    ///   Rust Regex
    ///
    ///   <para>
    ///     Managed wrapper around the rust regex class.
    ///   </para>
    /// </summary>
    public class Regex : IDisposable
    {
        /// <summary>
        ///   Create a new regex instance from the given pattern.
        /// </summary>
        public Regex(string pattern)
        {
            // Convert the pattern to a utf-8 encoded string.
            var patBytes = Encoding.UTF8.GetBytes(pattern);

            // Compile the regex. We get back a raw handle to the underlying
            // Rust object.
            var err = RureFfi.rure_error_new();
            _raw = RureFfi.rure_compile(
                patBytes,
                new UIntPtr((uint)patBytes.Length),
                0U,
                IntPtr.Zero,
                err);
            
            // If the regex failed to compile find out what the problem was.
            string message = null;
            if (_raw == IntPtr.Zero)
            {
                var errMessage = RureFfi.rure_error_message(err);
                message = Marshal.PtrToStringAnsi(errMessage);
            }
            
            // TODO: Rather than this dance we should create disposable
            //       wrappers around `err` and other unmanaged resources.
            RureFfi.rure_error_free(err);
            if (message != null)
                throw new RegexCompilationException(message);
        }

        ~Regex() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            var toFree = Interlocked.Exchange(ref _raw, IntPtr.Zero);
            if (toFree != IntPtr.Zero)
                RureFfi.rure_free(toFree);
        }

        /// <summary>
        ///   The handle to the Rust regex instance.
        /// </summary>
        private static IntPtr _raw;
    }
}
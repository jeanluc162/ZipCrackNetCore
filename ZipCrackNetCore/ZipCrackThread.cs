using CharsetLooping;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZipCrackNetCore
{
    /// <summary>
    /// Stores the Data necessary for Bruteforcing a Password with a Certain Length
    /// </summary>
    internal class ZipCrackThread
    {
        /// <summary>
        /// Provides the character combinations
        /// </summary>
        private CharsetLoop cl;
        /// <summary>
        /// Used to cancel the Thread from the outside
        /// </summary>
        private CancellationToken _ct;
        /// <summary>
        /// Called when the Thread comes to an end without being cancelled
        /// </summary>
        public event EventHandler Finished;
        /// <summary>
        /// Stores the password if any is found
        /// </summary>
        private String _Password = null;
        /// <summary>
        /// The File to test the passwords against
        /// </summary>
        private String _Filename;
        /// <summary>
        /// The Password that has been determined, <c>null</c> if none
        /// </summary>
        public String Password
        {
            get
            {
                return _Password;
            }
        }
        /// <summary>
        /// The ZIP-File this Thread works with
        /// </summary>
        public String Filename
        {
            get
            {
                return _Filename;
            }
        }
        /// <summary>
        /// The Length of the Combination that is being tried
        /// </summary>
        public Int32 CharCount
        {
            get
            {
                return cl.StateStart.Length;
            }
        }
        /// <summary>
        /// Creates a new ZipCrackThread-Object
        /// </summary>
        /// <param name="charset">The charset to combine into passwords</param>
        /// <param name="charcount">The length of the passwords to generate</param>
        /// <param name="filename">The File to test the passwords against</param>
        /// <param name="ct">CancellationToken for cancelling the thread</param>
        public ZipCrackThread(char[] charset, Int32 charcount, String filename, CancellationToken ct)
        {
            _ct = ct;
            cl = new CharsetLoop(charset, charcount);
            _Filename = filename;
        }
        public void Bruteforce()
        {
            System.Diagnostics.Debug.WriteLine("Starting Bruteforce: " + Filename);

            do
            {
                try
                {
                    if (ZipFile.CheckZipPassword(_Filename, cl.Loop())) //Check the Password against the File
                    {
                        _Password = new String(cl.State);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message); //Probably a broken ZIP-File or no read access
                }
            } while (cl.State != cl.StateStart && !_ct.IsCancellationRequested); //Stop if all combinations have been tested or the cancellation has been requested

            if (_ct.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine(_Filename + ": Cancellation was requested.");
                return;
            }           

            Finished?.Invoke(this, new EventArgs()); //Invoke the "Finished" Event
        }
    }
}

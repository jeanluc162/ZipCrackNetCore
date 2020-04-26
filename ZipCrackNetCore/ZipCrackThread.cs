using CharsetLooping;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ZipCrackNetCore
{
    internal class ZipCrackThread
    {
        private CharsetLoop cl;
        private CancellationToken _ct;
        public event EventHandler Finished;
        private String _Password = null;
        private String _Filename;
        public String Password
        {
            get
            {
                return _Password;
            }
        }
        public String Filename
        {
            get
            {
                return _Filename;
            }
        }
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
                    if (ZipFile.CheckZipPassword(_Filename, cl.Loop()))
                    {
                        _Password = new String(cl.State);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            } while (cl.State != cl.StateStart && !_ct.IsCancellationRequested);

            if (_ct.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine(_Filename + ": Cancellation was requested.");
                return;
            }           

            Finished?.Invoke(this, new EventArgs());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ZipCrackNetCore
{
    /// <summary>
    /// https://www.codeproject.com/Answers/397552/Validate-the-password-for-zip-file-using-DOTNETZIP
    /// </summary>
    internal class PasswordCheckStream : System.IO.MemoryStream
    {
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new GoodPasswordException();
        }

        public class GoodPasswordException : System.Exception { }
    }
}

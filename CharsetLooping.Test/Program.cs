using System;

namespace CharsetLooping.Test
{
    /// <summary>
    /// Programm for testing/showcasing the functionality of the <c>CharsetLoop</c>-Class
    /// </summary>
    class Program
    {
        /// <summary>
        /// CharsetLoop with Lowercase-letters and a combination length of 4
        /// </summary>
        private static CharsetLoop cl = new CharsetLoop(CharsetLoop.CharsetAlphabetLowerCase ,4);
        static void Main(string[] args)
        {
            do
            {
                Console.WriteLine(cl.State);
                cl.Loop();
            } while (cl.State != cl.StateStart); //Print all combinations till the first one would be due again
        }
    }
}

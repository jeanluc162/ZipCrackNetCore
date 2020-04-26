using System;

namespace CharsetLooping.Test
{
    class Program
    {
        private static CharsetLoop cl = new CharsetLoop(CharsetLoop.CharsetAlphabetLowerCase ,4);
        static void Main(string[] args)
        {
            do
            {
                Console.WriteLine(cl.State);
                cl.Loop();
            } while (cl.State != cl.StateStart);
        }
    }
}

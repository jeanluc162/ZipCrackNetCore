using CombinationGenerator;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace PerformanceTest
{
    class Program
    {
        private static readonly char[] CharsetAlphabetLowerCase = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        static void Main(string[] args)
        {
            Console.WriteLine("This program will test the speed of password generation and validation");
            Console.WriteLine();
            Console.WriteLine("####################");
            Console.WriteLine("Testing Generation");
            Console.WriteLine("####################");
            Console.WriteLine();
            
            for (int i = 1; i <= 6; i++)
            {
                Console.WriteLine("Length of Combination  : " + i.ToString());
                Console.WriteLine("Size of Charset        : " + CharsetAlphabetLowerCase.Length.ToString());
                Console.WriteLine("Combiations to generate: " + ((Int32)Math.Pow(CharsetAlphabetLowerCase.Length, i)).ToString());
                Generator g = new Generator(CharsetAlphabetLowerCase, i);
                Console.WriteLine("Starting Test...");
                DateTime Start = DateTime.Now;
                foreach (String s in g) System.Diagnostics.Debug.WriteLine(s);
                DateTime End = DateTime.Now;
                Console.WriteLine("Test Finished: " + (End - Start).TotalSeconds + "s");
                Console.WriteLine();
            }

            Console.WriteLine("####################");
            Console.WriteLine("Testing Validation");
            Console.WriteLine("####################");
            Console.WriteLine();

            MemoryStream ms = new MemoryStream();
            ms.Seek(0, SeekOrigin.Begin);

            using (ZipFile CreationZip = new ZipFile())
            {
                CreationZip.Encryption = EncryptionAlgorithm.WinZipAes128;
                CreationZip.Password = "1234567890";
                CreationZip.AddEntry("Benjamin.Franklin", "Those who would give up essential Liberty, to purchase a little temporary Safety, deserve neither Liberty nor Safety.");
                CreationZip.Save(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            using(ZipFile TestZip = ZipFile.Read(ms))
            {
                IEnumerator<ZipEntry> enumerator = TestZip.GetEnumerator();
                enumerator.MoveNext();
                ZipEntry ToTestAgainst = enumerator.Current;
                Console.WriteLine("Testing 1000 wrong passwords...");
                DateTime Start = DateTime.Now;
                for(int i = 0; i < 1000; i++)
                {
                    try
                    {
                        ToTestAgainst.Password = "0987654321";
                        using (MemoryStream tmpms = new MemoryStream())
                        {
                            ToTestAgainst.Extract(tmpms);
                        }
                    }
                    catch { }
                }
                DateTime End = DateTime.Now;
                Console.WriteLine("Test Finished: " + (End - Start).TotalSeconds + "s");
            }

            ms.Dispose();

            Console.WriteLine();
            Console.WriteLine("####################");
            Console.WriteLine("Combined Testing");
            Console.WriteLine("####################");
            Console.WriteLine();
            Console.WriteLine("Size of Charset        : " + CharsetAlphabetLowerCase.Length);
            Console.WriteLine();

            ms = new MemoryStream();
            ms.Seek(0, SeekOrigin.Begin);

            using (ZipFile CreationZip = new ZipFile())
            {
                CreationZip.Encryption = EncryptionAlgorithm.WinZipAes128;
                CreationZip.Password = "zzzy";
                CreationZip.AddEntry("Benjamin.Franklin", "Those who would give up essential Liberty, to purchase a little temporary Safety, deserve neither Liberty nor Safety.");
                CreationZip.Save(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            using (ZipFile TestZip = ZipFile.Read(ms))
            {
                IEnumerator<ZipEntry> enumerator = TestZip.GetEnumerator();
                enumerator.MoveNext();
                ZipEntry ToTestAgainst = enumerator.Current;
                DateTime OverallStart = DateTime.Now;
                for(int i = 1; i <= 4; i++)
                {
                    Console.WriteLine("Length of Combination  : " + i.ToString());
                    Console.WriteLine("Combiations to generate: " + ((Int32)Math.Pow(CharsetAlphabetLowerCase.Length, i)).ToString());

                    DateTime Start = DateTime.Now;
                    foreach (String s in new Generator(CharsetAlphabetLowerCase, i))
                    {
                        try
                        {
                            using(MemoryStream tmpms = new MemoryStream())
                            {
                                ToTestAgainst.ExtractWithPassword(tmpms, s);
                            }                           
                            Console.WriteLine("Password found: " + s);
                            break;
                        }
                        catch { }
                    }
                    DateTime End = DateTime.Now;
                    Console.WriteLine("Test Finished: " + (End - Start).TotalSeconds + "s");
                }
                DateTime OverallEnd = DateTime.Now;
                Console.WriteLine("Overall Time for Combined Test: " + (OverallEnd - OverallStart).TotalSeconds + "s");

            }

            ms.Dispose();
        }
    }
}

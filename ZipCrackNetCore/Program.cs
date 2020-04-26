using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZipCrackNetCore
{
    internal class Program
    {
        private static ConcurrentBag<CancellationTokenSource> CancellationTokens = new ConcurrentBag<CancellationTokenSource>();
        private static String TempPath = "";
        private static Int32 ThreadCount = Environment.ProcessorCount;
        private static Int32 MinLength = 0;
        private static Int32 MaxLength = 0;
        private static Int32 LengthToAssign = 0;
        private static List<Char> Charset = new List<Char>();
        private static String ZipPath = "";

        /// <summary>
        /// Main Function
        /// </summary>
        /// <param name="args">[PATH] [Charset-String] [MIN LENGHT] [MAX LENGTH]</param>
        static void Main(string[] args)
        {
            if(args.Length != 4)
            {
                Console.WriteLine("Wrong use of arguments!");
                Console.WriteLine("[PATH] [Charset-String] [MIN LENGTH] [MAX LENGTH]");
                Console.WriteLine("Example: C:\\bruh.zip ABCDEFGHIJKLMNOPQRSTUVWXYZ 5 8");
                return;
            }

            ZipPath = args[0];
            System.Diagnostics.Debug.WriteLine("Zip-Path: " + ZipPath);
            if(!File.Exists(ZipPath))
            {
                Console.WriteLine("[PATH] must exist!");
                return;
            }


            Charset.AddRange(args[1].ToCharArray());
            System.Diagnostics.Debug.WriteLine("Charset: " + args[1]);

            try
            {
                MinLength = Convert.ToInt32(args[2]);
                LengthToAssign = MinLength;
                System.Diagnostics.Debug.WriteLine("Min Length: " + MinLength.ToString());
                if (MinLength < 0)
                {
                    Console.WriteLine("[MIN LENGTH] must be 0 or greater!");
                    return;
                }

            }
            catch
            {
                Console.WriteLine("[MIN LENGTH] is not a valid number!");
                return;
            }

            try
            {
                MaxLength = Convert.ToInt32(args[3]);
                System.Diagnostics.Debug.WriteLine("Max Length: " + MaxLength.ToString());
                if (MaxLength < 0)
                {
                    Console.WriteLine("[MAX LENGTH] must be 0 or greater!");
                    return;
                }
                if(MaxLength < MinLength)
                {
                    Console.WriteLine("[MAX LENGTH] must be [MIN LENGTH] or greater!");
                    return;
                }
            }
            catch
            {
                Console.WriteLine("[MAX LENGTH] is not a valid number!");
                return;
            }

            if (MaxLength - MinLength + 1 < ThreadCount) ThreadCount = MaxLength - MinLength + 1;

            TempPath = Path.Combine(Path.GetTempPath(), "zipcracknetcore");
            System.Diagnostics.Debug.WriteLine("Temp Path: " + TempPath);
            if (!Directory.Exists(TempPath))
            {
                try
                {
                    Directory.CreateDirectory(TempPath);
                }
                catch
                {
                    Console.WriteLine("Could not created Temporary Folder: " + TempPath);
                    return;
                }
            }

            try
            {
                foreach (FileInfo file in new DirectoryInfo(TempPath).EnumerateFiles())
                {
                    System.Diagnostics.Debug.WriteLine("Deleting File: " + file.FullName);
                    file.Delete();
                }
            }
            catch
            {
                Console.WriteLine("Unable to clear out Temporary Folder: " + TempPath);
                return;
            }

            for(int i = 0; i < ThreadCount; i++)
            {
                try
                {
                    File.Copy(ZipPath, Path.Combine(TempPath, i.ToString() + ".zip"));
                    CancellationTokenSource cts = new CancellationTokenSource();
                    ZipCrackThread zct = new ZipCrackThread(Charset.ToArray(), LengthToAssign, Path.Combine(TempPath, i.ToString() + ".zip"), cts.Token);
                    zct.Finished += Zct_Finished;
                    CancellationTokens.Add(cts);
                    Thread t = new Thread(new ThreadStart(zct.Bruteforce));
                    t.Start();
                    LengthToAssign++;
                    System.Diagnostics.Debug.WriteLine("Copied ZIP: " + Path.Combine(TempPath, i.ToString() + ".zip"));
                }
                catch
                {
                    Console.WriteLine("Could not write into Temporary Folder: " + TempPath);
                    return;
                }
            }

            Console.WriteLine("Press key to abort!");
            Console.ReadKey();
            
            foreach (CancellationTokenSource cts in CancellationTokens)
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch
                {

                }
            }
        }

        private static void Zct_Finished(object sender, EventArgs e)
        {
            ZipCrackThread zct = (ZipCrackThread)sender;
            if(zct.Password != null)
            {
                foreach(CancellationTokenSource cts in CancellationTokens)
                {
                    try
                    {
                        cts.Cancel();
                        cts.Dispose();
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
                Console.WriteLine("Password: " + zct.Password);
                Environment.Exit(0);
            }
            else if(LengthToAssign > MaxLength)
            {
                Console.WriteLine("No Password found");
                Environment.Exit(1);
            }
            else
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                zct = new ZipCrackThread(Charset.ToArray(), LengthToAssign, zct.Filename, cts.Token);
                Thread t = new Thread(new ThreadStart(zct.Bruteforce));
                CancellationTokens.Add(cts);
                t.Start();
                LengthToAssign++;
            }
        }
    }
}

using CombinationGenerator;
using Ionic.Zip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZipCrackNetCore
{
    internal class Program
    {
        private static ConcurrentQueue<String> UnsuccessfullTries;
        private static ConcurrentQueue<String> Passwords = new ConcurrentQueue<String>();
        private static CancellationTokenSource CancellationToken = new CancellationTokenSource();
        private static String TempPath = "";
        private static Int32 ThreadCount = Environment.ProcessorCount;
        private static Int32 MinLength = 0;
        private static Int32 MaxLength = 0;
        private static String ZipPath = "";

        /// <summary>
        /// Main Function
        /// </summary>
        /// <param name="args">[PATH] [Charset-String] [MIN LENGHT] [MAX LENGTH] {output}</param>
        static void Main(string[] args)
        {
            if(args.Length < 4 || args.Length > 5) //4 Arguments are needed, 'output' is optional
            {
                Console.WriteLine("Wrong use of arguments!");
                Console.WriteLine("[PATH] [Charset-String] [MIN LENGTH] [MAX LENGTH] {output}");
                Console.WriteLine("Adding 'output' to the end of the command will print all tries to the console. This will slow down the program!");
                Console.WriteLine("Example: C:\\bruh.zip ABCDEFGHIJKLMNOPQRSTUVWXYZ 5 8");
                return;
            }

            //Check if the Zip-File actually exists
            ZipPath = args[0];
            System.Diagnostics.Debug.WriteLine("Zip-Path: " + ZipPath);
            if(!File.Exists(ZipPath))
            {
                Console.WriteLine("[PATH] must exist!");
                return;
            }

            //Add the Provided Characters to the List
            System.Diagnostics.Debug.WriteLine("Charset: " + args[1]);

            try
            {
                MinLength = Convert.ToInt32(args[2]);
                System.Diagnostics.Debug.WriteLine("Min Length: " + MinLength.ToString());
                if (MinLength < 0) //Password has to be at least 0 characters long
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
                if (MaxLength < 0) //Password has to be at least 0 characters long
                {
                    Console.WriteLine("[MAX LENGTH] must be 0 or greater!");
                    return;
                }
                if(MaxLength < MinLength) //The maximum length of the password has to be >= the minimum length
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
            try
            {
                if (args[4].Trim().ToLower() == "output")
                {
                    UnsuccessfullTries = new ConcurrentQueue<String>();
                    new Thread(() => OutputUnsuccessfullTries()).Start();
                }
            }
            catch { }

            TempPath = Path.Combine(Path.GetTempPath(), "zipcracknetcore"); //Temporary Directory to use for the copied ZIPs
            System.Diagnostics.Debug.WriteLine("Temp Path: " + TempPath);
            if (!Directory.Exists(TempPath))
            {
                try
                {
                    Directory.CreateDirectory(TempPath); //Generate the temporary directory if it does not exist
                }
                catch
                {
                    Console.WriteLine("Could not created Temporary Folder: " + TempPath);
                    return;
                }
            }

            try
            {
                foreach (FileInfo file in new DirectoryInfo(TempPath).EnumerateFiles()) //Delete existing files in the Temporary directory
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

            new Thread(() => GeneratorThread(MinLength, MaxLength, args[1])).Start();

            for (int i = 0; i < ThreadCount; i++)
            {
                try
                {
                    String Filename = Path.Combine(TempPath, i.ToString() + ".zip");
                    File.Copy(ZipPath, Filename); //Generate a copy of the ZIP-File for each Thread
                    System.Diagnostics.Debug.WriteLine("Copied ZIP: " + Filename);
                    new Thread(() => PasswordThread(Filename)).Start();                    
                }
                catch
                {
                    Console.WriteLine("Could not write into Temporary Folder: " + TempPath);
                    return;
                }
            }

            Console.WriteLine("Press key to abort!");
            Console.ReadKey();

            Passwords.Clear();
            CancellationToken.Cancel();
        }
        private static void GeneratorThread(Int32 MinLength, Int32 MaxLength, String Charset)
        {
            for(int i = MinLength; i <= MaxLength;i++)
            {
                Generator generator = new Generator(Charset, i);
                foreach(String password in generator)
                {
                    if (CancellationToken.Token.IsCancellationRequested) return;
                    if (Passwords.Count < 10000) Passwords.Enqueue(password);
                    else Thread.Sleep(10);
                }
            }
        }
        private static void OutputUnsuccessfullTries()
        {
            String LastTry;
            while (!CancellationToken.Token.IsCancellationRequested)
            {
                if(UnsuccessfullTries.TryDequeue(out LastTry))
                {
                    Console.WriteLine(LastTry);
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
        }
        private static void PasswordThread(String Filename)
        {
            using (ZipFile TestZip = ZipFile.Read(Filename))
            {
                IEnumerator<ZipEntry> enumerator = TestZip.GetEnumerator();
                enumerator.MoveNext();
                ZipEntry ToTestAgainst = enumerator.Current;

                while(!CancellationToken.Token.IsCancellationRequested || !Passwords.IsEmpty)
                {
                    String PasswordToTry;
                    if(Passwords.TryDequeue(out PasswordToTry))
                    {
                        UnsuccessfullTries?.Enqueue(PasswordToTry);
                        using (MemoryStream tmpms = new MemoryStream())
                        {
                            try
                            {
                                ToTestAgainst.ExtractWithPassword(tmpms, PasswordToTry);
                                CancellationToken.Cancel();
                                Passwords.Clear();
                                UnsuccessfullTries?.Clear();                                
                                Console.WriteLine("Found Password: " + PasswordToTry);
                                Environment.Exit(0);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
        }
    }
}

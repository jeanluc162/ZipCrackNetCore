using ZipCrackNetCore.Library;
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
        /// <summary>
        /// FIFO-List containing passwords that are tried/have been tried against the zip-file. Used for Console Output.
        /// </summary>
        private static ConcurrentQueue<String> PasswordTries;
        /// <summary>
        /// FIFO-List containg passwords that have yet to be tried against a zip-file.
        /// </summary>
        private static ConcurrentQueue<String> Passwords = new ConcurrentQueue<String>();
        /// <summary>
        /// Tokensource for stopping the Threads
        /// </summary>
        private static CancellationTokenSource CancellationToken = new CancellationTokenSource();
        /// <summary>
        /// Temporary path (used for creating copies of the Zip)
        /// </summary>
        private static String TempPath = "";
        /// <summary>
        /// Thread count
        /// </summary>
        private static Int32 ThreadCount = Environment.ProcessorCount;
        /// <summary>
        /// minimum password length
        /// </summary>
        private static Int32 MinLength = 0;
        /// <summary>
        /// maximum password length
        /// </summary>
        private static Int32 MaxLength = 0;
        /// <summary>
        /// Filepath of the zip-file to crack
        /// </summary>
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
                //Has the argument 'output' been provided?
                if (args[4].Trim().ToLower() == "output")
                {
                    PasswordTries = new ConcurrentQueue<String>(); //Create Output Queue
                    new Thread(() => OutputTries()).Start(); //Start Output Thread
                }
            }
            catch { }


            new Thread(() => GeneratorThread(MinLength, MaxLength, args[1])).Start(); //Start Password Generator Thread

            using (MemoryStream zipcontents = new MemoryStream())
            {
                using (FileStream fs = new FileStream(ZipPath, FileMode.Open))
                {
                    fs.CopyTo(zipcontents);
                }

                for (int i = 0; i < ThreadCount; i++)
                {
                    zipcontents.Position = 0;
                    try
                    {
                        MemoryStream currentZipContents = new MemoryStream();
                        zipcontents.CopyTo(currentZipContents);
                        currentZipContents.Position = 0;
                        new Thread(() => PasswordThread(currentZipContents)).Start(); //Start Password Try Thread                    
                    }
                    catch
                    {
                        Console.WriteLine("Could not write into Temporary Folder: " + TempPath);
                        return;
                    }
                }
            }
            

                

            Console.WriteLine("Press key to abort!");
            Console.ReadKey();

            //User wanted to abort. Clear queues, cancel threads and exit.
            PasswordTries?.Clear();
            Passwords.Clear();
            CancellationToken.Cancel();
            Environment.Exit(0);
        }
        /// <summary>
        /// Fills the Password-Queue
        /// </summary>
        /// <param name="MinLength">Minimum Combination Length</param>
        /// <param name="MaxLength">Maximum Combination Length</param>
        /// <param name="Charset">Used Charset</param>
        private static void GeneratorThread(Int32 MinLength, Int32 MaxLength, String Charset)
        {
            for(int i = MinLength; i <= MaxLength;i++) //Loop through all required lengths
            {
                Generator generator = new Generator(Charset, i); //Different Generator for each combination length
                foreach(String password in generator)
                {
                    if (CancellationToken.Token.IsCancellationRequested) return; //Check if Cancellation has been requested (meaning a valid password has been found)
                    while (Passwords.Count > 100000) Thread.Sleep(10); //Waits till the Password Queue has less than 10k elements in it
                    Passwords.Enqueue(password); //Add the new Password to the Queue if the Queue has less than 10k elements
                }
            }
            Console.WriteLine("Generator Thread finished work!");
        }
        /// <summary>
        /// Outputs all tried combinations
        /// </summary>
        private static void OutputTries()
        {
            String LastTry;
            while (!CancellationToken.Token.IsCancellationRequested) //Check if cancellation has been requested
            {
                if(PasswordTries.TryDequeue(out LastTry)) //Dequeue a tried password
                {
                    Console.WriteLine(LastTry); //Printout a Tried password
                }
                else
                {
                    Thread.Sleep(5); //Wait for 5ms if there were no passwords to display
                }
            }
        }
        /// <summary>
        /// Tries the passwords against a zip-file
        /// </summary>
        /// <param name="ZipStream">Filename of the zip-file</param>
        private static void PasswordThread(MemoryStream ZipStream)
        {
            using (ZipFile TestZip = ZipFile.Read(ZipStream)) //read the zip-file
            {
                IEnumerator<ZipEntry> enumerator = TestZip.GetEnumerator(); //Get an enumerator for the entries of the zip-file
                enumerator.MoveNext(); //Move to the first entry
                ZipEntry ToTestAgainst = enumerator.Current;
                String PasswordToTry;

                using (MemoryStream tmpms = new MemoryStream())
                {
                    while (!CancellationToken.Token.IsCancellationRequested || !Passwords.IsEmpty) //Check if Cancellation has been requested or all passwords have been tried
                    {
                        tmpms.Position = 0;
                        if (Passwords.TryDequeue(out PasswordToTry)) //Dequeue a password to try out
                        {
                            PasswordTries?.Enqueue(PasswordToTry); //Queue password to output

                            try
                            {
                                ToTestAgainst.ExtractWithPassword(tmpms, PasswordToTry); //Try Extracting the first entry. Will throw an exception if the wrong password is used.

                                //If we get here, no exception has been thrown meaning that the password was correct
                                CancellationToken.Cancel(); //Request cancellation of all threads
                                Passwords.Clear(); //Clear password queue
                                PasswordTries?.Clear(); //Clear output queue                       
                                Console.WriteLine("Found Password: " + PasswordToTry); //Output determined password
                                Environment.Exit(0); //End the program
                            }
                            catch { }
                        }
                        else
                        {
                            Thread.Sleep(10); //Wait 10ms if no password could be dequeued
                        }
                    }
                }                
            }
        }
    }
}

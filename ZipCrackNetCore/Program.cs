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
        /// <summary>
        /// The next combination length to test. Has to be increased after a new ZipCrackThread has been created.
        /// </summary>
        private static Int32 LengthToAssign = 0;
        private static List<Char> Charset = new List<Char>();
        private static String ZipPath = "";

        /// <summary>
        /// Main Function
        /// </summary>
        /// <param name="args">[PATH] [Charset-String] [MIN LENGHT] [MAX LENGTH]</param>
        static void Main(string[] args)
        {
            if(args.Length != 4) //All 4 Arguments are needed
            {
                Console.WriteLine("Wrong use of arguments!");
                Console.WriteLine("[PATH] [Charset-String] [MIN LENGTH] [MAX LENGTH]");
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
            Charset.AddRange(args[1].ToCharArray());
            System.Diagnostics.Debug.WriteLine("Charset: " + args[1]);

            try
            {
                MinLength = Convert.ToInt32(args[2]);
                LengthToAssign = MinLength;
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

            //If there are less Password Lengths to test than there are cores available, use the amount of Lengths as the Thread Count instead
            if (MaxLength - MinLength + 1 < ThreadCount) ThreadCount = MaxLength - MinLength + 1;

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

            for(int i = 0; i < ThreadCount; i++)
            {
                try
                {
                    File.Copy(ZipPath, Path.Combine(TempPath, i.ToString() + ".zip")); //Generate a copy of the ZIP-File for each Thread
                    CancellationTokenSource cts = new CancellationTokenSource();
                    ZipCrackThread zct = new ZipCrackThread(Charset.ToArray(), LengthToAssign, Path.Combine(TempPath, i.ToString() + ".zip"), cts.Token); //Object used for the Thread
                    zct.Finished += Zct_Finished; //Callback for when a Thread has finished it's work
                    CancellationTokens.Add(cts); //Cancellation Tokens need to be safed to properly shut down the program when a password has been found
                    Thread t = new Thread(new ThreadStart(zct.Bruteforce));
                    t.Start(); //Start the Thread
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
            
            foreach (CancellationTokenSource cts in CancellationTokens) //Get rid of all the Threads when the user presses a key
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

        /// <summary>
        /// A thread has come to an End without being cancelled
        /// </summary>
        /// <param name="sender">The Object containing the Thread-Function</param>
        /// <param name="e"></param>
        private static void Zct_Finished(object sender, EventArgs e)
        {
            ZipCrackThread zct = (ZipCrackThread)sender;
            if(zct.Password != null) //The password has been found
            {
                foreach(CancellationTokenSource cts in CancellationTokens) //Cancel all the remaining Threads
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
                Console.WriteLine("Password: " + zct.Password); //Print out the password
                Environment.Exit(0); //Exit the program
            }
            else if(LengthToAssign <= MaxLength) //Start a new Thread to try a longer password
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                zct = new ZipCrackThread(Charset.ToArray(), LengthToAssign, zct.Filename, cts.Token);
                Thread t = new Thread(new ThreadStart(zct.Bruteforce));
                CancellationTokens.Add(cts);
                t.Start();
                LengthToAssign++;
            }
            else if(zct.CharCount == MaxLength) //Longest combination has finished testing, program is done
            {
                Console.WriteLine("No password has been found.");
                Environment.Exit(1);
            }
        }
    }
}

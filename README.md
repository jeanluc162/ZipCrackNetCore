# ZipCrackNetCore

A simple zip-password bruteforce utility written in C# using the .Net-Core Framework.

# How to use

This program is a commandline-utility that needs four parameters:
1. \[PATH\]: The Filepath of the ZIP-File to bruteforce. Examples: "C:\bruh.zip" or "/home/boringuser/jeff.zip"
2. \[Charset-String\]: String Containing the characters to use. Example: "0123456789" to test numerical passwords
3. \[MIN LENGTH\]: The shortest combination to test. Example: "2"
4. \[MAX LENGTH\]: The longest combination to test. Example: "8"
5. {output}: Adding 'output' to the end of the command will display every password tried. This slows down execution!

The parameters have to be supplied in the same order as specified above!

Example usage: `dotnet ZipCrackNetCore.dll /home/myaccount/pron.zip abcdefghijklmnopqrstuvwxyz 5 8 output` would test passwords with 5 to 8 characters consisting of all lowercase letters against the file "pron.zip" and print all the tries.

The programm will either tell you the password or inform you that no password has been found. Progress is not visualized unless {output} is used.

# How it works

1. The program figures out how many Threads to use. By default, the amount is equal to the amount of logical cores available plus one additional Thread for creating the combinations and (optional) one for outputing the tried passwords to the console.
2. The program creates one copy of the ZIP-File for each thread in a temporary folder.
3. The program starts the amount of Threads it wants to use.
4. The program stops when a password is found or all combinations have been tried.
   

using System;
using System.IO;
using System.Collections.Generic;
using Execution;

namespace Skrypt
{
    class Program
    {
        static bool printTokens = false;
        static bool printAST    = true;

        static void Main(string[] args)
        {
            string  filePath    = @"E:\GitHub\Project_Skrypt\code.skrypt";

            StreamReader    sr      = new StreamReader(filePath);
            string          code    = sr.ReadToEnd();
            sr.Close();

            Execution.Executor.Run(code, printAST, printTokens);

            Console.Read();

            return;
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using Evaluation;
using Tokenisation;
using Parsing;
using System.Runtime.Serialization.Formatters.Binary;

namespace Skrypt
{
    class Program
    {
        static Tokenizer tokenizer = new Tokenizer();
        static List<Token> tokens;
        static node program;

        static bool printTokens = false;
        static bool printAST    = true;

        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string filePath = @"E:\GitHub\Project_Skrypt\code.skrypt";
            StreamReader sr = new StreamReader(filePath);
            string code = sr.ReadToEnd();
            sr.Close();

            code = code.Replace('\0', ' ');

            tokens = tokenizer.GetTokens(code);

            program = parser.ParseTokens(tokens);

            if (printTokens)
            {
                foreach (Token token in tokens)
                {

                    Console.Write("Token \t");

                    ConsoleColor color = ConsoleColor.White;
                    ConsoleColor seccolor = ConsoleColor.White;

                    if (token.type == TokenType.String) {
                        color = ConsoleColor.Red;    
                        seccolor = ConsoleColor.Yellow;  
                    }
                    else
                    if (token.type == TokenType.Keyword) {
                        color = ConsoleColor.Cyan;  
                        seccolor = color;                       
                    }
                    else
                    if (token.type == TokenType.Boolean) {
                        color = ConsoleColor.Red;  
                        seccolor = ConsoleColor.DarkMagenta;                       
                    }
                    else
                    if (token.type == TokenType.Numeric) {
                        color = ConsoleColor.Red;    
                       seccolor = ConsoleColor.DarkMagenta;                  
                    }
                    else
                    if (token.type == TokenType.Identifier) {
                        color = ConsoleColor.DarkBlue;    
                        seccolor = ConsoleColor.DarkBlue;                  
                    }
                    else
                    if (token.type == TokenType.Punctuator) {
                        color = ConsoleColor.White;    
                        seccolor = ConsoleColor.White;                  
                    }

                    Console.ForegroundColor = color;
                    Console.Write(token.type);
                    Console.ResetColor();
                    Console.Write("  \t");
                    Console.ForegroundColor = seccolor;
                    Console.Write(token.value + "\n");
                    Console.ResetColor();
                }
            }  

            if (printAST) {
                Console.WriteLine(program);
            }

            watch.Stop();
            Console.WriteLine("Total time: " + watch.ElapsedMilliseconds + " ms");

            return;
        }
    }
}

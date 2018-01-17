using System;
using System.Collections.Generic;
using Tokenisation;
using Parsing;

namespace ErrorHandling {
    public class SkryptException : Exception {
        public int Line;
        public int Row;
        public int ErrorCode;

        public SkryptException(string str = "", Token tkn = null) {
            if (tkn == null) {
                Console.WriteLine("Skrypt error:\n\t" + str);
            }else{
                Console.WriteLine("Skrypt error(" + tkn.line + "," + tkn.col + "):\n\t" + str);                
            }
        }

        public SkryptException(string message, string str) : base(message) {}

        public SkryptException(string message, string str, Exception inner) : base(message, inner) {}
    }

    public class SkryptVariableDoesNotExistException : SkryptException {
        public SkryptVariableDoesNotExistException(string v) {
            Console.WriteLine("\t Variable '" + v + "' does not exist!");
        }
    }

    public class SkryptInvalidOperationException : SkryptException {
        public SkryptInvalidOperationException(string T1, string T2, string Operation) {
            Console.WriteLine("\t No such thing as '" + T1 + "' " + Operation + " '" + T2 + "'");
        }
    }

    public class SkryptMethodDoesNotExistException : SkryptException {
        public SkryptMethodDoesNotExistException(string id) {
            Console.WriteLine("\t Method '" + id + "' does not exist!");
        }
    }

    public class Rule {
        string Preceded;
        string[] Expected;

        public Rule (string P, string[] E) {
            Preceded = P;
            Expected = E;
        }
    }

    public enum TokenTypes : byte {
        Punctuator,
        Identifier,
        String,       
        Keyword,
        Numeric,
        Boolean,
        Method,
    }

    public static class ErrorHandler {
        static Dictionary<string, List<Rule>> Rules = new Dictionary<string, List<Rule>>();

        static void SetRules () {
            Rules["assign"] = new List<Rule>();
            Rules["assign"].Add(new Rule (
                "identifier",
                new string[] {""}
            ));
        }

        static void DoError (SkryptException error) {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("PARSE ERROR (" + error.ErrorCode.ToString("X") + ")");
        }
    }
}
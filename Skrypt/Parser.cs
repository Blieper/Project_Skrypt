using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using Tokenisation;

namespace Parsing
{
    public class node {
        public string body;

        public List<node> nodes = new List<node>();
        public int depth = 0;

        public override string ToString() {

            int fb = depth > 0 ? depth+1: depth;
            string str = new String(' ', fb) + body;

            foreach (node Node in nodes) {
                str += "\n" + new String(' ', depth) + Node.ToString();
            }

            //if (nodes.Count > 0)
                //str = str +  "\n" + new String(' ', Math.Clamp(depth*2-1,0,9999)) + "}";

            return str;
        }
    }

    static public class parser {

        static public bool IsPredefinedVariable (Token Identifier) {
            return true;
        }

        static public bool IsPredefinedMethod (Token Identifier) {
            return true;
        }

        static public string stringList (List<Token> list) {
            string debug = "";

            foreach (Token tk in list) {
                debug += tk.value + " ";
            }

            return debug;
        }


        static public node ParseExpression (node branch, List<Token> expression) {
            List<Token> leftBuffer  = new List<Token>();
            List<Token> rightBuffer = new List<Token>();;

            int i = -1;
            bool isPar = false;

            Console.WriteLine(stringList(expression));

            while (i < expression.Count - 1) {
                i++;

                Token token = expression[i];

                Console.WriteLine(token);
                
                if (token.value == "lpar") {
                    isPar = true; 
                    Console.WriteLine("Found lpar");
                    continue;
                }

                if (isPar) {
                    if (token.value == "rpar") {
                        isPar = false; 
                        Console.WriteLine("Found rpar");
                    }

                    Console.WriteLine("Skipping");
                    continue;
                } else {
                    if (token.type == TokenType.Punctuator || token.type == TokenType.Keyword) {
                        node newNode = new node();
                        newNode.depth = branch.depth + 1;
                        newNode.body = token.value;

                        branch.nodes.Add(newNode);

                        if (i > 0) {
                            leftBuffer  = expression.GetRange(0,i);
                            newNode.nodes.Add(ParseExpression (newNode, leftBuffer));
                        }

                        if (i < expression.Count) {
                            rightBuffer = expression.GetRange(i + 1,expression.Count - i - 1);            
                            newNode.nodes.Add(ParseExpression (newNode, rightBuffer));
                        }  

                        if (newNode.nodes.Count > 2) 
                            newNode.nodes.RemoveAt(newNode.nodes.Count - 1);

                        break; 
                    }
                }
            }

            if (expression[0].value == "lpar" && expression[expression.Count - 1].value == "rpar") {
                 expression = expression.GetRange(1,expression.Count - 2);
                 Console.WriteLine("No par: " + stringList(expression));

                 return ParseExpression (branch, expression);
            }

            return new node () {body = expression[0].value, depth = branch.depth + 1};
        }

        static public node ParseTokens (List<Token> tokensList) 
        {
            node program = new node(){body = "program"};
            List<Token> tokenBuffer = new List<Token>();

            for (int i = 0; i < tokensList.Count; i++) {
                Token token = tokensList[i];

                if (token.value == "eol") 
                {
                    ParseExpression(program,tokenBuffer);
                    tokenBuffer.Clear();
                    continue;
                }
        
                tokenBuffer.Add(token);
            }

            Console.WriteLine(program);
            return program;
        }
    }
}
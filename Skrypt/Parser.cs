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

        static public string repStr (string str, int n) {
            string newString = "";

            while (n > 1) {
                if (n % 2 == 0) 
                    newString += str;
                n--;
            }

            return newString;
        }

        public override string ToString() {

            int fb = depth > 0 ? depth+1: depth;
            string str = repStr("¦", depth+1) + body;

            foreach (node Node in nodes) {
                str += "\n" + repStr("¦", depth+1) + Node.ToString();
            }

            //if (nodes.Count > 0)
                //str = str +  "\n" + new String(' ', Math.Clamp(depth*2-1,0,9999)) + "}";

            return str;
        }
    }

    public class Variable {
        public string value;
        public string type;
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

        static public string repStr (string str, int n) {
            string newString = str;

            while (n > 1) {
                newString += str;
                n--;
            }

            return newString;
        }

        static public node ParseExpression (node branch, List<Token> expression) {
            List<Token>    leftBuffer   = new List<Token>();
            List<Token>    rightBuffer  = new List<Token>();
            List<Variable> variableList = new List<Variable>();

            int     i           = -1;
            bool    isPar       = false;
            int     rParCount   = 0;
            bool    isSplit     = false;

            Console.WriteLine(repStr("¦", branch.depth) + stringList(expression) + "");

            while (i < expression.Count - 1) {
                i++;

                Token token = expression[i];

                // If a left parenthesis is found, increase rParCount so we can keep track of the current pair   
                if (token.value == "lpar") {
                    rParCount++;
                    isPar = true; 
                    Console.WriteLine(repStr("¦", branch.depth) +  "lpar: " + rParCount);
                    continue;
                }

                if (isPar) {

                    // If a right parenthesis is found, decrease rParCount so we can keep track of the current pair     
                    if (token.value == "rpar") {
                        rParCount--;

                        Console.WriteLine(repStr("¦", branch.depth) + "rpar: " + rParCount);

                        // If rParCount == 0, it means that we found the outer pair of parenthesis
                        if (rParCount == 0) {
                            isPar = false; 
                            rParCount = 0;
                        }
                    }else{
                        Console.WriteLine(repStr("¦", branch.depth) + "Skipping");

                        // Skip until we find the outer pair of parenthesis
                        continue;
                    }
                } else {
                    if (token.type == TokenType.Punctuator || token.type == TokenType.Keyword) {
                        // Create and add a new node for the branch and set its body/depth
                        node newNode = new node();
                        newNode.depth = branch.depth + 1;
                        newNode.body = token.value;

                        Console.WriteLine(repStr("¦", branch.depth) + "Punc: " + newNode.body);

                        // Only check for left tokens if there are any
                        if (i > 0) {
                            // All of the tokens on the left
                            leftBuffer  = expression.GetRange(0,i);
                            Console.WriteLine(repStr("¦", branch.depth) + "Left: " + stringList(leftBuffer));
                            
                            // Parsing a node from the left buffer
                            node addNode = ParseExpression (newNode, leftBuffer);

                            // Only add that node if it's valid
                            if (addNode != null)
                                newNode.nodes.Add(addNode);
                        }

                        // Only check for right tokens if there are any
                        if (i < expression.Count-1) {
                            // All of the tokens on the right
                            rightBuffer = expression.GetRange(i + 1,expression.Count - i - 1);       
                            Console.WriteLine(repStr("¦", branch.depth) + "Right: " + stringList(rightBuffer));
                            
                            // Parsing a node from the right buffer
                            node addNode = ParseExpression (newNode, rightBuffer);

                            // Only add that node if it's valid
                            if (addNode != null) 
                                newNode.nodes.Add(addNode);
                        }  
                         
                        if ((i < expression.Count-1) && i > 0)
                            branch.nodes.Add(newNode);
                        
                        // We had to split up the expression, so set isSplit to true
                        isSplit = true;
                        break; 
                    }
                }
            }

            // Parse an expression thats between a pair of parenthesis, but ONLY if it wasn't split up before
            if (!isSplit && expression[0].value == "lpar" && expression[expression.Count - 1].value == "rpar") {
                 expression = expression.GetRange(1,expression.Count - 2);
                 Console.WriteLine(repStr("¦", branch.depth-1) + "No par: " + stringList(expression));

                 return ParseExpression (branch, expression);
            }

            // Return no node if the expression list contains more than 1 token
            if (expression.Count > 1) {
                return null;
            }

            // Return leaf node
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
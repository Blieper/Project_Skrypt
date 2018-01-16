using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using Tokenisation;
using MethodBuilding;
using static MethodBuilding.MethodContainer;

namespace Parsing
{
    public class node {
        public string body;
        public object Value;
        public TokenType type;

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

            return str;
        }
    }

    public class OperatorCategory {
        public string[] operators;
        public bool rightToLeft = false;
        public byte count = 2;

        public OperatorCategory (string[] ops, bool rtl = false, byte cnt = 2) {
            operators = ops;
            rightToLeft = rtl;
            count = cnt;
        }
    }

    public class Variable {
        public string value;
        public string type;
    }

    static public class parser {

        public static List<OperatorCategory> opPrecedence = new List<OperatorCategory>();

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

        static public node ParseExpression (node branch, List<Token> expression, bool gotSplit = false) {
            List<Token>    leftBuffer   = new List<Token>();
            List<Token>    rightBuffer  = new List<Token>();
            List<Variable> variableList = new List<Variable>();

            int     i           = -1;
            int     opPC        = opPrecedence.Count;

            for (int j = 0; j < opPC; j++) {
                OperatorCategory Cat = opPrecedence[j];
                bool rtl = Cat.rightToLeft;
                byte cnt = Cat.count;
                int k = rtl ? Cat.operators.Length - 1 : 0;
                bool loop = true;
                bool obrk = false;

                while (loop) {

                    i = -1;
                    bool brk = false;
                            
                    bool    isPar       = false;
                    int     rParCount   = 0;
                    int     parPairs    = 0;
                    bool    surroundedPar = false;
                    bool    firstIsPar  = false;
                    bool    firstIsMethod = false;
                    bool    methodPar   = false;
                    bool    expressionIsMethod = false;
                    Method  foundMethod = null;

                    string op = Tokenizer.processPunctuator(Cat.operators[k]);

                    while (i < expression.Count - 1) {
                        i++;

                        Token token = expression[i];

                        if (MethodHandler.Exists(token.value)) {
                            if (i == 0) {
                                firstIsMethod = true; 
                                foundMethod = MethodHandler.Get(token.value);
                            }
                        }

                        // If a left parenthesis is found, increase rParCount so we can keep track of the current pair   
                        if (token.value == "lpar") {
                            rParCount++;
                            isPar = true; 

                            if (i == 0) {
                                firstIsPar = true; 
                            } else if (i == 1 && firstIsMethod) {
                                methodPar = true;
                            }
                        }

                        if (isPar) {
                            // If a right parenthesis is found, decrease rParCount so we can keep track of the current pair     
                            if (token.value == "rpar") {
                                rParCount--;

                                // If rParCount == 0, it means that we found a matching pair of parenthesis
                                if (rParCount == 0) {
                                    parPairs++;
                                    isPar = false;

                                    if (i == expression.Count - 1 && firstIsPar && parPairs == 1) {
                                        surroundedPar = true;
                                        break;
                                    }

                                    if (i == expression.Count - 1 && methodPar) {
                                        expressionIsMethod = true;
                                        break;
                                    }

                                    continue; 
                                }
                            }else{             
                                // Skip until we find the matching right parenthesis
                                continue;
                            }
                        } 
                        
                        if (Tokenizer.processPunctuator(op.Replace("V","")) == token.value) {
                            Console.WriteLine(op);

                            node newNode  = new node();
                            node addNode;
                            newNode.depth = branch.depth + 1;
                            newNode.body  = token.value;

                            // All of the tokens on the left
                            leftBuffer  = expression.GetRange(0,i);

                            // All of the tokens on the right
                            rightBuffer = expression.GetRange(i + 1,expression.Count - i - 1);  

                            if (Tokenizer.processPunctuator(op) == "pdec" || Tokenizer.processPunctuator(op) == "pinc") {      
                                op = op.Replace("V","");   
                                newNode.body = "p" + Tokenizer.processPunctuator(op);

                                Console.WriteLine(newNode.body);

                                addNode = ParseExpression (newNode, leftBuffer, true);
                                if (addNode != null) 
                                    newNode.nodes.Add(addNode);                                    
                            }else{
                                // Parsing a node from the left buffer
                                if (cnt != 1) {
                                    addNode = ParseExpression (newNode, leftBuffer, true);
                                    if (addNode != null) 
                                        newNode.nodes.Add(addNode);
                                }

                                // Parsing a node from the right buffer
                                addNode = ParseExpression (newNode, rightBuffer, true);
                                if (addNode != null) 
                                    newNode.nodes.Add(addNode);
                            }

                            branch.nodes.Add(newNode);

                            // Break out of all loops
                            brk = true;
                            break;
                        }
                    }

                    // Parse an expression thats between a pair of parenthesis, but ONLY 
                    // if it wasn't split up before, and only if the whole block is surrounded by matching parenthesis
                    if (surroundedPar) {
                        expression = expression.GetRange(1,expression.Count - 2);
                        return ParseExpression (branch, expression);
                    } 

                    if (expressionIsMethod) {
                        node addNode = ParseMethod (branch, expression, foundMethod);
                        if (addNode != null && !gotSplit) 
                            branch.nodes.Add(addNode);     

                        return addNode;                        
                    }

                    // Loop direction (0 to length - 1 or length - 1 to 0)
                    if (rtl) {
                        loop = k > 0;
                        k--;
                    }else{
                        loop = k < Cat.operators.Length - 1;
                        k++;
                    }

                    // Break out of all loops
                    if (brk) {
                        obrk = true;
                        break; 
                    }
                }

                // Break out of all loops
                if (obrk)
                    break;           
            }

            // Return no node if the expression list contains more than 1 token at this point
            if (expression.Count > 1) {
                return null;
            }

            Token tkn = expression[0];
            object Value = null;

            switch (tkn.type) {
                case TokenType.Numeric:
                    Value = float.Parse(tkn.value);
                break;
                case TokenType.String:
                    Value = tkn.value.Substring(1,tkn.value.Length - 2);
                break;
                case TokenType.Boolean:
                    if (tkn.value == "true") {
                        Value = true;
                    }else{
                        Value = false;
                    }
                break;
                default:
                break;
            }

            // Return leaf node
            return new node () {body = expression[0].value, depth = branch.depth + 1, Value = Value, type = tkn.type};
        }

        static public node ParseMethod (node branch, List<Token> expression, Method method) {
            // Initialise base node
            node baseNode = new node () {body = "method", depth = branch.depth + 1};

            // Initilise the method node
            node methodNode = new node () {body = expression[0].value, depth = baseNode.depth + 1};
            baseNode.nodes.Add(methodNode);
              
            node typeNode = new node() {body = "type", depth = methodNode.depth + 1};
            methodNode.nodes.Add(typeNode);

            node t = new node() {body = method.returnType, depth = typeNode.depth + 1};
            typeNode.nodes.Add(t);

            if (method.arguments.Length > 0 && method.arguments != null) {
                // arguments / parameters
                // Initilise the arguments node
                node arguments = new node() {body = "arguments", depth = methodNode.depth + 1};
                methodNode.nodes.Add(arguments);

                // Make a node for each argument for the method
                foreach (string arg in method.arguments) {
                    node addNode = new node() {body = arg, depth = arguments.depth + 1};

                    if (addNode != null)
                        arguments.nodes.Add(addNode);
                }

                // Get an expression for each argument, seperated by commas
                List<List<Token>> argumentExpressions = new List<List<Token>>();
                int argId = 0;
                argumentExpressions.Add(new List<Token>());

                for (int i = 2; i < expression.Count - 1; i ++) {
                    Token token = expression[i];

                    if (token.value == "seperate") {
                        argId++;
                        argumentExpressions.Add(new List<Token>());
                    }else{
                        argumentExpressions[argId].Add(token);
                    }
                }

                // Parse each expression
                argId = 0;
                foreach (List<Token> expr in argumentExpressions) {
                    node addNode = ParseExpression(arguments.nodes[argId],expr);
                    if (addNode != null) 
                        arguments.nodes[argId].nodes.Add(addNode);

                    argId++;
                }
            }


            return baseNode;
        }

        static public node ParseTokens (List<Token> tokensList) 
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //opPrecedence.Add(new OperatorCategory(new string[] {"(",")","."}));
            opPrecedence.Add(new OperatorCategory(new string[] {"!","~","--","++","V--","V++"}, true, 1));
            opPrecedence.Add(new OperatorCategory(new string[] {"^"}));           
            opPrecedence.Add(new OperatorCategory(new string[] {"*","/","%"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"-","+"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"<<",">>",">>>"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"<",">",">=","<="}));
            opPrecedence.Add(new OperatorCategory(new string[] {"==","!="}));
            opPrecedence.Add(new OperatorCategory(new string[] {"&"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"|||"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"|"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"&&"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"||"}));
            opPrecedence.Add(new OperatorCategory(new string[] {"?",":"}, true, 3));
            opPrecedence.Add(new OperatorCategory(new string[] {"=","+=","-=","*=","/=","%=",">>=","<<=",">>>=","&=","|||=","|="},true));
            opPrecedence.Add(new OperatorCategory(new string[] {","}));
            opPrecedence.Reverse();

            node program = new node(){body = "program"};
            List<Token> tokenBuffer = new List<Token>();

            for (int i = 0; i < tokensList.Count; i++) {
                Token token = tokensList[i];

                if (token.value == "eol") 
                {
                    node baseNode = new node () {body = "expression", depth = program.depth + 1};
                    program.nodes.Add(baseNode);
                    ParseExpression(baseNode,tokenBuffer);
                    tokenBuffer.Clear();
                    continue;
                }
        
                tokenBuffer.Add(token);
            }

            watch.Stop();

            Console.WriteLine("Parse time: " + watch.ElapsedMilliseconds + " ms");
            return program;
        }
    }
}
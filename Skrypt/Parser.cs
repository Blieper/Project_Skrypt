using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using Tokenisation;
using ErrorHandling;
using MethodBuilding;
using Execution;
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
                        
                        if (op == token.value) {

                            node newNode  = new node();
                            node addNode;
                            newNode.depth = branch.depth + 1;
                            newNode.body  = token.value;

                            // All of the tokens on the left
                            leftBuffer  = expression.GetRange(0,i);

                            // All of the tokens on the right
                            rightBuffer = expression.GetRange(i + 1,expression.Count - i - 1);  
                            
                            if (op == "dec" || op == "inc") {     
                               
                                if (leftBuffer.Count > 0 && rightBuffer.Count == 0) {
                                    newNode.body = "p" + Tokenizer.processPunctuator(op);
                                    addNode = ParseExpression (newNode, leftBuffer, true);
                                }else{
                                    newNode.body = Tokenizer.processPunctuator(op);
                                    addNode = ParseExpression (newNode, rightBuffer, true);
                                }
                                
                                if (addNode != null) 
                                    newNode.nodes.Add(addNode);                                    
                            }else{
                                // Parsing a node from the left buffer
                                if (cnt != 1) {
                                    addNode = ParseExpression (newNode, leftBuffer, true);
                                    if (addNode != null) 
                                        newNode.nodes.Add(addNode);
                                }

                                if (op == "return") {
                                    if (rightBuffer.Count > 0) {

                                        Console.WriteLine(stringList(rightBuffer));

                                        // Parsing a node from the right buffer
                                        addNode = ParseExpression (newNode, rightBuffer, true);

                                        if (addNode != null) 
                                            newNode.nodes.Add(addNode);
                                        
                                    }
                                }else{
                                    // Parsing a node from the right buffer
                                    addNode = ParseExpression (newNode, rightBuffer, true);
                                    if (addNode != null) 
                                        newNode.nodes.Add(addNode);
                                }
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

        static public node ParseBranch (node parent, List<Token> tokenList, string type) {
            // Initialise base node
            node baseNode = parent;
              
            node typeNode = new node() {body = "type", depth = baseNode.depth + 1};
            baseNode.nodes.Add(typeNode);

            node t = new node() {body = type, depth = typeNode.depth + 1};
            typeNode.nodes.Add(t);

            int i = 1;

            node addNode;

            if (type == "if" || type == "while" || type == "elseif") {
                node conditionNode = new node() {body = "condition", depth = baseNode.depth + 1};
                baseNode.nodes.Add(conditionNode);

                List<Token> conditionTokens = new List<Token>();
                Token tkn = tokenList[1];

                while (tkn.value != "lbracket") {
                    conditionTokens.Add(tkn);
                    i++;
                    tkn = tokenList[i];
                }

                if (conditionTokens.Count == 0) 
                    throw new SkryptException("Condition cannot be empty",tokenList[0]);

                addNode = ParseExpression(conditionNode,conditionTokens);

                if (addNode != null) 
                    conditionNode.nodes.Add(addNode);
            }
        
            node bodyNode = new node() {body = "body", depth = baseNode.depth + 1};
            baseNode.nodes.Add(bodyNode);

            List<Token> bodyTokens = tokenList.GetRange(i + 1,tokenList.Count - i - 2);
            bodyNode = ParseGlobal(bodyNode,bodyTokens);

            return baseNode;
        }

        static public node ParseMethodDeclaration (node branch, List<Token> tkns, node argsNode, List<Token> body) {
             // Initialise base node
            node baseNode = new node() {body = "methoddeclaration", depth = branch.depth + 1};    
            
            node typeNode = new node() {body = tkns[0].value, depth = baseNode.depth + 1};
            baseNode.nodes.Add(typeNode);            
            
            node identifierNode = new node() {body = tkns[1].value, depth = baseNode.depth + 1};
            baseNode.nodes.Add(identifierNode); 

            node bodyNode = new node() {body = "body", depth = baseNode.depth + 1};
            baseNode.nodes.Add(bodyNode); 

            argsNode.depth = baseNode.depth + 1;
            baseNode.nodes.Add(argsNode); 

            ParseGlobal(bodyNode, body);

            branch.nodes.Add(baseNode);

            return baseNode;
        }

        static public node ParseGlobal (node branch, List<Token> tokensList) {
            node newNode = branch;

            List<Token> tokenBuffer = new List<Token>();

            // Branching
            bool    isBranch            = false;
            int     branchDepth         = 0;
            string  branchType          = "";
            bool    prevWasIf           = false;
            bool    prevWasLiteralIf    = false;
            node    ifNode              = null;

            // Method declaration
            bool    isMethod        = false;
            bool    isMethodArgs    = false;
            bool    isMethodBody    = false; 
            bool    hasMethodArgs   = false;
            int     bracketDepth    = 0;
            node    argsNode        = null; 

            string  methodType = "";
            List<Token> methodArgs = new List<Token>();
            List<Token> methodBody = new List<Token>();
            List<Token> returnExpr = new List<Token>();

            for (int i = 0; i < tokensList.Count; i++) {
                Token token = tokensList[i];

                if (token.value == "lbracket")
                    bracketDepth++; 

                if (token.value == "rbracket")
                    bracketDepth--; 

                if (token.value == "function") {
                    isMethod = true;
                    continue;
                } else if (isMethod) {
                    methodType = token.value;

                    if (!hasMethodArgs && token.value == "lpar") {
                        isMethodArgs = true;
                        continue;
                    }

                    if (!hasMethodArgs && isMethodArgs && token.value == "rpar") {
                        isMethodArgs = false;
                        hasMethodArgs = true;

                        Console.WriteLine("Adding method " + tokenBuffer[1].value);
                        MethodHandler.Add(tokenBuffer[1].value,String.Empty,new string[0],(node)null);
                         
                        SkryptMethod found = MethodHandler.GetSk(tokenBuffer[1].value);

                        int foundIndex = MethodContainer.SKmethods.IndexOf(found);

                        MethodContainer.SKmethods[foundIndex].returnType = tokenBuffer[0].value;                       
                        argsNode = new node() {body = "arguments"};

                        int j = 0;
                        string[] argNames = new string[methodArgs.Count];
                        MethodContainer.SKmethods[foundIndex].arguments = new string[methodArgs.Count];

                        foreach (Token argTkn in methodArgs) {
                            MethodContainer.SKmethods[foundIndex].arguments[j] = argTkn.value;
                            argNames[j] = argTkn.value;
                            j++;
                        
                            if (argTkn.value == "seperate") 
                                continue;

                            argsNode.nodes.Add(new node() {body = argTkn.value, depth = argsNode.depth + 1});
                        }

 
                        continue;
                    }

                    if (isMethodArgs) {
                        methodArgs.Add(token);
                    }

                    if (!isMethodBody && hasMethodArgs && token.value == "lbracket") {
                        isMethodBody = true;
                        continue;
                    }

                    if (isMethodBody && token.value == "rbracket" && bracketDepth == 0) {
                        node MethodNode = ParseMethodDeclaration(newNode, tokenBuffer, argsNode, methodBody);

                        Executor.ExecuteMethodDeclaration(MethodNode);

                        isMethodBody = false;
                        //hasMethodBody = true;
                        tokenBuffer.Clear();
                        isMethod = false;
                        continue;
                    }

                    if (isMethodBody) {
                        methodBody.Add(token);
                    }

                    tokenBuffer.Add(token); 

                    continue;                 

                } else if (token.value == "if" || token.value == "while" || token.value == "for" || token.value == "else" || token.value == "elseif") {
                    branchDepth++;
                    branchType = token.value;

                    isBranch = true;
                }else if (isBranch) {
                    if (token.value == "rbracket") {
                        branchDepth--;

                        if (branchDepth == 0) {
                            node baseNode  = new node () {body = "branch", depth = newNode.depth + 1};
                            tokenBuffer.Add(token);
                            
                            if ((branchType == "else" || branchType == "elseif")) { 

                                if ((branchType == "else" || branchType == "elseif") && !prevWasIf)                     
                                    throw new SkryptException(branchType + " has to be preceeded by if/elseif statement", tokenBuffer[0]);

                                if ((branchType == "elseif") && !prevWasLiteralIf) 
                                    throw new SkryptException(branchType + " has to be preceeded by if statement", tokenBuffer[0]);

                            
                                if (branchType == "elseif")   
                                    prevWasIf = true;

                                node addNode  = new node () {body = "secondary", depth = ifNode.depth + 1};
                                ifNode.nodes.Add(addNode); 
                                 
                                node branchNode = ParseBranch(addNode,tokenBuffer,branchType);
                                ifNode = branchNode;

                                prevWasLiteralIf = false;
                                
                                tokenBuffer.Clear();
                                continue;
                            }else if (branchType == "if") {
                                prevWasLiteralIf = true;
                                prevWasIf = true;

                                ifNode = ParseBranch(baseNode,tokenBuffer,branchType);

                                newNode.nodes.Add(baseNode); 

                                tokenBuffer.Clear();
                                continue;
                            }

                            tokenBuffer.Clear();
                            continue;
                        }
                    }
                }else if (token.value == "eol") 
                {
                    node baseNode = new node () {body = "expression", depth = newNode.depth + 1};
                    newNode.nodes.Add(baseNode);

                    ParseExpression(baseNode,tokenBuffer);
                    tokenBuffer.Clear();
                    continue;
                }

                tokenBuffer.Add(token);
            }

            return newNode;
        }

        static public node ParseTokens (List<Token> tokensList) 
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //opPrecedence.Add(new OperatorCategory(new string[] {"(",")","."}));
            opPrecedence.Add(new OperatorCategory(new string[] {"!","~","--","++"}, true, 1));
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
            opPrecedence.Add(new OperatorCategory(new string[] {"return"},true,1));            
            opPrecedence.Reverse();

            node program = new node(){body = "program"};

            program = ParseGlobal(program, tokensList);

            watch.Stop();

            Console.WriteLine("Parse time: " + watch.ElapsedMilliseconds + " ms");
            return program;
        }
    }
}
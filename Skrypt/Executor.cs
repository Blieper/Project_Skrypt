using System;
using System.Collections.Generic;
using Tokenisation;
using Parsing;
using MethodBuilding;
using LibraryMaster;
using ErrorHandling;
using static MethodBuilding.MethodContainer;

namespace Execution {

    public class Variable {
        public string Identifier;
        public string Type;
        public object Value;

        public Variable (string Id, string T = "undefined", object V = null) {
            Identifier = Id;
            Type = T;
            Value = V;
        }
    }

    static public class Executor {
        static List<Variable> Variables = new List<Variable>();

        static public void ExecuteProgram (node Program) {
            foreach (node Node in Program.nodes) {
                switch (Node.body) {
                    case "expression":
                        ExecuteExpression(Node.nodes[0]);
                    break;
                }
            }
        }

        static Variable GetVariableWithType (node Expr) {

            object Value = null;
            string Type = "";

            switch (Expr.type) {
                case TokenType.Boolean:
                    Type = "bool";
                    Value = Expr.Value;                
                break;
                case TokenType.String:   
                    Type = "string";
                    Value = Expr.Value;
                break;                            
                case TokenType.Numeric:
                    Type = "numeric";
                    Value = Expr.Value;
                break;
                case TokenType.Identifier:
                    if (Variables.Exists(x => x.Identifier == Expr.body)) {
                        Variable v = Variables.Find(x => x.Identifier == Expr.body);
                        Value = v.Value;
                        Type = v.Type;
                    }else{
                        throw new SkryptVariableDoesNotExistException(Expr.body);
                    }
                break;
                default:
                break;
            }

            Variable variable = new Variable(Expr.body, Type, Value);

            return variable;
        }

        static public Variable ExecuteExpression (node Expr) {
            string body = Expr.body;

            if (Expr.body == "method") 
                return ExecuteMethod(Expr.nodes[0]);

            if (Expr.nodes.Count > 0) {
                node Left = Expr.nodes[0];
                node Right = null;
                Variable temp = new Variable("");

                if (Expr.nodes.Count > 1)
                    Right = Expr.nodes[1];

                if (Right != null) {
                    
                    Variable RightVar = ExecuteExpression(Right);                                    

                    if (body == "assign") { 
                        Variable Var;

                        if (Variables.Exists(x => x.Identifier == Left.body)) {
                            Var = Variables.Find(x => x.Identifier == Left.body);
                        }else{
                            Var = new Variable(Left.body);
                            Variables.Add(Var);
                        }

                        if (Var.Type == "undefined" || (Var.Type == RightVar.Type)) {
                            Var.Value = RightVar.Value;
                            Var.Type = RightVar.Type;
                        }

                        return Var;
                    }

                    Variable LeftVar  = ExecuteExpression(Left);

                    if (Left.type == TokenType.Identifier) {
                        if (!Variables.Exists(x => x.Identifier == Left.body)) {
                            throw new SkryptVariableDoesNotExistException(Left.body);
                        }
                    }else if (Right.type == TokenType.Identifier) {
                        if (!Variables.Exists(x => x.Identifier == Right.body)) {
                            throw new SkryptVariableDoesNotExistException(Right.body);
                        }                                
                    }

                    switch (body) {
                        case "add":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToDouble(LeftVar.Value) + Convert.ToDouble(RightVar.Value);
                                    temp.Type = "numeric";
                                    return temp;
                                case "string_numeric":
                                    temp.Value = Convert.ToString(LeftVar.Value) + Convert.ToDouble(RightVar.Value);
                                    temp.Type = "string";
                                    return temp;
                                case "numeric_string":
                                    temp.Value = Convert.ToDouble(LeftVar.Value) + Convert.ToString(RightVar.Value);
                                    temp.Type = "string";
                                    return temp;
                                case "string_bool":
                                    temp.Value = Convert.ToString(LeftVar.Value) + Convert.ToBoolean(RightVar.Value);
                                    temp.Type = "string";
                                    return temp;
                                case "bool_string":
                                    temp.Value = Convert.ToBoolean(LeftVar.Value) + Convert.ToString(RightVar.Value);
                                    temp.Type = "string";
                                    return temp;  
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body);                                
                            }
                        case "sub":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToDouble(LeftVar.Value) - Convert.ToDouble(RightVar.Value);
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "mul":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToDouble(LeftVar.Value) * Convert.ToDouble(RightVar.Value);
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "div":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToDouble(LeftVar.Value) / Convert.ToDouble(RightVar.Value);
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "mod":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToDouble(LeftVar.Value) % Convert.ToDouble(RightVar.Value);
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "pow":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Math.Pow(Convert.ToDouble(LeftVar.Value),Convert.ToDouble(RightVar.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "band":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToInt32(Convert.ToDouble(LeftVar.Value)) & Convert.ToInt32(Convert.ToDouble(RightVar.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "bor":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToInt32(Convert.ToDouble(LeftVar.Value)) | Convert.ToInt32(Convert.ToDouble(RightVar.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "bxor":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToInt32(Convert.ToDouble(LeftVar.Value)) ^ Convert.ToInt32(Convert.ToDouble(RightVar.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "bshl":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToInt32(Convert.ToDouble(LeftVar.Value)) << Convert.ToInt32(Convert.ToDouble(RightVar.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "bshr":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = Convert.ToInt32(Convert.ToDouble(LeftVar.Value)) >> Convert.ToInt32(Convert.ToDouble(RightVar.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }
                        case "bsh0":
                            switch (LeftVar.Type + "_" + RightVar.Type) {
                                case "numeric_numeric":
                                    temp.Value = (double)((uint)Convert.ToInt32(Convert.ToDouble(LeftVar.Value)) >> Convert.ToInt32(Convert.ToDouble(RightVar.Value)));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(LeftVar.Type,RightVar.Type,Expr.body); 
                            }                            
                    }
                }else{
                    Variable variable = ExecuteExpression(Expr.nodes[0]);
                    bool OnExistingVar = Variables.Exists(x => x.Identifier == Expr.nodes[0].body);

                    Variable ExistingVar = null; 

                    if (OnExistingVar) 
                        ExistingVar = Variables.Find(x => x.Identifier == Expr.nodes[0].body);

                    switch (body) {
                        case "not":
                            switch (variable.Type) {
                                case "bool":
                                    temp.Value = !Convert.ToBoolean(variable.Value);
                                    temp.Type = "bool";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(null,variable.Type,body); 
                            }  
                        case "bnot":
                            switch (variable.Type) {
                                case "numeric":
                                    temp.Value = ~Convert.ToInt32(Convert.ToDouble(variable.Value));
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(null,variable.Type,body); 
                            } 
                        case "pinc":
                            switch (variable.Type) {
                                case "numeric":
                                    temp.Value = Convert.ToDouble(ExistingVar.Value) + 1;
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(null,variable.Type,body); 
                            }  
                        case "inc":
                            switch (variable.Type) {
                                case "numeric":
                                    if (OnExistingVar) {
                                        temp.Value = Convert.ToDouble(ExistingVar.Value);
                                        temp.Type = "numeric";

                                        ExistingVar.Value = Convert.ToDouble(ExistingVar.Value) + 1;

                                        return temp;                                        
                                    }else{
                                        throw new SkryptInvalidOperationException(null,"non-variable",body);
                                    }
                                default:
                                    throw new SkryptInvalidOperationException(null,variable.Type,body); 
                            } 
                        case "pdec":
                            switch (variable.Type) {
                                case "numeric":
                                    temp.Value = Convert.ToDouble(ExistingVar.Value) - 1;
                                    temp.Type = "numeric";
                                    return temp;
                                default:
                                    throw new SkryptInvalidOperationException(null,variable.Type,body); 
                            }                              
                        case "dec":
                            switch (variable.Type) {
                                case "numeric":
                                    if (OnExistingVar) {
                                        temp.Value = Convert.ToDouble(ExistingVar.Value);
                                        temp.Type = "numeric";

                                        ExistingVar.Value = Convert.ToDouble(ExistingVar.Value) - 1;

                                        return temp;                                        
                                    }else{
                                        throw new SkryptInvalidOperationException(null,"non-variable",body);
                                    }
                                default:
                                    throw new SkryptInvalidOperationException(null,variable.Type,body); 
                            }                                                                                   
                    }
                }
            }

            return GetVariableWithType(Expr);
        }
        
        static public Variable ExecuteMethod (node methodNode) {
            if (!MethodHandler.Exists(methodNode.body))
                throw new SkryptMethodDoesNotExistException(methodNode.body);

            Method method = MethodHandler.Get(methodNode.body);
            string type = methodNode.nodes[0].body;
            List<node> argNodes;
            object[] args = new object[0];

            if (method.arguments.Length > 0 && method.arguments != null) {
                argNodes = methodNode.nodes[1].nodes;
                args = new object[argNodes.Count];

                int i = 0;

                foreach (node argNode in argNodes) {
                    node Expr = argNode.nodes[0];
                    Variable solved = ExecuteExpression(Expr);
                    
                    args[i] = solved.Value;

                    i++;
                }
            }

            Variable returnVariable = method.Run(args);

            return returnVariable;
        }

        static public void Run (string Code, bool printAST = false, bool printTokens = false) {
            List<Token> tokens;
            node program;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            LibraryMaster.LibraryHandler.Initialise();

            tokens  = Tokenisation.Tokenizer.GetTokens(Code);

            if (printTokens){
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

            program = parser.ParseTokens(tokens);

            if (printAST) {
                Console.WriteLine(program);
            }

            ExecuteProgram(program);

            watch.Stop();
            Console.WriteLine("Total time: " + watch.ElapsedMilliseconds + " ms");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Tokenisation
{

    public class Token
    {
        public TokenType type;
        public string value;
        public int line;
        public int col;

        public Token(TokenType t, string v, int l, int c)
        {
            type    = t;
            value   = v;
            line    = l;
            col     = c;           
        }

        public override string ToString()
        {
            return "Token Type: " + type + "\tValue: " + value;
        }
    }

    class CodeChunk
    {
        public string chunk;
        public int line;
        public int col;

        public CodeChunk(string ch, int li, int co)
        {
            chunk = ch;
            line = li;
            col = co;
        }

        public override string ToString()
        {
            return "Code chunk: \n\tChunk: " + chunk + "\n\tLine: " + line + "\n\tColumn: " + col;
        }
    }

    enum NumberType : byte {
        Binary,
        Decimal,
        Hexadecimal,
        None,
    }

    public enum TokenType : byte {
        Punctuator,
        Identifier,
        String,       
        Keyword,
        Numeric,
        Boolean,
        None,
    }

    static class Tokenizer
    {
        static List<Token> tokensList = new List<Token>();

        static int line = 1;
        static int col = 0;

        static Regex stringRegex        = new Regex(@"[""\'][^""\\]*(?:\\.[^""\\]*)*[""\']");
        static Regex binaryRegex        = new Regex(@"0b([01])+");
        static Regex decimalRegex       = new Regex(@"-?\d+(\.\d*)?([eE][-+]?\d+)?", RegexOptions.IgnoreCase);
        static Regex hexadecimalRegex   = new Regex(@"0x([A-Fa-f\d])+");
        static Regex commentRegex       = new Regex(@"(?:[/]{2,}(.*))|((/\*)+[\s\S]*(\*/)+)");
        static Regex punctuatorRegex    = new Regex(@"(&&)|(\|\|)|(\|\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>)|(>>>)|(\+\+)|(--)|[~=;<>+\-*/%^&|!\[\]\(\)\.\,{}]");
        static Regex identifierRegex    = new Regex(@"(?:[_a-zA-Z]+[_a-zA-Z0-9]*)");
        static Regex booleanRegex       = new Regex(@"(true|false)");
        static Regex keywordRegex       = new Regex(@"(if|else|elseif|class|this|new|null|public|private|protected|return|base|def|for|foreach|while|break|continue|switch|case|default|throw|string|numeric|bool|void)");

        public static NumberType getNumberType(string String)
        {
            // Binary
            string binaryMatch = binaryRegex.Match(String).Value;

            if (String == binaryMatch) { return NumberType.Binary; }

            // Decimal
            string decimalMatch = decimalRegex.Match(String).Value;

            if (String == decimalMatch) { return NumberType.Decimal; }

            // Hexadecimal
            string hexadecimalMatch = hexadecimalRegex.Match(String).Value;

            if (String == hexadecimalMatch) { return NumberType.Hexadecimal; }

            // None
            return NumberType.None;
        }

        public static string processNumber(string number)
        {
            if (getNumberType(number) == NumberType.Decimal)
            {
                number = number.Replace("e", "E");
                number = "" + Decimal.Parse(number, System.Globalization.NumberStyles.Any);
            }
            else
            if (getNumberType(number) == NumberType.Binary)
            {
                number = number.Substring(2);
                number = "" + Convert.ToInt32(number, 2);
            }
            else
            if (getNumberType(number) == NumberType.Hexadecimal)
            {
                number = number.Substring(2);
                number = "" + int.Parse(number, NumberStyles.HexNumber);
            }

            return number;
        }

        public static string processPunctuator (string Punctuator) 
        {
            switch (Punctuator) {
                case "=":
                    return "assign";  
                case ";":
                    return "eol";  
                case ".":
                    return "access";  
                case ",":
                    return "seperate";                      
                case "!":
                    return "not";   
                case "==":
                    return "is";               
                case "!=":
                    return "isnot";     
                case "&&":
                    return "and";     
                case "||":
                    return "or";    
                case "<":   
                    return "smaller"; 
                case ">":  
                    return "greater";                                  
                case ">=":
                    return "isgreater";     
                case "<=":
                    return "issmaller";  
                case "(":
                    return "lpar";
                case ")":
                    return "rpar";  
                case "{":
                    return "lbracket";  
                case "}":   
                    return "rbracket";       
                case "[":
                    return "laccess";  
                case "]":   
                    return "raccess";                                   
                case "+":
                    return "add";
                case "-":
                    return "sub";  
                case "*":
                    return "mul";  
                case "/":   
                    return "div";         
                case "%":
                    return "mod";  
                case "^":   
                    return "pow";  
                case "++":
                    return "inc";  
                case "--":   
                    return "dec";  
                case "&":   
                    return "band";  
                case "|":   
                    return "bor";  
                case "|||":   
                    return "bxor";                       
                case "~":   
                    return "bnot";         
                case "<<":   
                    return "bshl"; 
                case ">>":   
                    return "bshr"; 
                case ">>>":   
                    return "bsh0";                                                     
                default:
                    return Punctuator;
            }
        }

        public static List<CodeChunk> splitCode(string value, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            value += '\n';

            string[] separators = { 
            " ", 
            ";",    // end of operation 
            "!",    // not operator  
            "(",    // left parenthesis  
            ")",    // right parenthesis 
            "{",    // right curly bracket 
            "}",    // right curly bracket 
            "[",    // left array accessor 
            "]",    // right array accessor
            "==",
            "<=",   // is or is smaller than
            ">=",   // is or is greater than  
            ">>>",  // bit shift right zero fill             
            ">>",   // bit shift right
            ">",    // is greater than        
            "<<",   // bit shift left                          
            "<",    // is smaller than
            "=",    // assign operator  
            ".",    // accessor     
            ",",    // seperator    
            "//",   // Comment
            "/*",   // Multi line comment start
            "&&",   // and operator
            "|||",  // bitwise xor operator           
            "||",   // or operator       
            "!=",   // is not operator   
            "++",   // increment operator    
            "--",   // decrement operator                        
            "+",    // plus operator  
            "-",    // minus operator  
            "/",    // divide operator  
            "*",    // multiply operator  
            "^",    // power operator  
            "%",    // modulus operator  
            "&",    // bitwise and operator  
            "|",    // bitwise or operator             
            "~",    // bitwise not operator                
            "\t", 
            "\n"
            };

            List<CodeChunk> splitValues = new List<CodeChunk>();
            int itemStart = 0;

            string Chunk = "";
            bool foundString    = false;
            bool foundComment   = false;
            bool multiComment   = false;
            bool foundIncremental = false;
            char stringChar     = '\"';

            int chunkCol   = 1;
            int chunkLine  = 1;
            
            int  punctuatorLength = 0;

            for (int pos = 0; pos < value.Length; pos++)
            {
                char Character = value[pos];
                
                if (punctuatorLength > 0)
                {
                    punctuatorLength--;
                    continue;
                }

                Chunk += Character;

                bool foundNumber = false;
                    
                // Line & column counter
                if (Character == '\n')
                {
                    line++;
                    col = 1;
                }
                else
                {
                    col++;
                }

                // Is it a string?
                if (foundString)
                {
                    if (Character == '\\')
                    {
                        pos++;
                        col++;
                        continue;
                    }

                    if (Character == stringChar)
                    {
                        foundString = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (Character == '\"' || Character == '\'')
                    {
                        stringChar = Character;
                        foundString = true;
                    }
                }

                // Is it a comment?
                if (foundComment)
                {
                    if (Character == '\n')
                    {
                        foundComment = false;
                    }
                    else
                    {
                        continue;
                    }
                }

                // Is it a multi-line comment?
                if (multiComment)
                {
                    if (Character == '*' && value[pos + 1] == '/')
                    {
                        multiComment = false;
                        pos += 2;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (foundIncremental) {
                    
                }

                // Check for each symbol
                for (int sepIndex = 0; sepIndex < separators.Length; sepIndex++)
                {
                    string separator    = separators[sepIndex];
                    string lookForward  = "";
                    int    lf           = pos;

                    while (lf < (pos + separator.Length) && lf < value.Length) {
                        lookForward += value[lf];     
                        lf++;           
                    }

                    // Is the current character one of the symbols?
                    if (separator == lookForward)
                    {
                        // Is it a single-line comment?
                        if (separator == "//") {
                            foundComment = true;
                            break;
                        }

                        // Is it a multi-line comment?
                        if (separator == "/*") {
                            multiComment = true;
                            break;
                        }

                        // Is it part of a positive number?
                        if ((getNumberType(Chunk) != NumberType.None))
                        {
                            foundNumber = true;
                            break;
                        }

                        // Is it part of a negative number?
                        if (Chunk.Length == 1 && separator == "-" && (getNumberType("" + value[pos + 1]) != NumberType.None))
                        {
                            foundNumber = true;
                            break;                            
                        } 

                        if ((separator == "++" || separator == "--") && getTokenType("" + value[pos + 2]) == TokenType.Identifier) {
                            //foundIncremental = true;
                            //break;
                        }

                        // add the section of string before the separator 
                        // (unless its empty and we are discarding empty sections)
                        if (itemStart != pos || splitOptions == StringSplitOptions.None)
                        {
                            if (pos - itemStart >= 0) {
                                string toAdd = value.Substring(itemStart, pos - itemStart).Trim();
                                if (!String.IsNullOrWhiteSpace(toAdd))
                                {
                                    splitValues.Add(new CodeChunk(toAdd, chunkLine, chunkCol));
                                }
                            }
                        }

                        itemStart = pos + separator.Length;

                        // add the separator
                        if (!String.IsNullOrWhiteSpace(separator.ToString()))
                        {
                            splitValues.Add(new CodeChunk(separator.ToString(), line, col - 1));
                        }

                        Chunk       = "";
                        chunkCol    = col;
                        chunkLine   = line;

                        punctuatorLength = separator.Length - 1; 

                        break;
                    }

                    if (foundNumber)
                    {
                        continue;
                    }
                }
            }


            return splitValues;
        }

        private static Token addToken(TokenType type, string value, int l, int c)
        {
            if (String.IsNullOrWhiteSpace(value)) { return new Token(TokenType.None,String.Empty,0,0); }

            // Pre-calculate numbers
            if (getNumberType(value) != NumberType.None)
            {
                value = processNumber(value);
            }

            // set to proper token name
            if (getTokenType(value) == TokenType.Punctuator)
            {
                value = processPunctuator(value);
            }

            Token token = new Token(type, value, l , c);

            tokensList.Add(token);

            return token;
        }

        public static TokenType getTokenType (string value) {
  
            if (stringRegex.IsMatch(value)) {
                return TokenType.String;
            }

            if (getNumberType(value) != NumberType.None) {
                return TokenType.Numeric;
            }      

            if (punctuatorRegex.IsMatch(value)) {
                return TokenType.Punctuator;
            } 

            if (booleanRegex.IsMatch(value)) {
                return TokenType.Boolean;
            }   

            if (keywordRegex.IsMatch(value)) {
                return TokenType.Keyword;
            }

            if (identifierRegex.IsMatch(value)) {
                return TokenType.Identifier;
            }  

            return TokenType.None;
        }

        public static List<Token> GetTokens(string Code)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<CodeChunk> splitArray = splitCode(Code);

            Token previous = new Token(TokenType.None,String.Empty,0,0);

            foreach (CodeChunk bit in splitArray)
            {                   
                if (commentRegex.IsMatch(bit.chunk)) {
                    continue;
                }    

                TokenType Type = getTokenType (bit.chunk);

                if (previous.type == TokenType.Identifier || previous.type ==  TokenType.Numeric) {
                    if (Type == TokenType.Numeric) {
                        if (bit.chunk[0] == '-') {
                            addToken(TokenType.Punctuator, "-", bit.line, bit.col);
                            addToken(Type, bit.chunk.Substring(1), bit.line, bit.col + 1); 

                            continue;                       
                        }
                    }
                }

                previous = addToken(Type, bit.chunk, bit.line, bit.col);
            }

            watch.Stop();

            Console.WriteLine("Tokens: {0}", splitArray.Count);
            Console.WriteLine("Lines: {0}", line);
            Console.WriteLine("Tokenize time: {0} ms", watch.ElapsedMilliseconds);

            return tokensList;
        }
    }
}
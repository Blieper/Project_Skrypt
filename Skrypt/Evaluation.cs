using System;

namespace Evaluation
{
    class Evaluator {
        public static string[] operators = {
            "-",
            "+",
            "/",
            "*",
            "^"      
        };

        public int IndexOfREV (string Source, string Search, int Start) 
        {
            int n = 0;
            int bopen = 0;
            int bclose = 0;
            string sign;

            n = Start;

            if (n > Source.Length - 1) {n = Source.Length - Search.Length;}

            while (n > 0) 
            {
                sign = Source.Substring(n, Search.Length);
                
                if ((sign == Search) && (bopen == bclose)) {return n;}
                if (sign[0] == '(') {bopen++;}
                if (sign[0] == ')') {bclose++;}
                n--;
            }

            return -1;
        }

        public bool isOperator (string Expr, string Op, int n) 
        {
            string sign;

            if (Op == "+") {
                if (Expr.Substring(n-1,1).ToUpper() == "E") {
                    if (n > 2) {
                        

                        if ("1234567890.".IndexOf(Expr.Substring(n-2,1)) > -1) {
                            return false;
                        }
                    }
                }

                return true;
            }

            if (Op == "-") {
                if (n == 0) {
                    return false;
                }else{
                    sign = Expr.Substring(n);
                    sign = sign.TrimEnd().Substring(0,1);

                    if ("+-/*^".IndexOf(sign) > -1) {
                        return false;
                    }

                    if (Expr.Substring(n-1,1).ToUpper() == "E" && n > 2) {
                        if ("1234567890.".IndexOf(Expr.Substring(n-2,1)) > -1) {
                            return false;
                        }                        
                    }
                }

                return true;
            }

            return true;
        }
        
        public Evaluator()
        {
            //Console.WriteLine(isOperator("2.5 * -4","-",6));
        }

        public double SolveNumber (string Expression) 
        {
            string Expr = Expression.Trim();
            string Left, Right;
            string Operator;

            for (int i = 0; i <= operators.Length-1; i++) {
                Operator = operators[i];

                int Pos = IndexOfREV(Expr, Operator, 10000000);
                
                if (Pos == -1) {continue;}
                
                while (Pos > -1) 
                {
                    if (isOperator(Expr, Operator, Pos)) 
                    {
                        Left = Expr.Substring(0,Pos - 1).Trim();
                        Right = Expr.Substring(Pos + 1).Trim();

                        switch (Operator) 
                        {
                            case "-":
                                return SolveNumber(Left) - SolveNumber(Right);             
                            case "+":
                                return SolveNumber(Left) + SolveNumber(Right);             
                            case "/":
                                return SolveNumber(Left) / SolveNumber(Right);             
                            case "*":
                                return SolveNumber(Left) * SolveNumber(Right);             
                            case "^":
                                return Math.Pow(SolveNumber(Left), SolveNumber(Right));                                                    
                        }
                    }

                    if (Pos > -1) {Pos = IndexOfREV(Expr.ToUpper(), Operator, Pos-1);}
                }               
            }

            if (Expr[0] == '(' && Expr[Expr.Length - 1] == ')') {
                Expr = Expr.Substring(1,Expr.Length - 2);
                return SolveNumber(Expr);
            }

            return Convert.ToDouble(Expr);
        }
    }
}
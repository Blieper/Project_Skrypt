using System;
using MethodBuilding;
using static MethodBuilding.MethodContainer;

namespace CoreLibrary {
    static public class Library {
        static public void Initialise () {
            MDelegate f = delegate (object[] i)  {
                Console.WriteLine(i[0]);

                return null;
            };

            MethodHandler.Add("print","void", new string[] {"input"}, f);

            f = delegate (object[] i)  {
                return Math.Sqrt(Convert.ToDouble(i[0]));
            };

            MethodHandler.Add("sqrt","numeric", new string[] {"input"}, f);

            f = delegate (object[] i)  {
                return Math.E;
            };

            MethodHandler.Add("e","numeric", new string[0], f);                       
        }
    }
}
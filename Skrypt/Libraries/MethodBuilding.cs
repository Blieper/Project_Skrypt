using System;
using System.Collections.Generic;
using System.Globalization;
using Execution;

namespace MethodBuilding {

    public static class MethodContainer {
        static public List<Method> methods = new List<Method>();

        public delegate object MDelegate (params object[] input);

        public class Method {
            public string identifier;
            public string returnType;
            
            public string[] arguments;
            public MDelegate method;

            public Method (string id, string rt, string[] args, MDelegate m) {
                identifier = id;
                returnType = rt;

                arguments = args;
                method = m;
            }

            public Variable Run (params object[] input) {
                Variable returnVariable = new Variable(string.Empty);

                returnVariable.Value = method(input);
                returnVariable.Type  = returnType;

                return returnVariable;
            }
        }

        public static class MethodHandler { 
            static public bool Exists (string name) {
                return methods.Exists(x => x.identifier == name);
            }

            static public Method Get (string name) {
                return methods.Find(x => x.identifier == name);
            }

            static public Method Add (string identifier, string returnType, string[] arguments, MDelegate function) {

                Method method = new Method(
                    identifier,
                    returnType,
                    arguments, 
                    function
                );

                methods.Add(method);

                return method;
            }
        }
    }
}
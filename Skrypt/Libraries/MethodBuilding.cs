using System;
using System.Collections.Generic;
using System.Globalization;
using Parsing;
using Execution;

namespace MethodBuilding {

    public static class MethodContainer {
        static public List<Method> methods = new List<Method>();
        static public List<SkryptMethod> SKmethods = new List<SkryptMethod>();

        public delegate object MDelegate (params object[] input);

        public class Method {
            public string identifier;
            public string returnType;
            
            public string[] arguments;
            public MDelegate method;
            public bool predefined = true;

            public virtual void SetMethod (object m) {
                method = (MDelegate)m;
            }

            public Method (string id, string rt, string[] args, object m) {
                identifier = id;
                returnType = rt;

                arguments = args;
                SetMethod(m);
            }

            public virtual Variable Run (params object[] input) {
                Variable returnVariable = new Variable(string.Empty);

                returnVariable.Value = method(input);
                returnVariable.Type  = returnType;

                return returnVariable;
            }
        }

        public class SkryptMethod : Method {
            public node methodNode;

            public override void SetMethod (object m) {
                methodNode = (node)m;
            }

            public SkryptMethod (string id, string rt, string[] args, object m) : base(id, rt, args, m)  {
                predefined = false;
                identifier = id;
                returnType = rt;

                arguments = args;
                SetMethod(m);
            }

            public Variable Run (params Variable[] input) {
                Variable returnVariable = new Variable(string.Empty,"return");
                Executor.Variables.Add(returnVariable);

                Executor.ExecuteProgram(methodNode, input);

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

            static public SkryptMethod GetSk (string name) {
                return SKmethods.Find(x => x.identifier == name);
            }

            static public Method InSkrypt (string name) {
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

            static public Method Add (string identifier, string returnType, string[] arguments, node methodNode) {

                SkryptMethod method = new SkryptMethod(
                    identifier,
                    returnType,
                    arguments, 
                    methodNode
                );

                methods.Add(method);
                SKmethods.Add(method);

                return method;
            }            
        }
    }
}
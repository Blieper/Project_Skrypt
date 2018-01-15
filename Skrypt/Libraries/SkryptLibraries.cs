using System;
using System.Collections.Generic;
using System.Globalization;

namespace SkryptLibraries {

    public class Parameter {
        public string identifier;       
        //public string type;

        public Parameter (string id) {
            identifier = id;
            //type = t;
        }
    }

    public class Method {
        public string identifier;
        public string returnType;
        
        public List<Parameter> arguments;

        public Method (string id, string rt, List<Parameter> args) {
            identifier = id;
            returnType = rt;

            arguments = args;
        }
    }

    public partial class Library { 
        public List<Method> methods = new List<Method>();

        public bool Exists (string name) {
            return methods.Exists(x => x.identifier == name);
        }

        public Method Get (string name) {
            return methods.Find(x => x.identifier == name);
        }

        public Library () {
            List<Parameter> args = new List<Parameter>();
            Parameter par1 = new Parameter("input");
            args.Add(par1);

            methods.Add(new Method("print","void",args));
        }
    }
}

using CoreLibrary;
using MethodBuilding;
using static MethodBuilding.MethodContainer;

namespace LibraryMaster {
    static public class LibraryHandler {
        static public void Initialise() {
            CoreLibrary.Library.Initialise();

            foreach (Method m in methods) {
                m.Run(1);
            }
        }
    }    
}   
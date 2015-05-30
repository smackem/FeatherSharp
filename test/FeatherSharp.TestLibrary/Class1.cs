using FeatherSharp.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * This assembly serves as feathering object for debugging sessions,
 * which can verify debug symbol preservation.
 */ 

namespace FeatherSharp.TestLibrary
{
    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class Class1 : NotifyPropertyChanged
    {
        public int MyProperty1 { get; set; }
        public int MyProperty2 { get; set; }

        public int MyProperty1Plus2
        {
            get { return MyProperty1 + MyProperty2; }
        }
    }

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class Class2 : Class1
    {
        public int MyProperty3 { get; set; }

        public int MyProperty3Plus1
        {
            get { return MyProperty1 + MyProperty3; }
        }
    }
}

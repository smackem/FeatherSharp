using FeatherSharp.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Test.Resources
{
    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClass1 : INotifyPropertyChanged
    {
        public int TestProperty_Int { get; set; }
        public object TestProperty_Object { get; set; }
        public DateTime TestProperty_Struct { get; set; }
        public string TestProperty_String { get; set; }

        double _testField1;

        public double TestProperty_Complex
        {
            get { return _testField1; }
            set
            {
                _testField1 = value;

                if (_testField1 > 0.0)
                    Console.WriteLine("abc");
            }
        }

        bool _testField2;

        public bool TestProperty_WithBodies
        {
            get { return _testField2; }
            set
            {
                _testField2 = value;
            }
        }

        float _testField3;

        public float TestProperty_WithRaise
        {
            get { return _testField3; }
            set
            {
                _testField3 = value;
                OnPropertyChanged("TestProperty_WithRaise");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithBase : TestClassBase
    {
        public int MyProperty { get; set; }
    }

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassBase : INotifyPropertyChanged
    {
        public int MyBaseProperty { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithouthInpc
    {
        public int MyProperty { get; set; }
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithouthRaiseMethod : INotifyPropertyChanged
    {
        public int MyProperty { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithOverride : TestClassWithOverrideBase
    {
        public int MyProperty { get; set; }

        public bool OverrideMethodCalled;

        protected override void OnPropertyChanged(string propertyName)
        {
            OverrideMethodCalled = true;

            base.OnPropertyChanged(propertyName);
        }
    }

    public class TestClassWithOverrideBase : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithInaccessibleRaiseMethod : TestClassWithInaccessibleRaiseMethodBase
    {
        public int MyProperty { get; set; }
    }

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithInaccessibleRaiseMethodBase : INotifyPropertyChanged
    {
        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithDependenciesBase : NotifyPropertyChanged
    {
        public int A { get; set; }
        public int B
        {
            get { return A + 10; }
        }
    }

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithDependencies : TestClassWithDependenciesBase
    {
        public int C { get; set; }

        public int D
        {
            get { return A + C; }
        }

        public int E
        {
            get { return D + B; }
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassWithIgnoredProperty : NotifyPropertyChanged
    {
        public int MyProperty { get; set; }

        [FeatherIgnore(FeatherAction.NotifyPropertyChanged)]
        public int MyIgnoredProperty { get; set; }
    }

    [Feather(FeatherAction.NotifyPropertyChanged)]
    public class TestClassContainer : NotifyPropertyChanged
    {
        public int MyContainerProperty { get; set; }

        [Feather(FeatherAction.NotifyPropertyChanged)]
        class Nested : INotifyPropertyChanged
        {
            public int MyNestedProperty { get; set; }

            void OnPropertyChanged(string propertyName)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}

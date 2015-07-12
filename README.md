# FeatherSharp
Feather# - An AOP utility for .NET, based on Mono.Cecil.

## Features
* INotifyPropertyChanged Injection w/ Property Dependencies
* Logger TypeName and MethodName Injection
* Merge Assemblies
* Compatible with .NET 4.5 and Mono 4

## Usage
    [mono] FeatherSharp.exe <Feathers> <FileName>
      Available Feathers:
      -npc : Inject NotifyPropertyChanged
      -merge : Merge dependencies into <FileName>
      -log : Inject augmented log method calls
      Options:
      --sn=<StrongNameFile> : Re-sign with strong name

## INotifyPropertyChanged Injection
#### Feather# turns byte code doing this...
```c#
class Class1 // ...
{
    public int MyPropertyA { get; set; }
    public string MyPropertyB { get; set; }

    public string Combined
    {
        get { return MyPropertyA.ToString() + MyPropertyB; }
    }
    // ...
}
```
#### ...into byte code doing this
```c#
class Class1 // ...
{
    private int a;
    private string b;

    public int MyPropertyA
    {
        get { return this.a; }
        set
        {
            if (value != this.a)
            {
                this.a = value;

                OnPropertyChanged("MyPropertyA");
                OnPropertyChanged("Combined");
            }
        }
    }
    
    public string MyPropertyB
    {
        get { return this.b; }
        set
        {
            if (value != this.b)
            {
                this.b = value;

                OnPropertyChanged("MyPropertyB");
                OnPropertyChanged("Combined");
            }
        }
    }

    public string Combined
    {
        get { return MyPropertyA.ToString() + MyPropertyB; }
    }
    // ...
}
```

Implement the INotifyPropertyChanged interface without code bloat by calling
    [mono] FeatherSharp.exe -npc MyAssembly.dll

## Logger TypeName and MethodName Injection
#### Feather# turns byte code doing this...
```c#
class Class1 //...
{
    public void LogMethod()
    {
        Log.Debug("This seems to work");
    }
    // ...
}
```
   
#### ...into byte code doing this
```c#
class Class1 // ...
{
    public void LogMethod()
    {
        Log.Debug("This seems to work", "Test.FeatherSharp.LogInjection.Class1", "LogMethod");
    }
    // ...
}
```

Easily consume the log messages by subscribing to the Log.MessageRaised event, then pass it on to the logging backend of your choice.

# Feather# is on NuGet

Visit the Feather# NuGet site:

https://www.nuget.org/packages/FeatherSharp/

Add the Feather# package to your project using the Package Manager Console:

    Install-Package FeatherSharp

To make it work smoothly, create a batch file named `FeatherSharp.cmd` in your solution folder with the following contents:

    @%1\packages\FeatherSharp.0.3.0.0\tools\FeatherSharp.exe %2 %3 %4 %5 %6 %7 %8 %9

or

    @%1\packages\FeatherSharp.0.3.0.0\tools\FeatherSharp.exe --sn=<YourStrongNameFile> %2 %3 %4 %5 %6 %7 %8 %9

if your assembly is signed with a strong name.

Then create the following post build event for every project that uses Feather# (using the -log feather in this example):

    $(SolutionDir)FeatherSharp.cmd $(SolutionDir) -log $(TargetPath)

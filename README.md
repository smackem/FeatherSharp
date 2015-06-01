# FeatherSharp
Feather# - An AOP utility for .NET, based on Mono.Cecil.

### Features:
* NotifyPropertyChanged Injection w/ Property Dependencies
* Log TypeName and MethodName Injection
* Merge Assemblies
* Compatible with .NET 4.5 and Mono 4

### Usage:

    FeatherSharp.exe <Feathers> <FileName>
      Feathers:
      -npc : Inject NotifyPropertyChanged
      -merge : Merge dependencies into <FileName>
      -log : Inject augmented log method calls

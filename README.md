# TextTemplating

**Mono/T4 has released a new dotnet core version, check it out! https://github.com/mono/t4**

[中文说明](README.CHS.md)

## Goal
This project's goal is to bring the old T4 text templating code generating approach to the new ASP.NET Core 2.0 projects.

### Update 
The Visual Studio 2017 and Xamarin Studio now supports to process *.tt files in desing time, but this repo is maybe still useful who wants to process T4 templates in a dotnet core(netstandard2.0) project outside IDE (eg. in Linux or macOS with Visual Studio Code)

## How to use

### Add myget source

Add Nuget.Config File to your solution root with content below:
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="NuGet.org" value="https://nuget.org/api/v3/" />
    <add key="ZeekoGet" value="https://www.myget.org/F/zeekoget/api/v3/index.json" />
  </packageSources>
</configuration>
```

### As a command line tool
Add the following to  `YourProject.csproj`.

```xml
<ItemGroup>
    <PackageReference Include="TextTemplating" Version="2.1.0-alpha1" />
</ItemGroup>
<ItemGroup>
    <DotNetCliToolReference Include="TextTemplating.Tools" Version="2.1.0-alpha1" />
</ItemGroup>
```

Now you can use the `dotnet t4` command as a command line tool to transform templates at design-time, with the specified command line arguments.

Run `dotnet t4 -h` to see the usage.

Example:
```Batchfile
dotnet t4 proc -f DbBase.tt
```

### As a design time tool
Add the same packages mentioned above, then you can run `dotnet t4 trans -f Person.tt` to transform a text template.

Note: You can use all the packages that you have installed into your project when writing T4 template, so there may be no necessary to use "assembly" directive to reference assembly via assembly name(so I skipped these feature, you can also reference assembly via path).


### As a library
To transform templates at runtime, you can also use the `Engine` class.

*Sample is work in progres*

### As a service (Not Implemented)
*Work in progres*

# License
MIT

# Calculator
A fork of the now open-source Windows Calculator.

## Features
- Standard Calculator functionality which offers basic operations and evaluates commands immediately as they are entered.
- Scientific Calculator functionality which offers expanded operations and evaluates commands using order of operations.
- Programmer Calculator functionality which offers common mathematical operations for developers including conversion between common bases.
- Date Calculation functionality which offers the difference between two dates, as well as the ability to add/subtract years, months and/or days to/from a given input date.
- Calculation history and memory capabilities.
- Conversion between many units of measurement.
- Currency conversion based on data retrieved from [Bing](https://www.bing.com).
- [Infinite precision](https://en.wikipedia.org/wiki/Arbitrary-precision_arithmetic) for basic
  arithmetic operations (addition, subtraction, multiplication, division) so that calculations
  never lose precision.
- Internet connection capabilities which allows you to connect the calculator to the network.
- Ability to shutdown windows from calculator! (This one was added cause it's cool and idk. I was bored.)
- Update tool to be able to update the calculator.
## Getting started

(NOTE: Basin Calculator can be downloaded from releases where a precompiled EXE of the calculator will be.)

Prerequisites:
- Your computer must be running Windows 11, build 22000 or newer.
- Install the latest version of [Visual Studio](https://developer.microsoft.com/en-us/windows/downloads) (the free community edition is sufficient).
  - Install the "Universal Windows Platform Development" workload.
  - Install the optional "C++ Universal Windows Platform tools" component.
  - Install the latest Windows 11 SDK.

  ![Visual Studio Installation Screenshot](docs/Images/VSInstallationScreenshot.png)
- Install the [XAML Styler](https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler) Visual Studio extension.

- Get the code:
    ```
    git clone https://github.com/Microsoft/calculator.git
    ```

- Open [src\Calculator.sln](/src/Calculator.sln) in Visual Studio to build and run the Calculator app.
- For a general description of the Calculator project architecture see [ApplicationArchitecture.md](docs/ApplicationArchitecture.md).
- To run the UI Tests, you need to make sure that
  [Windows Application Driver (WinAppDriver)](https://github.com/microsoft/WinAppDriver/releases/latest)
  is installed.

NOTE:Basin Calculator only builds on X86. It does not build on ARM or X64. There are still options to build BasinCalculator on X64 and ARM but the precompiled builds in releases by default target X86. X86 is smaller and simpler to build releases for. Plus, 64 bit windows is compatible with X86 programs! If you want to build from a different architecture, follow instructions above.

Also NOTE: Basin Calculator will have a linux port. These are essentially linux ports of the BasinCalculator EXE and not  compiled directly from the source code. They are essentially reverse engineered versions of the compiled Windows version.

Fun fact: Basin Calculator is lightweight! 


## License
original copyright:
Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT License](./LICENSE).

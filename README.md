# Prerequisite
1. Build  
   Ensure that you have .NET Core 2.2 SDK installed. 

   > For Visual Studio 2017 (which I am actually using) compatibility reason, please use https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.107-windows-x64-installer 


# Architecture

1. Should be 3 processes in production, but as ...

1. Command handling sync or async

## Design

1. Never put Http Dto into domain and Never expose domain type to Http

1. Flat event structure (no embedded type, for easing event versioning), except for entity never changes

1. Simulate I/O, avoid blocking thread pool thread waiting for I/O
# Prerequisite
1. Build  
   Ensure that you have .NET Core 2.2 SDK installed. 

   > For Visual Studio 2017 (which I am actually using) compatibility reason, please use https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.107-windows-x64-installer 


# Assumptions
1. Acquiring bank

   Hereunder the statement of the test:
    > It also performs some validation of the card information and then **sends the payment details to the appropriate 3rd party organization for processing**.

    1. I suppose the `Acquiring bank` is a component of the Information System, not the real bank. 
    1. I suppose the `Third party` is a legal 

# Architecture

1. Should be 3 processes in production, but as ...

1. Command handling sync or async  
To manage burst/back pressure

# Design

1. Never put Http Dto into domain and Never expose domain type to Http

1. Flat event structure (no embedded type, for easing event versioning), except for entity never changes

1. Simulate I/O, avoid blocking thread pool thread waiting for I/O

1. Anti Corruption  
Never leak external libraries (acquiring bank ones) to Domain Entity / Aggregate, do mapping instead

# API
1. Ids  

# SLA

1. A `PaymentRequestId` will be handled once and only once.

# Tradeoff
1. Identical `PaymentRequest` submitted more than once. We have two options:  
   1. Idempotency: remind client of API that payment has already been created, and it is available at this location.

   1. Reject duplicated  `PaymentRequest`.

# TODO

1. Check `BankPaymentId` in tests when bank replies
1. Alls async, I/O should add timeout cancellation


## Publish
1. goto API csproj folder  
1. dotnet publish -c Release -r win10-x64
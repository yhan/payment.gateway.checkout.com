# Prerequisite
1. Build  
   Ensure that you have .NET Core 2.2 SDK installed. 

   > For Visual Studio 2017 (which I am actually using) compatibility reason, please use https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.107-windows-x64-installer 


# Assumptions
1. **Acquiring bank**

   The statement of the exercise text:
    > It also performs some validation of the card information and then **sends the payment details to the appropriate 3rd party organization for processing**.

    1. I suppose the `Acquiring bank` is a component of the Information System, not the real bank. 
    1. I suppose the `Third party` is a legal entity who liaise the **Acquiring bank** and **Issuing bank** and potentially a **Custodian** to complete the payment if eligible.

1. Banks
   **In the payment workflow, we should determine the `Issuing bank` and the `Acquiring bank`**.  

   1. **Issuing bank**  
   After have done some research, now I am aware of the fact that we can determine the `Issuing bank` via `card number`.

   1. **Acquiring bank**  
   But when it comes to Acquiring bank, I find no way in the beginning to determine it. The way that can determine the acquiring bank, could be: We associate the `Acquiring bank` to the `Merchant`.  
   Things can be done when we **onboard** a `Merchant`

   The statement of the exercise text, in the payment request does not mention an identifier of the `Merchant`. 
   
   > A payment request should include appropriate fields such as the card number, expiry month/date, amount, currency, and cvv.
   
   The question is how the system can forward a `Payment Request` to the `Merchant`'s Acquiring bank? 

   I added `Merchant`'s id to the `Payment Request`.   

   

# Architecture

1. Should be 3 processes in production, but as ...

1. Command handling sync or async  
To manage burst/back pressure

# Design
1. Entity:  
   **Payment**

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
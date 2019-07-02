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

1. **Determination of Banks**  
   In the payment workflow, we should determine the `Issuing bank` and the `Acquiring bank`.  

   1. **Issuing bank**  
   After have done some research, now I am aware of the fact that we can determine the `Issuing bank` via `card number`.

   1. **Acquiring bank**  
   But when it comes to Acquiring bank, I find no way in the beginning to determine it. The way that can determine the acquiring bank, could be: We associate the `Acquiring bank` to the `Merchant`.  
   Things can be done when we **onboard** a `Merchant`

   The statement of the exercise text, in the payment request does not mention an identifier of the `Merchant`. 
   
   > A payment request should include appropriate fields such as the card number, expiry month/date, amount, currency, and cvv.
   
   The question is how the system can forward a `Payment Request` to the `Merchant`'s Acquiring bank? 

   I added `Merchant`'s id to the `Payment Request`.    

   > **! Disclaimer:**: In real world, it will be not safe to let 
     


# Architecture

## **CQRS**
CQRS is chosen for these *reasons*:  
I am asked to develop two features: Payment request and Payment details retrieval. 
1. They should be scaled differently.
1. Do payment is the core function of Gateway, the benefit of the company may main come from the amount/number of transactions achieved by the platform. We should not disturb the payment request handling by a flood of payment details queries for reporting purpose.

### Implementation
Three components:
- **Write API**: Handle payment requests, saving to `write model`: events. (events: will be explained in [Event sourcing](##EventSourcing) section)

- **Read Projector**: Project `write model` to `read model` which fits read payment details requirement.

- **Read API**: Feed payment retrieval queries.   
  > Here we have only one read model which is asked for. But in real world, we probably have many of them, for performance enhancement. We can imagine that company's revenue comes from **transaction volume**, thus we can imagine a read model that give us live PnL vision as transactions go on.

> **! Disclaimer:** **In real world, above three components should be hosted to 3 separate processes, for scaling easily**. Here for the sake of simplicities of the exercise, I have not implemented neither external storage (events and read model) nor external message bus. It will be hence difficult to separate them to different processes.

Still you can see the embryonic form of the 3 processes.
- [Write API](https://github.com/yhan/payment.gateway.checkout.com/tree/master/src/PaymentGateway/Apps/PaymentGateway.API/Controllers/WriteAPI)

- [Read API](https://github.com/yhan/payment.gateway.checkout.com/tree/master/src/PaymentGateway/Apps/PaymentGateway.API/Controllers/ReadAPI)

- [Read projector](https://github.com/yhan/payment.gateway.checkout.com/tree/master/src/PaymentGateway/Apps/PaymentGateway.API/ReadProjector)


## **EventSourcing**
For a Gateway which handles sensitive financial transactions second to to second, it is critical that we have a full audit trail of what has happened.

Event sourcing also helps constructing CQRS. i.e. we have **always** capabilities to construct diverse and varied read models, as events recorded all information chronologically.

## **Hexagonal**
The motivation of Hexagonal is very general, can be found for example [here](https://apiumhub.com/tech-blog-barcelona/hexagonal-architecture/)


1. Command handling sync or async  
To manage burst/back pressure

# Design
1. Entity:  
**Payment** represent a financial transaction achieved with the help of a bank payment card. A `Payment` can fail or succeed.

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


# Improvements

Hereunder some improvements should be definitely done:  

1. I use `Merchant` id to determine its `Acquiring bank` (cf. [Assumptions](#Assumptions)), it is part of `Payment Request` payload. This is not safe. And in a very general way, the exchanges of messages between Gateway and Merchant is not protected by authentication.   

   In real world we should do authentication negotiation to let Gateway to know which `Merchant` I am dialoguing. This can be achieved as follows:  

    1. When we onboard a `Merchant`, we distribute a `secret` in a very safe manner to `Merchant`. 
    1. In all exchanges between `Merchant` to `Gateway`, the secret should be included in HTTP header '' 

# Go further
1. Retrieving a payment’s details API
   The exercise text states a basic requirement:  
   > The second requirement for the payment gateway is to allow a merchant to retrieve details of a previously made payment using its identifier.  

   In real world, we may consider adding:
   1. Query for a time window
   1. Query pagination
   1. Other filters

   For achieving query for a time window, I should add payment timestamp to both my `Events` and `Read models`.

   

## Publish
1. goto API csproj folder  
1. dotnet publish -c Release -r win10-x64
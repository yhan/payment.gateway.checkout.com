
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
  > Here we have only one read model which is asked for. But in real world, we probably have many of them, for performance enhancement. We can imagine that company's revenue comes from **transaction volume**, thus we can imagine a read model that give us live PnL vision (all Merchants consolidated or segregated) as transactions go on.

   > Write and Read API can scale to multiple instance. If we want to scale Read Projector, we should ensure that message consumption is competing and that the processing of messages should respect order of event sequence number.

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



# Design
1. Entity:  
**Payment** represent a financial transaction achieved with the help of a bank payment card. A `Payment` can fail or succeed.

1. **Command handling asynchrony**  
For managing: 
   - unreliable network, unknown bank API availability and latency
   - burst/back pressure: i.e. if we handle `PaymentRequest` synchronously, because of network and long latency, our Gateway may suffer from high I/O waiting, the system will congested. 

   I decided to handle `PaymentRequest` asynchronously. i.e. When `PaymentRequest` arrives, Gateway create immediately a `Payment` resource. The request forwarding and bank response handling are done asynchronously. HTTP status 202 Accepted along with a resource identifier in location header will be returned. Merchant can follow up (polling) the payment with the given address.

   > In real world, we can consider long polling, Server Sent Event or Webhooks.

   In real world, for the sake pragmatism, we can do more *smart* handling. i.e. We can say: if the Gateway get a response from the bank within 50 ms, it returns 201 Created with the `Payment` final status: Accepted or Rejected (by the bank); otherwise returns 202 Accepted.

1. **Anti corruption**:

   - Never put HTTP dto & external library into Domain and never expose domain type to HTTP.
   - Always do adaptation from one world to another.

1. **Event structure: flat**   
no embedded type, for easing event versioning.

1. Simulate I/O, avoid blocking thread pool thread waiting for I/O

1. Anti Corruption  
Never leak external libraries (acquiring bank ones) to Domain Entity / Aggregate, do mapping instead

1. **Storage**
For sake of simplicity of the exercise, I used InMemory for:
   - Write models storage: Event Store
   - Message bus
   - Read models storage

   > In real world, we should for sure using external storage and message bus, for cluster configuration. 
   
   > For storing events we may use [EventStore](https://eventstore.org/) (native events) or Azure blob storage  (should code something for serving it as event store), or other things

   > For message bus: RabbitMQ/Azure service bus/...

   > For read models: choose suitable SQL or NoSql storage.


# API
> *If you use [Restlet Client](https://chrome.google.com/webstore/detail/restlet-client-rest-api-t/aejoelaoggembcahagimdiliamlcdmfm?hl=en), you can import payment-gateway-apis.json (in the root folder), to view all APIs with examples. Otherwise, please use provided swagger.*

## Public API

1. **Request a payment**:  
  - **POST api/Payments**
     Endpoint to send payment request.  
     
     Request example:  
     > How to get a onboarded Merchant id? Cf. [Private API](#private-api)

     ```json
     {
        "requestId": "ccd8af8e-5a27-40dc-93c5-f19e78984391",
        "merchantId": "2d0ae468-7ac9-48f4-be3f-73628de3600e",
        "card":{
            "number": "4524 4587 5698 1200",
            "Expiry": "05/19",
            "Cvv": "321"
        },
        "amount": {
            "currency": "EUR",
            "value": 42.66
        }
    }
    ```

    Response example:  
    1. 202 Accepted
    ```json
    {
       "gatewayPaymentId": "41b49021-98a2-41cf-80dc-6f87382322f8",
       "acquiringBankPaymentId": null,
       "status": "Pending",
       "requestId": "ccd8af8e-5a27-40dc-93c5-f19e78984391",
       "approved": null
    }
    ```
       and with the header location.

    2. 404 Bad request with the invalidity details, if the request is invalid
    ```json
    {
       "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
       "title": "Invalid request",
       "status": 400,
       "detail": "Invalid card CVV"
    }
    ```

1. **Get payment and payment details**:  

   - **GET api/Payments/{gateWayPaymentId}**  
     Endpoint to retrieving payment status. Write controller redirect to this controller (why? Cf. [Command handling asynchrony](#Design)). 

     Response example: 
     > `approved` is the boolean indicating if payment is accepted by the bank or not.

     > `status` gives the reason/description of `approved` boolean. It can be _success_, _rejected_ or _timeout_.


     a) Success
     ```json
      {
        "gatewayPaymentId": "f72d3230-d08c-409f-a03b-c2872b7f762f",
        "acquiringBankPaymentId": "b49739f0-c193-49de-967f-fdbb1d8f7218",
        "status": "Success",
        "requestId": "bab81817-8f09-4c32-b1e0-e76b40039ec1",
        "approved": true
      }     
     ```
     > `acquiringBankPaymentId` should be used for further querying payment details. See below.

     b) Rejected:
     ```json
     {
       "gatewayPaymentId": "41b49021-98a2-41cf-80dc-6f87382322f8",
       "acquiringBankPaymentId": "0bfa5d5b-8742-459f-94c9-484d61ad6093",
       "status": "RejectedByBank",
       "requestId": "ccd8af8e-5a27-40dc-93c5-f19e78984391",
       "approved": false
     } 
     ```

     c) Timeout
      ```json
      {
         "gatewayPaymentId": "68e56457-d7b9-4c88-9f42-1075a8d18d13",
         "acquiringBankPaymentId": null,
         "status": "Timeout",
         "requestId": "8e8bcc4a-3fe7-4834-8257-eb8aaa948af3",
         "approved": false
      }
     ```
     > Production code, use random bank latency from 0 to 5 sec; and timeout is set to 2 sec

   -  **GET api/PaymentsDetails/{acquiringBankPaymentId}**  
      Endpoint to retrieving payment details

      Response example:
      ```json
      {
        "status": "RejectedByBank",
        "acquiringBankPaymentId": "0bfa5d5b-8742-459f-94c9-484d61ad6093",
        "card":{
        "number": "4524 XXXX XXXX XXXX",
        "expiry": "05/19",
        "cvv": "321"
        },
        "approved": false
      }
      ```


1. **Ids**: three types of ids
   - **Payment request id**: Payment unique identifier from merchants. Is part of payment request payload. Cf. C# struct `PaymentRequestId`. In real world, each `Merchant` will send their own format of request unique identifier. We should adapt it to the one of Gateway . For simplicity of exercise, I used `System.Guid`.

   - **Gateway payment id**: Unique identifier of payment in Gateway internal system, Cf. C# struct `Domain.GatewayPaymentId`. 

   - **Acquiring bank payment id**: Unique identifier returned from acquiring banks, Cf C# struct `Domain.AcquiringBankPaymentId`. In real world, each `Acquiring bank` will send their own unique identifer.  We should adapt it to the one of Gateway . For simplicity of exercise, I used `System.Guid`.


1. **Switch out for a real bank**
   Specific `MyBankAdapter` should be implemented implementing domain port `PaymentGateway.Domain.IAdaptToBank`.

## Private API 
For you code reviewer's convenience, some private endpoints are exposed. They are

1. **GET api/Merchants**
   Return all merchants. The **merchant id** will be useful when you construct you `PaymentRequest`.

   Response example:
   ```json
   [
      {
         "id": "2d0ae468-7ac9-48f4-be3f-73628de3600e",
         "name": "Amazon"
      },
      {
         "id": "06c6116f-1d4e-44d3-ae9f-8df90f991a52",
         "name": "Apple"
      }
   ]
   ```
1. **GET api/AcquiringBankPaymentsIds**  
   Returns all Acquiring banks' payment ids
     Response example:
   ```json
   [
     "593b4d51-8e5c-4ecc-a2b8-1946c9048275",
     "027f704c-531d-4bd7-bfda-09817926db49"
   ]
   ```
1. **GET api/GatewayPaymentsIds**

   Returns all Acquiring banks' payment ids
   Response example:
   ```json
   [
     "b541bda3-a0da-46f4-b51a-5d3673c0fd93",
     "63bcff37-364a-4f83-904a-2b9a339d2e4f"
   ]
   ```

## Configuration

1. Retries and timeouts
   When we can Bank API, `timeout` can happen. The system will try three times (with sparse incremented wait time before retrying). If still fail in the end, we consider the payment timeouts definitely.
   For better demo effect, I configured:

   ```json
   "AppSettings": {
        "TimeoutInMilliseconds": 2000,
        "MaxBankLatencyInMilliseconds": 4000  
    }
   ```
   The very specific behavior "during retries, if timeouts once, timeouts always" is purely for better demo feeling, i.e. to see `timeout` without submitting a lot of payment requests. Cf. `RandomDelayProvider.cs`

1. Call bank API synchronously or asynchronously
   For production code, use `API`; for testing use `Tests`
   ```json
   "AppSettings": {
        "Executor": "API"
    }
   ```


# SLA
1. A `PaymentRequestId` will be handled once and only once.

# Tradeoff
1. Identical `PaymentRequest` submitted more than once. We have two options:  
   1. Idempotency: remind client of API that payment has already been created, and it is available at this location.

   1. Reject duplicated  `PaymentRequest`.

   I chose the 2nd.

# Performance
For a Payment Gateway, what is important is:
- High availability
- Throughput
- Low latency
- Scalability

I have done in solution:
- throughput tests 
- latency tests
- how the application cope under burst condition
  1. large number clients launched in parallel requesting payments
  1. large number of clients, plus large number of payment details, do parallel query on combination of the two.

> In real world, above testing need fit realistic production scenario.

when IGenerateBankPaymentId is configured as `NoDelay`, performances in Performance.xlsx.  

For read payments, 93100 parallel requests seem to be the limit of the system. We can configure proper max limit parallel calls to kestrel. 

Nevertheless, under burst situation
- API does not crash
- When clients disconnected by rejection of connection, other read/write operation continue to work well

```csharp
 .ConfigureKestrel((context, options) =>
   {
      options.Limits.MaxConcurrentConnections = 10_000;
      options.Limits.MaxConcurrentUpgradedConnections = 1000;
   })
```
For performance consideration, all coming requests thread is offloaded to thread pool threads.  
To resist burst, we can add `requestTimeout` to kestrel configuration. We can also scale the server instances using Kubernetes cluster or Swarm cluster. This can help for achieving high availability. 

> Memory consumption is due to in memory cache in my system. In real world, specific caching might be considered, when unacceptable latency is caused by no-caching. Consider caching only when necessary. Caching introduces two complexities / problem: 1) synchronization. 2) large memory footprint triggers GC, causing latency overhead.

To run performance tests:
   1. Goto API csproj folder  
   1. Run: 
      ```
      Dotnet publish -c Release -r win10-x64
      ```
   1. Run the tests in `PaymentGateway.Write.PerformanceTests` and `PaymentGateway.Read.PerformanceTests`

Further: If I have more time, I will also test:
- Endurance / Soak testing
- Test individual components: currently my Read Projector is not performance tested
- Test components hosted in cluster
- Integrate performance testing to CI
- Monitor production systems: metrics and perf indicators should be monitored


# Unit and Acceptance tests
The coding is entirely test driven.  

## Coverage
Excluding performance tests assembly, Code coverage: 83.05%. (report on PaymentGateway.coveragexml in the root folder)

Non covered codes are:  
 - API bootstrap
 - Some infrastructure code borrowed from [Greg Young's git repository](https://github.com/gregoryyoung/m-r)
 - Some randomness generation only for production. (Acceptance tests use output deterministic behavior)
 - Properties in acquiring bank stubs, they are there just to show the design.
 - Guid ids generator

## Edge cases
1. Bank sends payment id which conflicts with a previously received one.
   Not asked to do as per: 
   > We should assume that a bank response returns a unique identifier
   
   But I still implemented and tested. In this situation, we should consider that the payment is on a unknown state. Two possibilities at least:
   - Bank accepted the second conflicting one, but sent a `PaymentId` already used.
   - Bank never proceeded the second payment request, it instead just resent a payment status for the very first one.

   This will be a production incident, hence should be investigated.

# Prerequisite for building the solution in Visual Studio
   Ensure that you have .NET Core 2.2 SDK installed. 

   > For Visual Studio 2017 (which I am actually using) compatibility reason, please use https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.107-windows-x64-installer 

# Improvements

Hereunder some improvements should be definitely done:  

1. I use `Merchant` id to determine its `Acquiring bank` (cf. [Assumptions](#Assumptions)), it is part of `Payment Request` payload. This is not safe. And in a very general way, the exchanges of messages between Gateway and Merchant is not protected by authentication.   

   In real world we should do authentication negotiation to let Gateway to know which `Merchant` I am dialoguing. This can be achieved as follows:  

    1. When we onboard a `Merchant`, we distribute a `secret` in a very safe manner to `Merchant`. 
    1. In all exchanges between `Merchant` to `Gateway`, the secret key should be included in HTTP header 'Authorization' 

1. Alls simulated async, I/O should add timeout cancellation

# Go further
1. Retrieving a payment’s details API
   The exercise text states a basic requirement:  
   > The second requirement for the payment gateway is to allow a merchant to retrieve details of a previously made payment using its identifier.  

   In real world, we may consider adding:
   1. Query for a time window
   1. Query pagination (consider if unbounded queries are allowed, deal with manageable chunks)
   1. Other filters

   For achieving query for a time window, I should add payment timestamp to both my `Events` and `Read models`.

1. Require `PaymentRequest` Smart Batching [here](https://blog.scooletz.com/2018/01/22/the-batch-is-dead-long-live-the-smart-batch/))

   The motivations are:
   - Maybe for a merchant, say Amazon, the receives 50,000 payment requests per second from shopper. Batching 5000 requests is an option, because shopper doesn't care about 1s of delay.
   - For our Gateway, we will have less resources to consume, thus improve the performance. 

   > A combination of time window and number of requests can be used to size the Smart Batching.
   
# Open source used:
The event sourcing infrastructure is borrowed from [Greg Young's git repository](https://github.com/gregoryyoung/m-r)

# BigBookStore

<br>

## Purpose

BigBookStore solution is a sandbox that I am leveraging to showcase my capabilities with C# and test automation.

The BigBooks.API project implements a simple API for a hypothetical on-line book store.

I will continue to evolve this project as my bandwidth allows. I will keep the main branch in an operational state. (I make no promises about the dev branch.)

<br>

## Repo Elements

1. src/BigBooks.API

1. test/PostmanCollection

1. test/BigBooks.UnitTest

1. test/BigBooks.IntegrationTest


<br>

### BigBooks.API

- The project leverages a typical Web API pattern.
    - Controller layer is intentionally light-weight.
    - Provider layer resolves data transformation and db interactions.
    - Dto objects exchanged with controllers
    - Simmplified token authorization

- Intentional complexities
    - PurchaseController, PurchaseBooks() indentifies user from token claim
    - Admin user required for:
        - BookController, AddBook()
        - BookController, UpdateBook()
        - UserController, GetUserInfo()
        - UserController, GetUsers()
    - Validation of modified object in BookProvider.cs UpdateBook()    
    - BookAddUpdateDto.Isbn must be valid Guid
    - UserAddUpdateDto.UserEmail must be valid email

- SQLite database, BigBooks.db

<br>

### PostmanCollection

- This Postman collection holds pre-defined API calls to demonstrate operation of BigBooks.API
- Since the BigBooks.API leverages authentication tokens, the Postman AuthenticationRequest must precede other API requests.
    - The AuthenticationRequest will save the response token to a Postman variable which is then used in the header of subsequent requests.
    - Example user with Customer Role: Bella.Barnes@demo.com
    - Example user with Admin Role: Clark.Kent@demo.com

<br>

### BigBooks.UnitTest

- xunit tests of project elements exercised in isolation.
    - AuthenticationServiceTest.cs
    - BookProviderTest.cs
    - PurchaseProviderTest.cs
- InMemory DbContext created for each test case.
    - Refer to BigBookTest.cs constructor.

<br>

### BigBooks.IntegrationTest

- xunit project demonstrating end-to-end message scenarios leveraging an in-memory database and a WebApplicationFactory design pattern.
- This example is significant not for the coverage it provides (which is small), but rather for the manner of test execution.
    - BigBookWebAppFactory.cs, ConfigureWebHost() adjusts the service and configuration of the source WebApplication to spawn an internal, temporary TestServer for testing.
    - The HttpRequests of MessageTest.cs target the TestServer to exercise system-level behaviors of the source project.

<br>

### Feature Roadmap

Things yet to do ...

1. Lots more unit tests
1. Convert Controller methods to async.
1. UserController, Modify User
1. BookReviewController, Add Review
1. BookReviewController, Delete Review

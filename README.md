# BigBookStore

<br>

## Purpose

BigBookStore solution is a sandbox that I am leveraging to showcase my capabilities with C# and test automation.

The BigBooks.API project implements a simple API for a hypothetical on-line book store.

I will continue to evolve this project as my bandwidth allows. I will keep the master branch in an operational state. (I make no promises about the dev branch.)

<br>

## Postman Collection

- In the SupportFiles subdirectory a Postman collection can be found. This can be used to directly exercise the BigBooks.API
- Since the BigBooks.API leverages authentication tokens, the Postman AuthenticationRequest must precede other API requests.
    - The AuthenticationRequest will save the response token to a Postman variable which is then used in the header of subsequent requests.
    - Example user with Customer Role: Bella.Barnes@demo.com
    - Example user with Admin Role: Clark.Kent@demo.com






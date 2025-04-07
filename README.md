# Smart Parking Lot System

This project provides a backend API for managing parking spots in a smart parking lot, simulating interactions with IoT devices.

## Features

* Track parking spot status (Free/Occupied).
* CRUD operations for managing parking spots.
* Simulated IoT device interaction via API calls to occupy/free spots.
* Validation: Ensures spots aren't occupied/freed incorrectly, checks for registered devices.
* Dynamic tracking of available spots.
* RESTful API endpoints.
* (Bonus) Rate Limiting per device for status change actions.
* (Bonus) Pagination for retrieving parking spots.

## Architecture

The solution follows a layered architecture inspired by Clean Architecture:

* **Domain:** Core business entities, enums, and domain logic (e.g., `ParkingSpot`, `Device`, `ParkingSpotStatus`).
* **Application:** Contains application logic, use cases, service interfaces (`IParkingService`), repository interfaces (`IParkingSpotRepository`), DTOs, and custom exceptions.
* **Infrastructure:** Implements interfaces defined in the Application layer. Includes data persistence (In-Memory repositories for this simulation) and implementations for external concerns (like the Rate Limiter).
* **API (Presentation):** Exposes the RESTful API using ASP.NET Core controllers. Handles HTTP requests/responses, interacts with the Application layer.

## Technology Stack

* .NET 8
* ASP.NET Core 8 Web API
* xUnit (for Unit Testing)
* Moq (for Mocking Dependencies in Tests)
* In-Memory Data Storage (using Concurrent Collections)

## API Endpoints

* `GET /api/parking-spots?pageNumber=1&pageSize=10`: Get a paginated list of all parking spots.
* `POST /api/parking-spots`: Add a new parking spot.
  * **Body:** `{ "name": "string" }`
* `DELETE /api/parking-spots/{id}`: Remove a parking spot.
* `POST /api/parking-spots/{id}/occupy`: Mark a parking spot as occupied.
  * **Requires Header:** `X-Device-ID: <registered-device-guid>`
* `POST /api/parking-spots/{id}/free`: Mark a parking spot as free.
  * **Requires Header:** `X-Device-ID: <registered-device-guid>`

*(Note: See Swagger UI for detailed request/response schemas)*

## Setup and Running

1. **Prerequisites:**
   * .NET 8 SDK installed.
   * Git (optional, for cloning).
2. **Clone the repository (optional):**
   
    ```bash
    git clone https://github.com/Neydor/SmartParkingLot.git
    cd SmartParkingLot
    ```
3.  **Build the solution:**
    ```bash
    dotnet build SmartParkingLot.sln
    ```
4.  **Run the API project:**
    ```bash
    dotnet run --project SmartParkingLotManagement.Api/SmartParkingLot.Api.csproj
    ```
	Or execute with VS2022 and docker or IIS Express
	
	Exist a dockerfile in SmartParkingLotManagement.Api folder, execute it to make the image in a docker container.
	
5.  **Access the API:**
    *   The API will typically be available at `https://localhost:7XXX` or `http://localhost:5XXX` (check the console output).
    *   Access the Swagger UI documentation by navigating to the base URL in your browser (e.g., `https://localhost:7123`).

## Running Tests

1.  Navigate to the solution directory.
2.  Run the following command:
    ```bash
    dotnet test SmartParkingLotManagement.sln
    ```

## Registered Devices (Simulation)

For testing the `occupy` and `free` endpoints, use one of the pre-registered device IDs found in `Infrastructure/Persistence/DataStore.cs`. By default, these are:
Devices
*   `a1a1a1a1-b2b2-c3c3-d4d4-e5e5e5e5e5e5`
*   `f6f6f6f6-a7a7-b8b8-c9c9-d0d0d0d0d0d0`
Free Parking Spots
*   `10000000-0000-0000-0000-000000000001`
*   `10000000-0000-0000-0000-000000000002`
*   `10000000-0000-0000-0000-000000000003`

Pass the chosen ID in the `X-Device-ID` header when calling the relevant endpoints.

## Key Design Choices & Principles

*   **SOLID Principles:** Applied throughout the design (e.g., SRP in services/repositories, OCP via interfaces, DIP via dependency injection).
*   **Dependency Injection:** Used extensively via the built-in ASP.NET Core container.
*   **Layered Architecture:** Clear separation of concerns between presentation, application logic, domain, and infrastructure.
*   **Repository Pattern:** Abstracted data access logic.
*   **DTOs:** Used for clean data transfer between layers and for API contracts.
*   **Clean Code:** Followed practices like meaningful naming, small methods, and clear structure.
*   **Error Handling:** Centralized exception handling middleware providing consistent `ProblemDetails` responses. Custom exceptions for specific error scenarios (NotFound, Validation, Conflict).
*   **Async/Await:** Used throughout for I/O operations to maintain responsiveness.
*   **Thread Safety:** Used `ConcurrentDictionary` for the in-memory store for basic thread safety.

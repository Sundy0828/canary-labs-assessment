# Canary Labs In-Memory Sensor API Challenge

[![codecov](https://codecov.io/gh/Sundy0828/canary-labs-assessment/graph/badge.svg?token=PLRUT7SSXZ)](https://codecov.io/gh/Sundy0828/canary-labs-assessment)

This project implements a high-throughput, in-memory sensor data API. It provides:

-   A **write endpoint** for storing sensor readings
-   A **read endpoint** for querying sensor readings within a time range
-   Thread-safe, in-memory storage optimized for fast retrieval
-   Console clients for testing write and read throughput
-   xUnit integration tests for verification

---

## Table of Contents

-   [Setup](#setup)
-   [Run the API](#run-the-api)
-   [Run Clients](#run-clients)
-   [Run Tests](#run-tests)
-   [Design Choices](#design-choices)

---

## Setup

Make sure you have [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.

Clone the repo and navigate to the solution:

```bash
git clone https://github.com/Sundy0828/canary-labs-assessment.git
cd canary-labs-assessment
```

Restore packages:

```bash
dotnet restore
```

---

## Run the API

Navigate to the API project:

```bash
cd SensorApi
dotnet run
```

By default, the API will run at:

-   `http://localhost:5043`

Swagger UI is available at `http://localhost:5043/swagger` for exploring the endpoints.

---

## Run Clients

The console clients can act as **writers** or **readers**.

### Writer Client

Simulates high-throughput sensor data ingestion.

```bash
cd SensorClients
dotnet run -- writer <apiKey> <apiBase> <numSensors> <pointsPerSecond> <durationSeconds>
```

Example:

```bash
dotnet run -- writer key-client-a-12345 http://localhost:5247 1000 10 60
```

-   Writes to 1000 sensors
-   10 data points per second per sensor
-   Runs for 60 seconds

### Reader Client

Retrieves sensor data on demand.

```bash
dotnet run -- reader <apiKey> <apiBase> <sensorNames> <start> <end>
```

Example:

```bash
dotnet run -- reader key-client-a-12345 http://localhost:5247 Sensor-0001,Sensor-0002 2025-01-01T00:00:00Z 2030-01-01T00:00:00Z
```

-   Requests data for one or more sensors
-   Start and end times in ISO 8601 format
-   Prints partial JSON response

---

## Run Tests

From the solution root:

```bash
dotnet test
```

Tests are organized by directory, with:

-   A base class per file to handle DI setup and helper functions
-   Separate test files per function (e.g., `PostTests.cs` for `/read` POST)

---

## Design Choices

### Architecture

-   Controllers focus solely on input validation
-   Domain classes (`ReadDomain`, `WriteDomain`) contain the core logic
-   If using a database instead of in-memory storage, data access logic would reside in a DAO (Data Access Object) layer
-   Rationale:
    -   Separation of concerns
    -   Controllers remain thin and focused on HTTP concerns
    -   Business logic is testable independently of the web layer
    -   Clear path to introduce persistence without modifying business logic

### API Key Authentication

-   **Custom ApiKeyAuthorizationAttribute** validates API keys and enforces tenant isolation
    -   Each request must include a valid API key
    -   Sensor names are prefixed with the client ID derived from the API key
    -   Ensures clients with identical sensor names only retrieve their own data
    -   Rationale:
        -   Multi-tenant support without complex infrastructure
        -   Data isolation at the application level
        -   Simple but effective security model for the in-memory store

### Storage

-   **ConcurrentDictionary<string, List<DataPoint>>** per sensor with **per-sensor locks**
-   Rationale:
    -   Simple, predictable behavior
    -   Optimized for append heavy workloads
    -   No global locks, so sensors do not block each other
    -   Filtering is done on a snapshot outside the lock to reduce contention

#### Why Not Other Approaches For InMemorySensorStore

##### ConcurrentBag`<DataPoint>`

-   `ConcurrentBag` is unordered, so timestamps would be jumbled and require sorting every time you read
-   Removing or updating individual items is difficult or impossible
-   Performance degrades with large collections, because bags spread entries across internal structures
-   No ability to maintain a consistent snapshot without copying everything

##### ImmutableList`<DataPoint>`

-   Great for read heavy systems, but each append creates a new list
-   Significant allocation and copying for write heavy workloads

##### Using List without locks

-   Not thread safe
-   Writes and enumerations can corrupt the structure

##### ReaderWriterLockSlim

-   Adds complexity
-   Can incur overhead or cause write starvation
-   Per sensor locking with simple lock objects is faster and easier

##### Global lock around the whole store

-   All sensors block each other
-   Poor scalability under load

##### Database

-   Adds external dependencies
-   Slower than in memory
-   Overkill for the assignment

### Endpoints

-   **Write endpoint**

    -   Accepts batches of sensor readings
    -   Appends to in-memory store in a thread-safe manner
    -   Returns total points stored for monitoring

-   **Read endpoint**
    -   Accepts sensor names and time range
    -   Returns a snapshot of matching points
    -   Filtering done outside the lock to minimize contention

### DTOs and Parameters

-   DTOs (Data Transfer Objects) separate from request parameters
-   Folders: `Dto/` and `Parameters/`
-   Rationale:
    -   Keeps API contracts clear
    -   Makes testing and extension easier
    -   Separates transport layer from domain objects (`DataPoint`)

### Clients

-   Single console program handles both write and read modes
-   Can simulate multiple clients by running multiple instances
-   Rationale:
    -   Simple setup for testing
    -   Flexible for scaling write/read load
    -   Easy to run locally without complex infrastructure

### Tests

-   Organized by directory
-   Base classes handle setup and common operations
-   Separate files for each function (e.g., `PostTests.cs`)
-   Rationale:
    -   Keeps tests clean and maintainable
    -   Makes it easy to add more endpoint tests
    -   Ensures thread safety and correctness under realistic load


# SalesFlow — Multi-Tenant SaaS CRM

A full-stack Customer Relationship Management (CRM) project built with ASP.NET Core, React, and PostgreSQL.

SalesFlow explores modern SaaS architecture, multi-tenancy, clean code principles, and scalable business application development.

> **Project Status:** Under active development.
> This is a personal portfolio project. Features and setup instructions are evolving.

## Tech Stack

### Backend
- C# / .NET
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT Authentication
- RESTful APIs

### Frontend
- React
- JavaScript
- Modern component-based UI

### Database
- PostgreSQL

### Architecture & Tools
- Onion Architecture
- Dependency Injection
- Git & GitHub
- Swagger / OpenAPI
- Unit Testing

## Architecture

The backend follows Onion Architecture, separating business rules, application logic, infrastructure, and API concerns.

```text
SalesFlow
|
|-- src/
|   |-- Domain
|   |-- Application
|   |-- Infrastructure
|   |-- API
|
|-- tests/
|   |-- SalesFlow.UnitTests
|
|-- SalesFlow.slnx
```

### Architecture Layers

**Domain**
- Core business entities
- Domain rules and validation
- Business-related enumerations

**Application**
- Application services
- Data Transfer Objects (DTOs)
- Business use cases

**Infrastructure**
- Database persistence
- Entity Framework Core
- Identity and authentication integration

**API**
- REST API endpoints
- Request and response handling
- Swagger documentation

## Project Goals

SalesFlow is designed to explore the development of a modern multi-tenant SaaS CRM, including:

- Tenant-based data isolation
- User authentication and authorization
- Customer and lead management
- Sales pipeline and deal tracking
- Task and activity management
- Role-based access control
- REST API integrations
- Responsive CRM interface

These represent the project's intended capabilities and should not be interpreted as a list of completed features.

## Getting Started

### Prerequisites

- .NET SDK compatible with the solution
- PostgreSQL
- Node.js and npm for the frontend
- Git
- Visual Studio or Visual Studio Code

### Local Development

1. Clone the repository:

```bash
git clone https://github.com/Umidshamdin/Multi-Tenant-SaaS-CRM.git
```

2. Open `SalesFlow.slnx` in Visual Studio.

3. Configure the PostgreSQL connection string for your local environment.

4. Configure authentication settings and a development JWT signing key securely.

5. Restore the .NET dependencies:

```bash
dotnet restore SalesFlow.slnx
```

6. Apply the database migrations and start the API using the project's development configuration.

7. Install the frontend dependencies and start the React development server.

Detailed database migration and frontend startup commands will be documented as the project evolves.

## Testing

The repository includes a unit-test project under:

`tests/SalesFlow.UnitTests`

Run the solution tests with:

```bash
dotnet test SalesFlow.slnx
```

## Development Roadmap

- [ ] Complete core CRM modules
- [ ] Expand tenant and role management
- [ ] Complete sales pipeline workflows
- [ ] Improve frontend usability
- [ ] Expand automated test coverage
- [ ] Add CI/CD workflows
- [ ] Prepare deployment documentation

## Security

Do not commit credentials, production connection strings, JWT signing keys, or other secrets.

Use environment variables or a secure secret management solution for sensitive configuration.

## Author

**Umid Shamdinli**

Full Stack Developer | C#/.NET | React | Angular

[LinkedIn](https://www.linkedin.com/in/umidshamdinli) |
[GitHub](https://github.com/Umidshamdin)

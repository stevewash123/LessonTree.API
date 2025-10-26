# LessonTree API

A comprehensive .NET 8 Web API for managing educational content with dynamic lesson scheduling, drag-and-drop operations, and calendar integration.

> **🔗 Parent Project**: See [LessonTree README](../README.md) and [Full-Stack Setup Guide](../../FULL-STACK-SETUP-GUIDE.md) for complete project overview and standardized C#/Angular patterns.

## 🚀 Features

### Core Functionality
- **Dynamic Lesson Management**: Create, update, move, and organize lessons within topics and subtopics
- **Intelligent Schedule Generation**: Automatically generates calendar events based on lesson order and schedule configurations
- **Drag-and-Drop API**: RESTful endpoints supporting complex drag-and-drop operations with proper sort order management
- **Multi-Period Scheduling**: Support for multiple class periods with different courses per period
- **Real-time Calendar Integration**: Seamless integration with Angular calendar components

### Technical Highlights
- **Entity Framework Core**: Code-first approach with migrations and relationship management
- **AutoMapper Integration**: Comprehensive DTO mapping with custom configurations
- **Robust Error Handling**: Structured exception handling with detailed logging
- **100% Unit Test Coverage**: 184+ passing tests with comprehensive mocking
- **Clean Architecture**: Separation of concerns with service, repository, and API layers

## 🛠️ Technology Stack

- **.NET 8**: Latest LTS framework with improved performance
- **Entity Framework Core**: ORM with SQL Server support
- **AutoMapper**: Object-to-object mapping with custom profiles
- **Serilog**: Structured logging with file and console outputs
- **MSTest**: Unit testing framework with Moq for mocking
- **Swagger/OpenAPI**: Comprehensive API documentation

## 📋 API Endpoints

### Lesson Management
```http
POST   /api/lessons                    # Create new lesson
GET    /api/lessons/{id}              # Get lesson details
PUT    /api/lessons/{id}              # Update lesson
DELETE /api/lessons/{id}              # Delete lesson
POST   /api/lessons/{id}/move         # Move lesson with sort order management
```

### Schedule Management
```http
GET    /api/schedules/active                           # Get active schedule
GET    /api/schedules/{id}/events                      # Get all schedule events
GET    /api/schedules/{id}/events/daterange           # Get events by date range
POST   /api/schedules/{id}/regenerate                 # Regenerate schedule events
```

### Administrative
```http
GET    /api/admin/health              # Health check endpoint
GET    /api/admin/info                # System information
```

## 🏗️ Architecture

### Project Structure
```
LessonTree_API/
├── LessonTree.Api/              # Web API controllers and configuration
├── LessonTree.Service/          # Business logic and services
├── LessonTree.BLL/             # Business logic layer
├── LessonTree.Data/            # Entity Framework contexts and repositories
├── LessonTree.Tests/           # Unit tests (100% coverage)
└── LessonTree.Models/          # Data models and DTOs
```

### Key Services
- **LessonService**: Core lesson CRUD operations with move functionality
- **ScheduleService**: Dynamic schedule generation and event management
- **ScheduleGenerationService**: Intelligent calendar event creation
- **TreeDragDropService**: Complex drag-and-drop operation handling

### AutoMapper Profiles
Comprehensive mapping configurations including:
- `LessonResource ↔ LessonDetailResource`
- `ScheduleEvent ↔ ScheduleEventResource`
- Entity-to-DTO mappings with custom property handling

## 🗄️ Database Schema

### Core Entities
- **Courses**: Educational course definitions
- **Topics**: Course subdivisions with hierarchical structure
- **SubTopics**: Further topic subdivisions
- **Lessons**: Individual lesson content with sort ordering
- **Schedules**: Schedule configurations with period definitions
- **ScheduleEvents**: Generated calendar events with date/time information

### Key Relationships
- Lessons belong to SubTopics or Topics
- ScheduleEvents reference Lessons and Schedules
- Proper foreign key constraints with cascade rules

## 🧪 Testing

### Test Coverage
- **184+ Unit Tests**: Comprehensive coverage of all business logic
- **Service Layer Tests**: Complete testing of lesson operations and schedule generation
- **Repository Tests**: Database operation validation with in-memory providers
- **API Controller Tests**: HTTP endpoint validation and error handling

### Test Highlights
```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "Category=LessonService"
dotnet test --filter "Category=ScheduleGeneration"

# Test coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## 🚀 Getting Started

**See [Full-Stack Setup Guide](../../FULL-STACK-SETUP-GUIDE.md)** for standardized C#/Angular setup patterns and **critical Windows/WSL debugging considerations**.

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Quick Start
```bash
# Restore packages
dotnet restore

# Update database
dotnet ef database update --project LessonTree.Data

# Run application (recommended port)
dotnet run --project LessonTree.Api --urls=http://localhost:5000
```

## 🔧 Development

### Adding Migrations
```bash
dotnet ef migrations add YourMigrationName --project LessonTree.Data --startup-project LessonTree.Api
dotnet ef database update --project LessonTree.Data --startup-project LessonTree.Api
```

### Adding New Services
1. Create service interface in `LessonTree.Service/Interface/`
2. Implement service in `LessonTree.Service/Service/`
3. Register in `Program.cs` dependency injection
4. Add corresponding unit tests

### API Development
- Controllers follow RESTful conventions
- All endpoints include comprehensive error handling
- Swagger documentation auto-generated from XML comments
- Request/response DTOs with validation attributes

## 📊 Performance Features

### Optimizations
- **Efficient Queries**: Entity Framework query optimization with proper includes
- **Lazy Loading**: Strategic use of lazy loading for related entities
- **Caching Ready**: Architecture supports future caching layer implementation
- **Bulk Operations**: Optimized batch processing for schedule generation

### Monitoring
- Structured logging with correlation IDs
- Performance metrics tracking
- Health check endpoints for monitoring systems

## 🤝 Contributing

### Code Standards
- Follow C# coding conventions
- Maintain 100% unit test coverage
- Use AutoMapper for all DTO mappings
- Include XML documentation for public APIs

### Pull Request Process
1. Create feature branch from `main`
2. Implement changes with tests
3. Ensure all tests pass (`dotnet test`)
4. Update documentation as needed
5. Submit pull request with detailed description

## 📝 License

This project is part of a portfolio demonstration showcasing modern .NET development practices, clean architecture, and comprehensive testing strategies.

## 🔗 Complete Solution

This API is part of a full-stack educational management system:

- **Backend**: [LessonTree API](https://github.com/stevewash123/LessonTree.API) (this repository)
- **Frontend**: [LessonTree UI](https://github.com/stevewash123/LessonTree.UI) - Angular 19 with TypeScript
- **Integration**: RESTful API communication with real-time UI updates
- **Testing**: Comprehensive unit tests (API) + Cypress E2E tests (UI)

## 🎯 Future Enhancements

- Authentication and authorization integration
- Real-time notifications with SignalR
- Advanced caching strategies
- Microservices architecture considerations
- Docker containerization support

---

*Built with ❤️ using .NET 8 and modern development practices*
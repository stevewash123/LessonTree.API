# LessonTree.Tests

Comprehensive unit testing project for the LessonTree API, focusing on business logic validation and ensuring code reliability.

## Project Structure

```
LessonTree.Tests/
├── Controllers/              # Controller tests (HTTP concerns)
│   └── LessonControllerTests.cs
├── Services/                 # Service layer tests (business logic)
│   └── ScheduleGenerationServiceTests.cs
├── Repositories/             # Repository tests (data access business logic)
│   └── LessonRepositoryTests.cs
├── Helpers/                  # Test utilities and support classes
│   ├── TestBase.cs          # Base class for all tests
│   ├── TestDataBuilders.cs  # Builder pattern for test data
│   └── MockExtensions.cs    # Extensions for mocking Entity Framework
└── README.md                # This file
```

## Testing Framework & Tools

- **xUnit**: Primary testing framework (modern, fast, extensible)
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Readable, expressive assertions
- **Entity Framework InMemory**: For database-related testing
- **ASP.NET Core Testing**: For controller and integration testing
- **AutoMapper**: For DTO mapping testing

## Testing Philosophy

### Focus Areas (in priority order):
1. **ScheduleGenerationService**: Complex lesson-shifting algorithms for special day integration
2. **All CRUD controllers**: Basic HTTP response handling and error cases
3. **Repository business logic**: Positioning calculations and complex queries
4. **Basic coverage**: All components have at least basic test coverage

### Testing Patterns:
- **Arrange-Act-Assert (AAA)**: Clear test structure
- **Builder Pattern**: Clean, maintainable test data creation
- **Mocks over Database**: No test database, all dependencies mocked
- **Business Logic Focus**: Test complex algorithms, not simple CRUD

## Key Test Categories

### 1. ScheduleGenerationService Tests
**Focus**: Complex lesson-shifting algorithm for special day integration

```csharp
[Fact]
public void ShiftLessonsForwardFromDate_WithLessonsOnDate_ShouldShiftLessonsForward()
```

**Critical Scenarios Tested**:
- Finding next available teaching dates
- Shifting lessons forward from specific dates
- Special day integration with lesson conflicts
- Configuration validation for schedule generation
- Full schedule generation workflow

### 2. Controller Tests  
**Focus**: HTTP concerns, request/response handling, error scenarios

```csharp
[Fact]
public async Task GetLesson_WithNonExistentId_ShouldReturnNotFound()
```

**Scenarios Covered**:
- Standard CRUD operations (GET, POST, PUT, DELETE)
- Error handling (NotFound, BadRequest, Forbidden)
- Request validation
- Proper status code returns
- Authentication context handling

### 3. Repository Tests
**Focus**: Business logic, complex positioning calculations

```csharp
[Fact]
public void CalculateNewSortOrder_ForLessonInsertBetween_ShouldReturnCorrectOrder()
```

**Scenarios Covered**:
- Sort order calculations for drag-drop positioning
- Mixed entity positioning (Lessons + SubTopics)
- Complex lesson sequence ordering
- Business rule validation

## Test Data Management

### Test Data Builders
Clean, maintainable test data creation using the Builder pattern:

```csharp
var lesson = new LessonBuilder()
    .WithId(1)
    .WithTitle("Test Lesson")
    .WithUserId(userId)
    .WithTopicId(topicId)
    .Build();
```

### Factory Methods for Common Scenarios:
```csharp
TestDataBuilders.CreateBasicLesson(id: 1, userId: 1, topicId: 1)
TestDataBuilders.CreateLessonSequence(count: 5, userId: 1)
TestDataBuilders.CreateScheduleEventSequence(count: 10, startDate: DateTime.Today)
```

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "LessonControllerTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in specific category
dotnet test --filter "Category=ScheduleGeneration"
```

### Visual Studio
- Test Explorer shows all tests organized by namespace
- Right-click to run individual tests or test classes
- Debug tests by setting breakpoints and selecting "Debug Test"

## Test Configuration

### Base Test Class
All tests inherit from `TestBase` which provides:
- Pre-configured AutoMapper
- Logger factory for testing
- Common test utilities
- Proper disposal of resources

### Mocking Strategy
- **Repository Dependencies**: Mocked using Moq
- **Entity Framework**: Mocked using custom extensions
- **Logger**: Real logger instance for debugging
- **AutoMapper**: Real mapper with actual profiles

## Best Practices Implemented

### Test Naming
- **Pattern**: `MethodName_Scenario_ExpectedResult`
- **Example**: `GetLesson_WithNonExistentId_ShouldReturnNotFound`

### Test Organization
- **One class per tested class**: `ScheduleGenerationServiceTests` tests `ScheduleGenerationService`
- **Grouped by functionality**: Related tests in same region
- **Clear test methods**: Each test has single responsibility

### Assertion Style
```csharp
// Preferred: FluentAssertions
result.Should().NotBeNull();
result.Success.Should().BeTrue();
result.Errors.Should().BeEmpty();

// Instead of: Traditional assertions
Assert.NotNull(result);
Assert.True(result.Success);
Assert.Empty(result.Errors);
```

## Critical Business Logic Tested

### 1. Lesson Shifting Algorithm
The complex algorithm that shifts lessons forward when special days are inserted:
- **FindNextAvailableTeachingDate**: Finds next conflict-free date
- **ShiftLessonsForwardFromDate**: Recursively shifts conflicting lessons
- **ApplySpecialDayIntegrationAsync**: Coordinates special day integration

### 2. Positioning Calculations  
The drag-drop positioning logic that was previously buggy:
- **Sort order calculations**: Between existing items
- **Mixed entity positioning**: Lessons and SubTopics in same space
- **Edge cases**: Beginning, end, and empty containers

### 3. Schedule Generation
The complete workflow from configuration to schedule:
- **Configuration validation**: Date ranges, period assignments
- **Event generation**: For different period types
- **Sequence analysis**: Finding continuation points

## Debugging Tests

### Using Reflection for Private Methods
Some critical business logic is private. Tests use reflection to verify:

```csharp
var method = typeof(ScheduleGenerationService).GetMethod("FindNextAvailableTeachingDate", 
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var result = (DateTime?)method?.Invoke(_service, new object[] { events, startDate, period });
```

### Logger Output
Tests use real loggers to help with debugging:
```csharp
protected ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
```

## Continuous Integration

### Test Requirements for CI/CD:
- All tests must pass before deployment
- Code coverage should be tracked
- Critical path tests (ScheduleGenerationService) are mandatory
- Performance tests for complex algorithms

### Test Categories:
- **Unit Tests**: Fast, isolated, no external dependencies
- **Integration Tests**: Test multiple components together
- **Business Logic Tests**: Focus on complex algorithms

## Contributing

When adding new tests:

1. **Follow naming conventions**: `MethodName_Scenario_ExpectedResult`
2. **Use test data builders**: Don't create data inline
3. **Test business logic**: Focus on complex scenarios, not simple CRUD
4. **Add appropriate assertions**: Use FluentAssertions for readability
5. **Group related tests**: Use regions and clear organization
6. **Mock dependencies**: No real database or external services
7. **Update this README**: Document new test patterns or approaches
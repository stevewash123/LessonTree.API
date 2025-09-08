# LessonTree API (ASP.NET Core)

## Repository Context
- **This is the API repository** - separate from UI
- **GitLab**: https://gitlab.com/stevewash123/lessontree-api.git
- **Working Directory**: `C:/Users/steve/LessonTree/LessonTree_API/`
- **Main Project**: `LessonTree.Api/` (startup project)

## Quick Commands
```bash
# From LessonTree.Api directory
dotnet build                    # Build API
dotnet run                     # Start API (port 5046)

# Database operations  
dotnet ef database drop --force --context LessonTree.DAL.LessonTreeContext
dotnet ef database update --context LessonTree.DAL.LessonTreeContext

# Database migrations (run from API directory)
dotnet ef migrations add MigrationName --context LessonTree.DAL.LessonTreeContext --project ../LessonTree.DAL
dotnet ef database update --context LessonTree.DAL.LessonTreeContext

# Reset and reseed database
curl -X POST http://localhost:5046/api/admin/reset-and-reseed
```

## Project Structure
```
LessonTree_API/
├── LessonTree.Api/         (Web API - Controllers, Program.cs)
├── LessonTree.Service/     (Business Logic)
├── LessonTree.DAL/        (Data Access Layer)
└── LessonTree.Models/     (DTOs, Entities)
```

## Key APIs
- `/api/admin/health` - Health check
- `/api/admin/reset-and-reseed` - Database reset
- `/api/lesson/move` - Lesson positioning
- `/api/topic/move` - Topic positioning  
- `/api/subtopic/move` - SubTopic positioning
- `/account/login` - Authentication

## Positioning Contract
All move endpoints expect:
- `relativeToId`: Sibling entity ID (null for append)
- `position`: "before" or "after" (null if no positioning)
- `relativeToType`: Entity type (null if no positioning)

**Rule**: All three fields together or all null.

## Development Best Practices

### Entity Framework ORM Operations
**Best practice for saving lots of things**: Update the parent ORM object in memory, then save the parent. 

For example, if changing lots of `scheduleEvents`, modify the `schedule` object and save it once rather than saving individual events. This approach:
- Reduces database round trips
- Maintains referential integrity
- Leverages Entity Framework change tracking
- Improves performance for bulk operations

**Example**: When adding multiple special days or shifting lesson sequences, load the schedule, modify its events collection in memory, then call `SaveChanges()` on the schedule.
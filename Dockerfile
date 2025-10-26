# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies (in dependency order)
COPY LessonTree.Models/LessonTree.Models.csproj ./LessonTree.Models/
COPY LessonTree.DAL/LessonTree.DAL.csproj ./LessonTree.DAL/
COPY LessonTree.Service/LessonTree.BLL.csproj ./LessonTree.Service/
COPY LessonTree.Api/LessonTree.API.csproj ./LessonTree.Api/

# Restore dependencies
RUN dotnet restore LessonTree.Api/LessonTree.API.csproj

# Copy source code
COPY LessonTree.Models/ ./LessonTree.Models/
COPY LessonTree.DAL/ ./LessonTree.DAL/
COPY LessonTree.Service/ ./LessonTree.Service/
COPY LessonTree.Api/ ./LessonTree.Api/

# Build and publish the application
RUN dotnet build LessonTree.Api/LessonTree.API.csproj -c Release -o /app/build
RUN dotnet publish LessonTree.Api/LessonTree.API.csproj -c Release -o /app/publish

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose port 8080 (Render's default)
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "LessonTree.API.dll"]
# Use the official Microsoft .NET 7 SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the csproj file and restore any dependencies (via NuGet)
COPY ["./.git", "./.git"]
COPY ./msvc1/msvc1.csproj ./
RUN dotnet restore

# Copy the rest of the code and build the project
COPY . ./
RUN dotnet publish ./msvc1/msvc1.csproj -c Release -o out

# Generate the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "msvc1.dll"]

# Configure the container to listen on port 5001
ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001

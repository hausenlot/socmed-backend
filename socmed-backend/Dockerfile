# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore
COPY ["socmed-backend.csproj", "./"]
RUN dotnet restore "socmed-backend.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "socmed-backend.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "socmed-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .

# SQLite database volume
VOLUME /app/data

# Environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["dotnet", "socmed-backend.dll"]

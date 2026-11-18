FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
COPY Backend.csproj ./
RUN dotnet restore Backend.csproj
COPY . ./
RUN dotnet publish Backend.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_ENVIRONMENT=Production
CMD ["sh", "-c", "dotnet Backend.dll --urls http://0.0.0.0:${PORT:-8080}"]

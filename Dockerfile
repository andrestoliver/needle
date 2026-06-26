FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Needle.sln ./
COPY global.json ./

COPY src/Needle.Domain/Needle.Domain.csproj src/Needle.Domain/
COPY src/Needle.Application/Needle.Application.csproj src/Needle.Application/
COPY src/Needle.Infrastructure/Needle.Infrastructure.csproj src/Needle.Infrastructure/
COPY src/Needle.Api/Needle.Api.csproj src/Needle.Api/

RUN dotnet restore src/Needle.Api/Needle.Api.csproj

COPY src/ src/

RUN dotnet publish src/Needle.Api/Needle.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Needle.Api.dll"]
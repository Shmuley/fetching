FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet test
RUN dotnet publish Fetching.Service/Fetching.Service.csproj -c Release -o build

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

WORKDIR /app
COPY --from=build /app/build .

ENTRYPOINT [ "dotnet", "Fetching.Service.dll" ]

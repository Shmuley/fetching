FROM mcr.microsoft.com/dotnet/sdk:6.0 as build

WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o build

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

WORKDIR /app
COPY --from=build /app/build .

ENTRYPOINT [ "dotnet", "fetching.dll" ]

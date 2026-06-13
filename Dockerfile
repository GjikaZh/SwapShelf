FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SwapShelf/SwapShelf.csproj SwapShelf/
RUN dotnet restore SwapShelf/SwapShelf.csproj

COPY SwapShelf/ SwapShelf/
RUN dotnet publish SwapShelf/SwapShelf.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SwapShelf.dll"]

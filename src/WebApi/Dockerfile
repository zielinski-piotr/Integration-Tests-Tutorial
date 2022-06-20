FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/WebApi/Api.csproj", "WebApi/"]
COPY ["src/Auth.Data/Auth.Data.csproj", "Auth.Data/"]
COPY ["src/Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["src/Auth.Common/Auth.Common.csproj", "Auth.Common/"]
COPY ["src/Seeding/Seeding.csproj", "Seeding/"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
COPY ["src/Auth.Service/Auth.Service.csproj", "Auth.Service/"]
COPY ["src/Contract/Contract.csproj", "Contract/"]
COPY ["src/Data/Data.csproj", "Data/"]
COPY ["src/Service/Service.csproj", "Service/"]

RUN dotnet restore "WebApi/Api.csproj"
COPY . .
WORKDIR "/src/src/WebApi"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
#WORKDIR "/src/WebApi"
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]

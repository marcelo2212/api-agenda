FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Agenda.API/Agenda.API.csproj", "Agenda.API/"]
COPY ["Agenda.Application/Agenda.Application.csproj", "Agenda.Application/"]
COPY ["Agenda.Domain/Agenda.Domain.csproj", "Agenda.Domain/"]
COPY ["Agenda.Infrastructure/Agenda.Infrastructure.csproj", "Agenda.Infrastructure/"]
RUN dotnet restore "Agenda.API/Agenda.API.csproj"
COPY . .
WORKDIR "/src/Agenda.API"
RUN dotnet build "Agenda.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Agenda.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Agenda.API.dll"]

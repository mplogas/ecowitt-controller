#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080  

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ADD Ecowitt.Controller/ /src/Ecowitt.Controller
WORKDIR "/src/Ecowitt.Controller"
RUN dotnet restore "./Ecowitt.Controller.csproj"

FROM build AS publish
ENV ASPNETCORE_URLS=http://+:8080
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Ecowitt.Controller"
RUN dotnet publish "./Ecowitt.Controller.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecowitt.Controller.dll"]
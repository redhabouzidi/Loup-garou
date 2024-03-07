FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /server

COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet build -c Release -o out


ENTRYPOINT ["dotnet","run"]
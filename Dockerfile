FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
RUN apt-get update &&     apt-get install -y curl wget gnupg
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - &&     apt-get install -y nodejs &&     npm install --global yarn
RUN wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash

COPY Think4.csproj ./

COPY package.json yarn.lock* ./

RUN yarn install --frozen-lockfile || yarn install

COPY . .

RUN dotnet restore Think4.csproj

RUN dotnet publish Think4.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://*:5000

ENTRYPOINT ["dotnet", "Think4.dll"

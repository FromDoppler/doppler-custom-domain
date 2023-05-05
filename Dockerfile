FROM node:20 AS verify-format
WORKDIR /src
COPY package.json yarn.lock ./
RUN yarn
COPY . .
RUN yarn verify

FROM koalaman/shellcheck-alpine:v0.9.0 as verify-sh
WORKDIR /src
COPY ./*.sh ./
RUN shellcheck -e SC1091,SC1090 ./*.sh

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim AS restore
WORKDIR /src
COPY ./*.sln ./
COPY */*.csproj ./
COPY ./.config/dotnet-tools.json ./.config/
# Take into account using the same name for the folder and the .csproj and only one folder level
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet tool restore
RUN dotnet restore

FROM restore AS build
COPY . .
RUN dotnet dotnet-format --check
RUN dotnet build -c Release

FROM build AS test
RUN dotnet test

FROM build AS publish
RUN dotnet publish "DopplerCustomDomain/DopplerCustomDomain.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ARG version=unknown
RUN echo $version > /app/wwwroot/version.txt
ENTRYPOINT ["dotnet", "DopplerCustomDomain.dll"]

FROM  --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-resolute AS build
ARG TARGETARCH

WORKDIR /usr/src/app
COPY ./src/Application/*.csproj ./src/Application/
COPY ./src/Domain/*.csproj ./src/Domain/
COPY ./src/Infrastructure/*.csproj ./src/Infrastructure/
COPY ./src/Host/*.csproj ./src/Host/
RUN dotnet restore src/Host -a $TARGETARCH

COPY . ./
RUN dotnet publish src/Host -a $TARGETARCH -o /usr/app


FROM mcr.microsoft.com/dotnet/aspnet:10.0-resolute-chiseled

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
WORKDIR /usr/app

COPY --from=build /usr/app .
ENTRYPOINT ["/usr/app/Host"]

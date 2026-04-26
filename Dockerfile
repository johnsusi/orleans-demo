FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /usr/src/app
COPY . ./

ENV ASPNETCORE_URLS=http://+:8080

CMD ["dotnet", "run", "-c", "Release", "--project", "src/Host"]
FROM microsoft/dotnet:1.0.0-preview2-sdk
MAINTAINER jdpohl789@gmail.com

EXPOSE 8080
ENV ASPNETCORE_URLS http://0.0.0.0:8080/
RUN mkdir /src
COPY src /src
RUN cd /src && dotnet restore
ENTRYPOINT cd /src/Services/MyVote.Services.AppServer.Core && dotnet run

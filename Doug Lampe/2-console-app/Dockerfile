FROM microsoft/dotnet:latest

# Copy startup files into container: 
COPY /src/* /app/

# Build applicationi:
WORKDIR /app
COPY src/CodeCamp2017.Cli/bin/Debug/netcoreapp1.1/publish .

ENTRYPOINT ["dotnet", "CodeCamp2017.Cli.dll"]
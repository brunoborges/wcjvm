# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/sdk:7.0-windowsservercore-ltsc2022

# Set the working directory
WORKDIR /app

# Copy csproj and restore any dependencies (via dotnet restore)
COPY *.csproj ./
RUN dotnet restore

# Publish the application
COPY . ./
RUN dotnet publish -c Release -o out

# Specify the entry point of the application
CMD ["/app/out/wcjvm.exe"]

# Build the .NET application
dotnet publish -c Release -o out

# Build the Docker image
docker build -t wcjvm -f Slim.Dockerfile .

# Run the Docker container in Process Isolation Mode with limited memory/cpu
docker run --memory=256m --cpus 2 --isolation=process -ti wcjvm

docker build -t javaapp -f .\JavaApp.Dockerfile .

# Run a Java application in Process Isolation Mode with limited memory/cpu
docker run --memory=512m --cpus 2 --isolation=process -ti javaapp


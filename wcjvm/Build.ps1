# Build the .NET application
dotnet publish -c Release -o out

# Build the Docker image
docker build -t wcjvm -f Slim.Dockerfile .

# Run the Docker container in Process Isolation Mode with limited memory/cpu
docker run --memory=128m --cpus 2 --isolation=process -ti wcjvm

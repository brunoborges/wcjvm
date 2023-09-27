# Build the .NET application
dotnet publish -c Release -o out

# Build the Docker image
docker build -t wcjvm -f Slim.Dockerfile .

# Run the Docker container in Process Isolation Mode with limited memory/cpu
docker run --memory=256m --cpus 2 --isolation=process -ti wcjvm

docker build -t javaapp -f .\JavaApp.Dockerfile .

docker run --memory=512m --cpus 2 --isolation=process -ti javaapp

# CompressedClassSpaceSize                 = 1073741824
# HeapBaseMinAddress                       = 2147483648
# HeapSizePerGCThread                      = 43620760
# InitialHeapSize                          = 402653184
# LargePageHeapSizeThreshold               = 134217728
# LargePageSizeInBytes                     = 0

# MarkStackSizeMax                         = 536870912
# MaxHeapSize                              = 402653184 (OK)
# MaxMetaspaceExpansion                    = 5439488
# MaxNewSize                               = 241172480 (BAD)
# GC selection not happening as expected with MaxRAM configured manually.
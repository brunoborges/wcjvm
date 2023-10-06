FROM mcr.microsoft.com/windows/servercore:ltsc2022

RUN powershell -Command \
    $ErrorActionPreference = 'Stop'; \
    $ProgressPreference = 'SilentlyContinue'; \
    Invoke-WebRequest -OutFile microsoft-jdk-17.0.8.1-windows-x64.zip -Uri https://aka.ms/download-jdk/microsoft-jdk-17.0.8.1-windows-x64.zip; \
    Expand-Archive microsoft-jdk-17.0.8.1-windows-x64.zip -DestinationPath C:\; \
    Remove-Item microsoft-jdk-17.0.8.1-windows-x64.zip -Force

COPY out/native/wcjvm.exe /app/wcjvm.exe

ENV JAVA_HOME="C:\jdk-17.0.8.1+1"
ENV PATH="$JAVA_HOME\bin;$PATH:/app"

ENTRYPOINT ["/app/wcjvm.exe"]

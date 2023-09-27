FROM mcr.microsoft.com/windows/servercore:ltsc2022

COPY out/wcjvm.exe /app/wcjvm.exe

CMD ["/app/wcjvm.exe"]

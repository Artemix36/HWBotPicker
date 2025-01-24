FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /HWbotDotnet

COPY /bin/Release/net8.0/publish ./
COPY var.yaml ./
EXPOSE 8082

CMD ["dotnet", "HWPicker_NewDotNet.dll"]
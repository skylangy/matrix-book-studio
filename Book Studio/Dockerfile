# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js and build tools (for building Angular client from server project)
RUN apt-get update && \
    apt-get install -y curl build-essential && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    node -v && npm -v && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/*

COPY ["MatrixBook.Server/MatrixBook.Server.csproj", "MatrixBook.Server/"]
RUN dotnet restore "MatrixBook.Server/MatrixBook.Server.csproj"
COPY . .

WORKDIR /src/matrixbook.client
# RUN npm cache clean --force && npm install --force && npm ci
RUN cd ../matrixbook.client && npm cache clean --force && npm install --force
RUN npm run build

WORKDIR /src/MatrixBook.Server
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8090
ENV ASPNETCORE_URLS=http://+:8090

# Install ffmpeg and clean up to reduce size
RUN apt-get update && \
    apt-get install -y --no-install-recommends ffmpeg openssh-server rsync && \
    mkdir -p /var/run/sshd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/*

COPY --from=build /app/publish .

# Setup non-root user
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "MatrixBook.Server.dll"]

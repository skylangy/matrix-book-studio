
// Need to give permission for the folder storage ravendb data
sudo chown -R $(whoami):staff /Users/andylang/MatrixBook/RavenDb


//
```
docker-compose build --no-cache

docker-compose --env-file env.windows up -d
```

docker build -t matrix.studio/latest .

docker run -d -p 8965:8080 --name matrix.studio matrix.studio/latest

docker-compose up -d

docker-compose up -d --pull always

docker run -d -p 8965:8090 --name matrix.studio matrix.studio/latest
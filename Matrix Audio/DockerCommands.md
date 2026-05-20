
// build and start for windows
docker-compose --env-file env.windows up

// build for linux
docker-compose build --no-cache --env-file env.linux


//
docker-compose build

docker push http://192.168.7.218:8961/matrixaudio:latest



// build for arm

docker buildx create --use

// list all builders
docker buildx ls

// switch context/builder
docker context use default
docker buildx use default

// push to docker registry
docker buildx build --platform linux/arm64 -t 192.168.7.218:8961/matrixaudio:latest --push .
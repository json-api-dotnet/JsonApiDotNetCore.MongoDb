#Requires -Version 7.0

# This script starts a MongoDB database in a docker container, which is required for running examples locally.

docker container stop jsonapi-mongo-db
docker run --pull always --rm --detach --name jsonapi-mongo-db -p 27017:27017 mongo:latest

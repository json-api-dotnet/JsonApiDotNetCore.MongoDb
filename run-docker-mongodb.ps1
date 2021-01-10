#Requires -Version 7.0

# This script starts a docker container with MongoDB database, used for running tests.

docker container stop jsonapi-dotnet-core-mongodb-testing

docker run --rm --name jsonapi-dotnet-core-mongodb-testing       `
 -p 27017:27017                                                  `
 mongo:latest

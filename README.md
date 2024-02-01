# Project Title

[![GitHub Actions Status](https://github.com/Username/Project/workflows/Build/badge.svg?branch=main)](https://github.com/Username/Project/actions)

[![GitHub Actions Build History](https://buildstats.info/github/chart/Username/Project?branch=main&includeBuildsFromPullRequest=false)](https://github.com/Username/Project/actions)

Project Description

## Schema Grains
Registry: https://github.com/dyn-poc/schema-orleans-bun/blob/main/Source/schema.Grains/SchemaRegistryGrain.cs
Guest: https://github.com/dyn-poc/schema-orleans-bun/blob/main/Source/schema.Grains/GuestSchemaGrain.cs

## Schema Client
### Api
/Source/schema.web/Controllers/SchemaController.cs

### Client
/Source/schema.web/ClientApp


## Run 
- use launch profile `.run/launch.run.xml` to run the project
- or run `dotnet run --project source/schema.server/schema.server.csproj` and `dotnet run --project source/schema.web/schema.web.csproj` from the root folder



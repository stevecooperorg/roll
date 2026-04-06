.PHONY: build test run

SLN := DiceExpressions.sln
ROLL_PROJ := src/Roll/Roll.csproj

ARGS ?= 2d6+1

build:
	dotnet build $(SLN) --configuration Release

test:
	dotnet test $(SLN) --configuration Release

run:
	dotnet run --project $(ROLL_PROJ) --configuration Release -- $(ARGS)

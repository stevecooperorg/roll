.PHONY: all build test rust-build rust-test csharp-build csharp-test rust-roll csharp-roll

ARGS ?= 2d6+1

all: build test

build: rust-build csharp-build

test: rust-test csharp-test

rust-build:
	$(MAKE) -C rs build

rust-test:
	$(MAKE) -C rs test

csharp-build:
	$(MAKE) -C csharp build

csharp-test:
	$(MAKE) -C csharp test

rust-roll:
	$(MAKE) -C rs run ARGS="$(ARGS)"

csharp-roll:
	$(MAKE) -C csharp run ARGS="$(ARGS)"

ifndef BUILD_GROUP
	BUILD_GROUP="mvm-backend"
endif

ifndef BUILD_NUMBER
	BUILD_NUMBER="1.0.0"
endif

ifndef BRANCH
	BRANCH=$(git symbolic-ref -q HEAD)
	BRANCH=${BRANCH##refs/heads/}
	BRANCH=${BRANCH:-HEAD}
endif

ifndef HASH
	HASH=$(git rev-parse HEAD)
endif

ifndef BUILD_USER
	BUILD_USER=$(git --no-pager show -s --format='<mailto:%ae|%an>' $HASH)
endif

.PHONY: help

help: ## This help.
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}' $(MAKEFILE_LIST)

.DEFAULT_GOAL := help

clean:
	find . -type d -name bin -prune -exec rm -rf {} \;
	find . -type d -name obj -prune -exec rm -rf {} \;

docker-build:
	docker-compose -f infra-compose.yml -f docker-compose.yml build

docker-push:
	docker-compose -f infra-compose.yml -f docker-compose.yml build games
	docker-compose -f infra-compose.yml -f docker-compose.yml push games

docker-infra:
	docker-compose -f infra-compose.yml -f infra-other-compose.yml up -d

docker-test:
	docker-compose -f infra-compose.yml up -d
	#optional sleep 15 #wait for infra to come up
	docker-compose -f infra-compose.yml -f docker-compose.yml run tests

docker-clean:
	-docker stop `docker ps -aq`
	-docker rm `docker ps -aq`
	-docker rmi `docker images -qf dangling=true`
	-docker volume rm `docker volume ls -qf dangling=true`
	docker rmi `docker images --format '{{.Repository}}:{{.Tag}}' | grep "chadgrant/games"` -f

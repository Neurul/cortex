1. From CLI, in current folder
	- docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
2. If building/uploading to docker hub
	1. Update "docker-compose.yml" version, commit to repository
	2. Remove all running docker images (kitematic)
	3. From CLI
		1. docker-compose -f docker-compose.yml -f docker-compose.override.yml build
		2. docker push [image name]
			- docker push neurul/cortex.out.api:0.2.1
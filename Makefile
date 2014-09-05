SERVER = thomas@localhost

.PHONY:
all:
	@echo 'make deploy'
	@echo 'make SERVER=thomas@ukeep.it deploy'
	@echo 'while ( true ) do make deploy ; sleep 2 ; done'

.PHONY: deploy
deploy:
	rsync -avzP ssl $(SERVER):/srv/data/uKeepIt/
	rsync -avzP nginx.conf $(SERVER):/srv/data/uKeepIt/
	rsync -avzP start $(SERVER):/srv/data/uKeepIt/

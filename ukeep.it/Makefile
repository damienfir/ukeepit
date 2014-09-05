SERVER = thomas@localhost
PHP_FILES = $(shell find . -type f -name '*.php' -not -name '*.inc.php')
PHP_INC_FILES = $(shell find . -type f -name '*.inc.php')
HTML_FILES = $(patsubst %.php, %.html, $(PHP_FILES))
OTHER_FILES = $(shell find . -type f \( -name '*.png' -o -name '*.css' -o -name '*.jpg' -o -name '*.js' \))

.PHONY:
all:
	@echo 'make deploy'
	@echo 'make SERVER=thomas@ukeep.it deploy'
	@echo 'while ( true ) do make deploy ; sleep 2 ; done'

.PHONY: deploy
deploy: .$(SERVER)-update

.$(SERVER)-update: $(HTML_FILES) $(OTHER_FILES)
	touch .$(SERVER)-update-running
	rsync -avzP --delete --exclude '*.php' --exclude /Makefile --exclude '.*' . $(SERVER):/srv/data/uKeepIt/ukeep.it/
	mv .$(SERVER)-update-running .$(SERVER)-update

%.html: %.php $(PHP_INC_FILES)
	php '$<' > '$@'


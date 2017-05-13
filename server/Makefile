.PHONY:all clean linux macosx

PLAT=linux
linux:PLAT := linux
macosx:PLAT := macosx

linux macosx:all

all:silly/silly

silly/silly:silly/Makefile
	make -C silly $(PLAT)


clean:
	make -C silly clean


CC = gcc
CFLAGS = -Wall -g

all: myshell looper mypipe

myshell: myshell.o LineParser.o
	$(CC) $(CFLAGS) -o myshell myshell.o LineParser.o

looper: looper.o
	$(CC) $(CFLAGS) -o looper looper.o

mypipe: mypipe.o
	$(CC) $(CFLAGS) -o mypipe mypipe.o

myshell.o: myshell.c LineParser.h
	$(CC) $(CFLAGS) -c myshell.c

LineParser.o: LineParser.c LineParser.h
	$(CC) $(CFLAGS) -c LineParser.c

looper.o: looper.c
	$(CC) $(CFLAGS) -c looper.c

mypipe.o: mypipe.c
	$(CC) $(CFLAGS) -c mypipe.c

clean:
	rm -f *.o myshell looper mypipe

.PHONY: all clean
# Compiler and flags
CC = gcc
CFLAGS = -Wall -Wextra -g

# Targets
all: myshell looper mypipeline

# Build myshell with object files
myshell: myshell.o LineParser.o
	$(CC) $(CFLAGS) -o myshell myshell.o LineParser.o

# Build looper
looper: looper.o
	$(CC) $(CFLAGS) -o looper looper.o

# Build mypipeline (Part 1)
mypipeline: mypipeline.o
	$(CC) $(CFLAGS) -o mypipeline mypipeline.o

# Compile myshell.c to object file
myshell.o: myshell.c LineParser.h
	$(CC) $(CFLAGS) -c myshell.c

# Compile LineParser.c to object file
LineParser.o: LineParser.c LineParser.h
	$(CC) $(CFLAGS) -c LineParser.c

# Compile looper.c to object file
looper.o: looper.c
	$(CC) $(CFLAGS) -c looper.c

# Compile mypipeline.c to object file
mypipeline.o: mypipeline.c
	$(CC) $(CFLAGS) -c mypipeline.c

# Clean build artifacts
clean:
	rm -f *.o myshell looper mypipeline

# Run myshell
run: myshell
	./myshell

# Run myshell in debug mode
debug: myshell
	./myshell -d

# Run mypipeline
run-pipe: mypipeline
	./mypipeline

.PHONY: all clean run debug run-pipe
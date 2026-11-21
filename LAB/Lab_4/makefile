# Compiler and flags
CC = gcc
CFLAGS = -Wall -g
CFLAGS_32BIT = -m32 -fno-pie -fno-stack-protector

# Targets
all: task1 task4

# Task 1: hexeditplus
task1: task1.c
	$(CC) $(CFLAGS) -o task1 task1.c

# Task 4: fixed_counter (32-bit with special flags)
task4: task4.c
	$(CC) $(CFLAGS_32BIT) -o task4 task4.c

# Clean up
clean:
	rm -f task1 task4

# Phony targets
.PHONY: all clean
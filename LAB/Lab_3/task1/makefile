CC = gcc
ASM = nasm
LD = ld

CFLAGS = -m32 -Wall -ansi -c -nostdlib -fno-stack-protector
ASMFLAGS = -f elf32
LDFLAGS = -m elf_i386

OBJS = start.o  util.o
TARGET = start

all: $(TARGET)

$(TARGET): $(OBJS)
	$(LD) $(LDFLAGS) $(OBJS) -o $(TARGET)

start.o: start.s
	$(ASM) $(ASMFLAGS) start.s -o start.o

# encoder.o: encoder.s
# 	$(ASM) $(ASMFLAGS) encoder.s -o encoder.o

util.o: util.c util.h
	$(CC) $(CFLAGS) util.c -o util.o

clean:
	rm -f $(OBJS) $(TARGET)

.PHONY: all clean
CC = gcc
ASM = nasm
LD = ld

CFLAGS = -m32 -Wall -ansi -c -nostdlib -fno-stack-protector
ASMFLAGS = -f elf32
LDFLAGS = -m elf_i386

OBJS = start.o virus.o util.o
TARGET = virus

all: $(TARGET)

$(TARGET): $(OBJS)
	$(LD) $(LDFLAGS) $(OBJS) -o $(TARGET)

start.o: start.s
	$(ASM) $(ASMFLAGS) start.s -o start.o

virus.o: virus.c util.h
	$(CC) $(CFLAGS) virus.c -o virus.o

util.o: util.c util.h
	$(CC) $(CFLAGS) util.c -o util.o

clean:
	rm -f $(OBJS) $(TARGET)

# Create test directory with dummy files
test-setup:
	mkdir -p test_dir
	echo "test content 1" > test_dir/test1.txt
	echo "test content 2" > test_dir/test2.txt
	echo "other content" > test_dir/other.txt
	echo "file content" > test_dir/file1.txt
	cp $(TARGET) test_dir/

test-list:
	cd test_dir && ./$(TARGET)

test-infect:
	cd test_dir && ./$(TARGET) -ate

.PHONY: all clean test-setup test-list test-infect
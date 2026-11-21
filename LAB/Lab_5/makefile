# Makefile for ELF Loader - Lab 5

# Compiler and flags
CC = gcc
CFLAGS = -m32 -Wall -g

# Assembler (choose one: NASM or GAS)
# For NASM (if start.s is in NASM syntax):
ASM = nasm
ASMFLAGS = -f elf32

# For GAS (if start.s is in GAS syntax):
# ASM = as
# ASMFLAGS = --32

# Linker and flags
LD = ld
LDFLAGS = -m elf_i386

# Check if we're on a 64-bit system
ARCH := $(shell uname -m)
ifeq ($(ARCH),x86_64)
    DYNAMIC_LINKER = /lib32/ld-linux.so.2
    LIBPATH = /usr/lib32
else
    DYNAMIC_LINKER = /lib/ld-linux.so.2
    LIBPATH = /usr/lib
endif

# Target executable name
TARGET = my_loader

# Source files
SRCS = my_loader.c

# Object files
OBJS = $(SRCS:.c=.o)

# Additional object files (start.o for system_call interface, startup.o for Task 2d)
# You should have start.o from Lab 3 and startup.o from Lab 5
SUPPORT_OBJS = start.o startup.o

# Default target - uses custom linking for Task 2a
all: $(TARGET)

# Task 0-1: Build with gcc (simple version)
simple: $(OBJS)
	$(CC) $(CFLAGS) -o $(TARGET) $(OBJS)

# Task 2a: Build with custom linker script
$(TARGET): $(OBJS) $(SUPPORT_OBJS) linking_script
	@echo "=== Linking with custom script ==="
	@echo "Command: $(LD) $(LDFLAGS) -o $(TARGET) $(OBJS) $(SUPPORT_OBJS) -L$(LIBPATH) -lc -T linking_script -dynamic-linker $(DYNAMIC_LINKER)"
	$(LD) $(LDFLAGS) -o $(TARGET) $(OBJS) $(SUPPORT_OBJS) \
		-L$(LIBPATH) -lc \
		-T linking_script \
		-dynamic-linker $(DYNAMIC_LINKER)

# Compile C source files to object files
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# Assemble .s files with NASM
%.o: %.s
	$(ASM) $(ASMFLAGS) $< -o $@

# Verify the loader was linked correctly
verify: $(TARGET)
	@echo "=== Verifying loader binary ==="
	@echo ""
	@echo "1. Entry point address (should be around 0x00200000):"
	@readelf -h $(TARGET) | grep "Entry point"
	@echo ""
	@echo "2. Full ELF header:"
	@readelf -h $(TARGET)
	@echo ""
	@echo "3. Program headers (check base address):"
	@readelf -l $(TARGET) | head -20

# Clean up build files
clean:
	rm -f $(OBJS) $(TARGET)

# Run the loader with a test file (change test_file as needed)
test: $(TARGET)
	./$(TARGET) /bin/ls

# Display help
help:
	@echo "Available targets:"
	@echo "  all     - Build the loader with custom linking (Task 2a)"
	@echo "  simple  - Build without custom linking (Tasks 0-1)"
	@echo "  verify  - Verify the loader was linked correctly"
	@echo "  clean   - Remove build files"
	@echo "  test    - Build and run with /bin/ls"
	@echo "  help    - Display this help message"
	@echo ""
	@echo "Note: You need start.o from Lab 3 to build with custom linking"

# Phony targets (not actual files)
.PHONY: all simple verify clean test help
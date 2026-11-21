# Makefile for myELF - Lab E

# Compiler and flags
CC = gcc
CFLAGS = -m32 -g -Wall -Wextra
LDFLAGS = -m32

# Target executable
TARGET = myELF

# Source files
SRCS = myELF.c

# Object files
OBJS = $(SRCS:.c=.o)

# Default target
all: $(TARGET)

# Link the executable
$(TARGET): $(OBJS)
	$(CC) $(LDFLAGS) -o $(TARGET) $(OBJS)
	@echo "Build complete: $(TARGET)"

# Compile source files to object files
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# Clean build artifacts
clean:
	rm -f $(OBJS) $(TARGET)
	@echo "Clean complete"

# Phony targets
.PHONY: all clean run test

# Run the program
run: $(TARGET)
	./$(TARGET)

# Test with a sample ELF file (adjust the path as needed)
test: $(TARGET)
	@echo "Testing with $(TARGET) itself..."
	@echo "1" | ./$(TARGET)
	@echo "$(TARGET)" | ./$(TARGET)
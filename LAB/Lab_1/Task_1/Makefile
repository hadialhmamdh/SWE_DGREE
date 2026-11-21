CC = gcc
CFLAGS = -Wall -Wall -g -m32
TARGET = addresses
SRCS = addresses.c

all: $(TARGET)

$(TARGET): $(SRCS)
	$(CC) $(CFLAGS) -o $(TARGET) $(SRCS)

clean:
	rm -f $(TARGET) *.o

.PHONY: all clean

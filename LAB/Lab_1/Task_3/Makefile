CC = gcc
CFLAGS = -Wall -g -m32
TARGET = menu
SRC = menu_map.c

.PHONY: all clean

all: $(TARGET)

$(TARGET): $(SRC)
	$(CC) $(CFLAGS) -o $(TARGET) $(SRC)

clean:
	rm -f $(TARGET) *.o

CC = gcc
CFLAGS = -g -Wall -m32
TARGET = count-words
SRC = count-words.c

.PHONY: all clean run 

all: $(TARGET)

$(TARGET): $(SRC)
	$(CC) $(CFLAGS) -o $(TARGET) $(SRC)

run: $(TARGET)
	./$(TARGET)

clean:
	rm -f $(TARGET) *.o

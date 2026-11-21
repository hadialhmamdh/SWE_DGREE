CC = gcc
CFLAGS = -g -Wall -Wextra -std=c99 -lm
TARGET1 = bubblesort
TARGET2 = hexaPrint
TARGET3 = AntiVirus

all: $(TARGET1) $(TARGET2) $(TARGET3)

$(TARGET1): lab3_bubblesort.c
	$(CC) $(CFLAGS) -o $(TARGET1) lab3_bubblesort.c

$(TARGET2): hexaPrint.c
	$(CC) $(CFLAGS) -o $(TARGET2) hexaPrint.c

$(TARGET3): AntiVirus.c
	$(CC) $(CFLAGS) -o $(TARGET3) AntiVirus.c

debug_bubble: $(TARGET1)
	valgrind --leak-check=full --show-reachable=yes --track-origins=yes -v ./$(TARGET1) 5 3 8 1 9 2

debug_hexa: $(TARGET2)
	valgrind --leak-check=full --show-reachable=yes ./$(TARGET2) exampleFile

debug_antivirus: $(TARGET3)
	valgrind --leak-check=full --show-reachable=yes --track-origins=yes ./$(TARGET3)

run_bubble: $(TARGET1)
	./$(TARGET1) 5 3 8 1 9 2

run_hexa: $(TARGET2)
	./$(TARGET2) exampleFile

run_antivirus: $(TARGET3)
	./$(TARGET3)

clean:
	rm -f $(TARGET1) $(TARGET2) $(TARGET3)

.PHONY: all debug_bubble debug_hexa debug_antivirus run_bubble run_hexa run_antivirus clean
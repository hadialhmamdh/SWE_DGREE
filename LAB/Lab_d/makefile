CC = gcc
NASM = nasm
CFLAGS = -m32 -g -no-pie
NASMFLAGS = -f elf32

# Final program (uses assembly main)
multi: multi.o
	$(CC) $(CFLAGS) -o multi multi.o

multi.o: multi.s
	$(NASM) $(NASMFLAGS) multi.s -o multi.o

clean:
	rm -f *.o multi test_multi

.PHONY: clean
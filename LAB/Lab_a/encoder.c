#include <stdlib.h>
#include <stdio.h>
#include <string.h>

// --------------------parse encoding --------------------

char *encoding_string = "0";  
int encoding_index = 0;
int add_mode = 1; 

void parse_encoding(int argc, char **argv) {
    for (int i = 1; i < argc; i++) {
        if (argv[i][1] == 'E') {
            add_mode = (argv[i][0] == '+') ? 1 : 0;
            encoding_string = (argv[i][2] == '\0') ? "0" : &argv[i][2];
        }
    }
}


//----------------------------------debugger ----------------------------------
int debug_mode = 1;
unsigned char password[] = "1234";

void parse_debug(int argc, char **argv) {
    for (int i = 0; i < argc; i++) {
        if (debug_mode) {
            fprintf(stderr, "%s\n", argv[i]);
        }

        if (strcmp(argv[i], "-D") == 0) {
            debug_mode = 0;
        } 
        else if (argv[i][0] == '+' && argv[i][1] == 'D') {
            char *given = &argv[i][2];
            if (strcmp(given, (char *)password) == 0) {
                debug_mode = 1;
            }
        }
    }
}


// --------------------parse input/output files --------------------
FILE *infile = NULL;
FILE *outfile = NULL;

int parse_files(int argc, char **argv) {
    infile = stdin;
    outfile = stdout;

    for (int i = 1; i < argc; i++) {
        if (argv[i][0] == '-' && argv[i][1] == 'i') {
            infile = fopen(&argv[i][2], "r");
            if (!infile) {
                fprintf(stderr, "Error opening input file: %s\n", &argv[i][2]);
                return 1;
            }
        } else if (argv[i][0] == '-' && argv[i][1] == 'o') {
            outfile = fopen(&argv[i][2], "w");
            if (!outfile) {
                fprintf(stderr, "Error opening output file: %s\n", &argv[i][2]);
                return 1;
            }
        }
    }
    return 0;
}

//---------------------------------encoder -----------------------------------------
char encode(char c) {
    unsigned char key_char;
    int shift;

    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z')) {
        key_char = encoding_string[encoding_index];
        shift = key_char - '0';
        if (!add_mode)
            shift = -shift;

        encoding_index++;
        if (encoding_string[encoding_index] == '\0')
            encoding_index = 0;

        if (c >= '0' && c <= '9') {
            int range = 10;
            c = '0' + ((c - '0' + shift + range) % range);
        } else if (c >= 'A' && c <= 'Z') {
            int range = 26;
            c = 'A' + ((c - 'A' + shift + range) % range);
        }
    }
    return c;
}


int main(int argc, char **argv) {
    if (parse_files(argc, argv)) return 1;

    parse_debug(argc, argv);
    parse_encoding(argc, argv);

    int c;
    while ((c = fgetc(infile)) != EOF) {
        char encoded = encode((char)c);
        fputc(encoded, outfile);
    }

    if (infile != stdin) fclose(infile);
    if (outfile != stdout) fclose(outfile);

    return 0;
}

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

//------------------ task 2 functions -------------------------------------------
char* map(char *array, int array_length, char (*f) (char)){
  char* mapped_array = (char*)(malloc(array_length*sizeof(char)));
  /* TODO: Complete during task 2.a */
  for (int i = 0; i < array_length; i++) {
    char temp = f(array[i]);
    mapped_array[i] = temp;    }
  return mapped_array;
}
 
char my_get(char c) {
    return fgetc(stdin);
}


char cxprt(char c) {
    if (c >= 0x20 && c <= 0x7E)
        printf("%c ", c);
    else
        printf(". ");
    printf("%02X\n", (unsigned char)c);
    return c;
}

char encrypt(char c) {
    if (c >= 0x20 && c <= 0x7E)
        return c + 1;
    else
        return c;
}

char decrypt(char c) {
    if (c >= 0x21 && c <= 0x7F)
        return c - 1;
    else
        return c;
}

char dprt(char c) {
    printf("%d\n", c);
    return c;
}

typedef struct {
  char *name;
  char (*fun)(char);
} fun_desc;

fun_desc menu[] = {
    {"Get string", my_get},
    {"Encrypt", encrypt},
    {"Decrypt", decrypt},
    {"Print dec", dprt},
    {"Print char + hex", cxprt},
    {NULL, NULL}
};

void printMenu() {
    printf("Select operation from the menu:\n");
    for (int i = 0; menu[i].name != NULL; i++)
        printf("%d %s\n", i, menu[i].name);
}

void flush_stdin() {
    int c;
    while ((c = getchar()) != '\n' && c != EOF);
}

int main() {
    const int ARR_LEN = 4; // 4 characters + '\0'
    char *carray = (char *)malloc((ARR_LEN + 1) * sizeof(char));
    carray[0] = '\0';
    if (!carray) {
        perror("malloc failed");
        return 1;
    }
    carray[0] = '\0';

    int bound = (sizeof(menu) / sizeof(fun_desc)) - 1;
    char s[100];

    printMenu();
    while (fgets(s, sizeof(s), stdin) != NULL) {
        int select = atoi(s);
     if (select >= 0 && select < bound) {
      printf("Within bounds\n");

      if (select == 0) {
        if (fgets(carray, 256, stdin) == NULL) {
          printf("\n");
          free(carray);
          break;
        }
        carray[strcspn(carray, "\n")] = '\0';
      } else {
        char *new_c = map(carray, strlen(carray), menu[select].fun);
        if (new_c != NULL) {
          free(carray);
          carray = new_c;
        }
      }
      printf("DONE.\n");
    } else {
            printf("Not within bounds\n");
            break;
        }

        printMenu();
    }

    free(carray);
    return 0;
}
/*if ((select >= 0) && (select < bound))
    {
      printf("Within bounds\n");
      char *temp = map(carray, 5, menu[select].fun);
      //get the newline after fgetc
      if(select==0){
        char buffer[2];
        fgets(buffer,2,stdin);
      }
      if (carray != NULL)
        free(carray);
      carray = temp;
    }
    else
    {
      printf("Not within bounds\n");
      exit(1);
    }
    printMenu();
  }*/
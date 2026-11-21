#include <stdio.h>

int count_digits(char* str) {
    int count = 0;
    while (*str != '\0') {
        if (*str >= '0' && *str <= '9') {
            count++;
        }
        str++;
    }
    return count;
}

int main(int argc, char* argv[]) {
    if (argc != 2) {
        printf("Usage: %s <string>\n", argv[0]);
        return 1;
    }
    
    int result = count_digits(argv[1]);
    printf("String contains %d digits\n", result);
    return 0;
}
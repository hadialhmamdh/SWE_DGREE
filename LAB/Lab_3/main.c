#include "util.h"

#define SYS_WRITE 4
#define STDOUT 1
#define SYS_OPEN 5
#define O_RDWR 2
#define SYS_SEEK 19
#define SEEK_SET 0
#define SHIRA_OFFSET 0x291

extern int system_call();

int main (int argc , char* argv[], char* envp[])
{
  /*Complete the task here*/
  int i;
    /* Print each argument on a separate line */
    for (i = 0; i < argc; i++) {
        /* Print the argument string */
        system_call(4, STDOUT, argv[i], strlen(argv[i]));
        
        /* Print a newline character */
        system_call(4, STDOUT, "\n", 1);
    }
    
    return 0;
}

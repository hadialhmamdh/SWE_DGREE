#include <stdio.h>
#include <signal.h>
#include <string.h>
#include <unistd.h>
#include <stdlib.h>

void handler(int sig)
{
    printf("\nReceived Signal: %s\n", strsignal(sig));
    
    if (sig == SIGTSTP)
    {
        signal(SIGCONT, handler);
        signal(SIGTSTP, SIG_DFL);
        raise(sig);
    }
    else if (sig == SIGCONT)
    {
        signal(SIGTSTP, handler);
        signal(SIGCONT, SIG_DFL);
        raise(sig);
    }
    else if (sig == SIGINT)
    {
        signal(SIGINT, SIG_DFL);
        raise(sig);
    }
}

int main(int argc, char **argv)
{
    printf("Starting the program\n");
    printf("Process ID: %d\n", getpid());
    
    signal(SIGINT, handler);
    signal(SIGTSTP, handler);
    signal(SIGCONT, handler);
    
    while (1)
    {
        sleep(1);
    }
    
    return 0;
}
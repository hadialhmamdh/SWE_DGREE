#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <linux/limits.h>
#include <fcntl.h>
#include <signal.h>
#include <sys/wait.h>
#include "LineParser.h"

#define MAX_INPUT 2048

int debug_mode = 0;  // Global flag for debug mode

void execute(cmdLine *pCmdLine) {
    if (strcmp(pCmdLine->arguments[0], "cd") == 0) {
        // Case: user typed simply "cd" with no arguments
        if (pCmdLine->argCount < 2) {
            fprintf(stderr, "cd: missing argument\n");
        }
        else {
            // Try to change directory
            if (chdir(pCmdLine->arguments[1]) == -1) {
                perror("cd failed");   // prints: "cd failed: <system error>"
            }
        }
        return; // IMPORTANT: do NOT fork/exec for "cd"
    }
    
    if (strcmp(pCmdLine->arguments[0], "zzzz") == 0 ||
        strcmp(pCmdLine->arguments[0], "kuku") == 0 ||
        strcmp(pCmdLine->arguments[0], "blast") == 0) {

        if (pCmdLine->argCount < 2) {
            fprintf(stderr, "%s: missing PID argument\n", pCmdLine->arguments[0]);
            return;
        }

        pid_t target = atoi(pCmdLine->arguments[1]);
        int sig;

        if (strcmp(pCmdLine->arguments[0], "zzzz") == 0) sig = SIGSTOP;
        else if (strcmp(pCmdLine->arguments[0], "kuku") == 0) sig = SIGCONT;
        else sig = SIGINT; // blast

        if (kill(target, sig) == -1) {
            perror("signal failed");
        }

        return; // no fork/exec for signal commands
    }

    pid_t pid = fork();   // Create a child process

    if (pid < 0) {
        perror("fork failed");
        exit(1);
    }

    if (pid == 0) {
        // --- CHILD PROCESS ---
        
        // DEBUG: Print PID and executing command
        if (debug_mode) {
            fprintf(stderr, "PID: %d\n", getpid());
            fprintf(stderr, "Executing command: %s\n", pCmdLine->arguments[0]);
        }
        
        // Input redirection: "< filename"
        if (pCmdLine->inputRedirect != NULL) {
            int inFd = open(pCmdLine->inputRedirect, O_RDONLY);
            if (inFd < 0) {
                /* cannot open input file -> error and exit child */
                perror("open inputRedirect failed");
                _exit(1);
            }
            /* replace stdin (fd 0) with inFd */
            if (dup2(inFd, STDIN_FILENO) == -1) {
                perror("dup2 input failed");
                close(inFd);
                _exit(1);
            }
            close(inFd); /* original fd not needed after dup2 */
        }

        /* Output redirection: "> filename" (create/truncate) */
        if (pCmdLine->outputRedirect != NULL) {
            int outFd = open(pCmdLine->outputRedirect,
                             O_CREAT | O_WRONLY | O_TRUNC,
                             0644); /* rw-r--r-- */
            if (outFd < 0) {
                perror("open outputRedirect failed");
                _exit(1);
            }
            /* replace stdout (fd 1) with outFd */
            if (dup2(outFd, STDOUT_FILENO) == -1) {
                perror("dup2 output failed");
                close(outFd);
                _exit(1);
            }
            close(outFd);
        }
        
        // Replace the child with the requested command
        execvp(pCmdLine->arguments[0], pCmdLine->arguments);

        // If execvp returns, it means it FAILED
        perror("execvp failed");
        _exit(1);
    }
    else {
        // --- PARENT PROCESS ---
        if (pCmdLine->blocking == 1) {
            // Foreground job → must WAIT
            waitpid(pid, NULL, 0);
        }
        else {
            // Background job → DO NOT WAIT
            // Optional but useful:
            printf("Started background process PID %d\n", pid);
        }
    }
}

void protectSpacesInQuotes(char *s) {
    int inQuote = 0;
    for (int i = 0; s[i]; i++) {
        if (s[i] == '"')
            inQuote = !inQuote;
        else if (s[i] == ' ' && inQuote)
            s[i] = 7;   // ASCII BEL as placeholder
    }
}

void restoreSpaces(cmdLine *cmd) {
    for (int i = 0; i < cmd->argCount; i++) {
        char *a = cmd->arguments[i];
        for (int j = 0; a[j]; j++)
            if (a[j] == 7)
                a[j] = ' ';
    }
}

int main(int argc, char *argv[]) {
    char cwd[PATH_MAX];
    char input[MAX_INPUT];
    
    // Check for -d flag
    if (argc > 1 && strcmp(argv[1], "-d") == 0) {
        debug_mode = 1;
    }
    
    while (1) {
        // 1. Display prompt (current directory)
        if (getcwd(cwd, PATH_MAX) == NULL) {
            perror("getcwd error");
            exit(1);
        }
        printf("%s> ", cwd);
        
        // 2. Read input
        if (fgets(input, MAX_INPUT, stdin) == NULL) {
            break;  // EOF
        }
        
        // 3. Parse input
        protectSpacesInQuotes(input);
        cmdLine *cmd = parseCmdLines(input);
        restoreSpaces(cmd);
        if (cmd == NULL) {
            continue;  // Empty line
        }
        
        // 4. Check for quit
        if (strcmp(cmd->arguments[0], "quit") == 0) {
            freeCmdLines(cmd);
            break;
        }
        
        // 5. Execute command
        execute(cmd);
        
        // 6. Clean up
        freeCmdLines(cmd);
    }
    
    return 0;
}
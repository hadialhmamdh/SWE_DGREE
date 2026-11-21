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
#define TERMINATED  -1
#define RUNNING 1
#define SUSPENDED 0
#define HISTLEN 10

int debug_mode = 0;  // Global flag for debug mode

// History circular queue
char *history[HISTLEN];
int history_count = 0;  // Total number of commands ever added
int history_start = 0;  // Index of oldest command
int history_size = 0;   // Current number of commands in history (0 to HISTLEN)

// Process list node structure
typedef struct process {
    cmdLine* cmd;
    pid_t pid;
    int status;
    struct process *next;
} process;

// Global process list
process *process_list = NULL;

// History management functions
void addToHistory(const char *cmdLine) {
    // Don't add recall commands (!!, !n) or empty lines to history
    if (cmdLine == NULL || strlen(cmdLine) == 0 || 
        cmdLine[0] == '\n' || strncmp(cmdLine, "!!", 2) == 0 || 
        (cmdLine[0] == '!' && cmdLine[1] >= '0' && cmdLine[1] <= '9')) {
        return;
    }
    
    // Remove trailing newline for storage
    char *clean_cmd = strdup(cmdLine);
    if (clean_cmd == NULL) return;
    
    size_t len = strlen(clean_cmd);
    if (len > 0 && clean_cmd[len-1] == '\n') {
        clean_cmd[len-1] = '\0';
    }
    
    // Calculate the index where we'll insert
    int insert_idx;
    
    if (history_size < HISTLEN) {
        // History not full yet
        insert_idx = history_size;
        history_size++;
    } else {
        // History full - overwrite oldest
        insert_idx = history_start;
        free(history[insert_idx]);  // Free the old command
        history_start = (history_start + 1) % HISTLEN;
    }
    
    history[insert_idx] = clean_cmd;
    history_count++;
}

void printHistory() {
    if (history_size == 0) {
        printf("No history available\n");
        return;
    }
    
    printf("History:\n");
    for (int i = 0; i < history_size; i++) {
        int idx = (history_start + i) % HISTLEN;
        int cmd_num = history_count - history_size + i + 1;
        printf("%d %s\n", cmd_num, history[idx]);
    }
}

char* getHistoryCommand(int n) {
    // Convert absolute command number to relative index
    int oldest_num = history_count - history_size + 1;
    int newest_num = history_count;
    
    if (n < oldest_num || n > newest_num) {
        return NULL;  // Out of range
    }
    
    int relative_idx = n - oldest_num;
    int actual_idx = (history_start + relative_idx) % HISTLEN;
    
    return history[actual_idx];
}

char* getLastCommand() {
    if (history_size == 0) {
        return NULL;
    }
    
    // Get the most recent non-history command
    int last_idx = (history_start + history_size - 1) % HISTLEN;
    return history[last_idx];
}

void freeHistory() {
    for (int i = 0; i < history_size; i++) {
        int idx = (history_start + i) % HISTLEN;
        free(history[idx]);
    }
}

// Function to get status string
const char* getStatusString(int status) {
    switch(status) {
        case RUNNING: return "Running";
        case SUSPENDED: return "Suspended";
        case TERMINATED: return "Terminated";
        default: return "Unknown";
    }
}

// Add a process to the process list
void addProcess(process** process_list, cmdLine* cmd, pid_t pid) {
    process *new_proc = (process*)malloc(sizeof(process));
    if (new_proc == NULL) {
        perror("malloc failed");
        return;
    }
    
    new_proc->cmd = cmd;
    new_proc->pid = pid;
    new_proc->status = RUNNING;
    new_proc->next = *process_list;
    *process_list = new_proc;
}

// Update process status
void updateProcessStatus(process* process_list, int pid, int status) {
    process *curr = process_list;
    while (curr != NULL) {
        if (curr->pid == pid) {
            curr->status = status;
            return;
        }
        curr = curr->next;
    }
}

// Update process list - check status of all processes
void updateProcessList(process **process_list) {
    process *curr = *process_list;
    int status;
    
    while (curr != NULL) {
        // Use WNOHANG to not block
        pid_t result = waitpid(curr->pid, &status, WNOHANG | WUNTRACED | WCONTINUED);
        
        if (result == -1) {
            // Process doesn't exist anymore
            curr->status = TERMINATED;
        }
        else if (result == curr->pid) {
            // Process status changed
            if (WIFEXITED(status) || WIFSIGNALED(status)) {
                // Process terminated
                curr->status = TERMINATED;
            }
            else if (WIFSTOPPED(status)) {
                // Process was stopped (SIGSTOP)
                curr->status = SUSPENDED;
            }
            else if (WIFCONTINUED(status)) {
                // Process was continued (SIGCONT)
                curr->status = RUNNING;
            }
        }
        // If result == 0, no status change
        
        curr = curr->next;
    }
}

// Delete terminated processes from the list
void deleteTerminatedProcesses(process **process_list) {
    process *curr = *process_list;
    process *prev = NULL;
    
    while (curr != NULL) {
        if (curr->status == TERMINATED) {
            process *to_delete = curr;
            
            if (prev == NULL) {
                // Delete first node
                *process_list = curr->next;
                curr = *process_list;
            } else {
                prev->next = curr->next;
                curr = curr->next;
            }
            
            // Free the command line
            freeCmdLines(to_delete->cmd);
            free(to_delete);
        } else {
            prev = curr;
            curr = curr->next;
        }
    }
}

// Print process list
void printProcessList(process** process_list) {
    // Update all process statuses first
    updateProcessList(process_list);
    
    printf("PID\t\tCommand\t\tSTATUS\n");
    
    process *curr = *process_list;
    while (curr != NULL) {
        // Print command with all arguments
        printf("%d\t\t", curr->pid);
        for (int i = 0; i < curr->cmd->argCount; i++) {
            printf("%s ", curr->cmd->arguments[i]);
        }
        printf("\t\t%s\n", getStatusString(curr->status));
        
        curr = curr->next;
    }
    
    // Delete terminated processes after printing
    deleteTerminatedProcesses(process_list);
}

// Free entire process list
void freeProcessList(process* process_list) {
    process *curr = process_list;
    while (curr != NULL) {
        process *next = curr->next;
        freeCmdLines(curr->cmd);
        free(curr);
        curr = next;
    }
}

void execute_single(cmdLine *pCmdLine) {
    // Input redirection: "< filename"
    if (pCmdLine->inputRedirect != NULL) {
        int inFd = open(pCmdLine->inputRedirect, O_RDONLY);
        if (inFd < 0) {
            perror("open inputRedirect failed");
            _exit(1);
        }
        if (dup2(inFd, STDIN_FILENO) == -1) {
            perror("dup2 input failed");
            close(inFd);
            _exit(1);
        }
        close(inFd);
    }

    // Output redirection: "> filename"
    if (pCmdLine->outputRedirect != NULL) {
        int outFd = open(pCmdLine->outputRedirect,
                         O_CREAT | O_WRONLY | O_TRUNC,
                         0644);
        if (outFd < 0) {
            perror("open outputRedirect failed");
            _exit(1);
        }
        if (dup2(outFd, STDOUT_FILENO) == -1) {
            perror("dup2 output failed");
            close(outFd);
            _exit(1);
        }
        close(outFd);
    }
    
    // Execute the command
    execvp(pCmdLine->arguments[0], pCmdLine->arguments);
    
    // If execvp returns, it failed
    perror("execvp failed");
    _exit(1);
}

void execute_pipe(cmdLine *pCmdLine) {
    // Validate: left side should NOT have output redirect
    if (pCmdLine->outputRedirect != NULL) {
        fprintf(stderr, "Error: Cannot redirect output of left side of pipe\n");
        return;
    }
    
    // Validate: right side should NOT have input redirect
    if (pCmdLine->next && pCmdLine->next->inputRedirect != NULL) {
        fprintf(stderr, "Error: Cannot redirect input of right side of pipe\n");
        return;
    }
    
    int pipefd[2];
    pid_t pid1, pid2;
    
    // Create the pipe
    if (pipe(pipefd) == -1) {
        perror("pipe failed");
        return;
    }
    
    // Fork first child (left side of pipe)
    if (debug_mode) {
        fprintf(stderr, "Forking for left side of pipe...\n");
    }
    
    pid1 = fork();
    if (pid1 < 0) {
        perror("fork failed");
        close(pipefd[0]);
        close(pipefd[1]);
        return;
    }
    
    if (pid1 == 0) {
        // CHILD 1: Left side of pipe
        if (debug_mode) {
            fprintf(stderr, "Child1 PID: %d, executing: %s\n", 
                    getpid(), pCmdLine->arguments[0]);
        }
        
        // Close stdout and duplicate write end of pipe
        close(STDOUT_FILENO);
        dup(pipefd[1]);
        close(pipefd[1]);
        close(pipefd[0]);  // Close read end in child1
        
        // Handle input redirection for left side (if any)
        if (pCmdLine->inputRedirect != NULL) {
            int inFd = open(pCmdLine->inputRedirect, O_RDONLY);
            if (inFd < 0) {
                perror("open inputRedirect failed");
                _exit(1);
            }
            if (dup2(inFd, STDIN_FILENO) == -1) {
                perror("dup2 input failed");
                close(inFd);
                _exit(1);
            }
            close(inFd);
        }
        
        // Execute left command
        execvp(pCmdLine->arguments[0], pCmdLine->arguments);
        perror("execvp failed");
        _exit(1);
    }
    
    // PARENT: Close write end of pipe
    if (debug_mode) {
        fprintf(stderr, "Parent: closing write end of pipe\n");
    }
    close(pipefd[1]);
    
    // Fork second child (right side of pipe)
    if (debug_mode) {
        fprintf(stderr, "Forking for right side of pipe...\n");
    }
    
    pid2 = fork();
    if (pid2 < 0) {
        perror("fork failed");
        close(pipefd[0]);
        return;
    }
    
    if (pid2 == 0) {
        // CHILD 2: Right side of pipe
        if (debug_mode) {
            fprintf(stderr, "Child2 PID: %d, executing: %s\n", 
                    getpid(), pCmdLine->next->arguments[0]);
        }
        
        // Close stdin and duplicate read end of pipe
        close(STDIN_FILENO);
        dup(pipefd[0]);
        close(pipefd[0]);
        
        // Handle output redirection for right side (if any)
        if (pCmdLine->next->outputRedirect != NULL) {
            int outFd = open(pCmdLine->next->outputRedirect,
                           O_CREAT | O_WRONLY | O_TRUNC,
                           0644);
            if (outFd < 0) {
                perror("open outputRedirect failed");
                _exit(1);
            }
            if (dup2(outFd, STDOUT_FILENO) == -1) {
                perror("dup2 output failed");
                close(outFd);
                _exit(1);
            }
            close(outFd);
        }
        
        // Execute right command
        execvp(pCmdLine->next->arguments[0], pCmdLine->next->arguments);
        perror("execvp failed");
        _exit(1);
    }
    
    // PARENT: Close read end of pipe
    if (debug_mode) {
        fprintf(stderr, "Parent: closing read end of pipe\n");
    }
    close(pipefd[0]);
    
    // Wait for both children
    if (debug_mode) {
        fprintf(stderr, "Parent: waiting for children to terminate\n");
    }
    waitpid(pid1, NULL, 0);
    waitpid(pid2, NULL, 0);
}

void execute(cmdLine *pCmdLine) {
    // Handle history command
    if (strcmp(pCmdLine->arguments[0], "history") == 0) {
        printHistory();
        return;
    }
    
    // Handle procs command
    if (strcmp(pCmdLine->arguments[0], "procs") == 0) {
        printProcessList(&process_list);
        return;
    }
    
    // Handle built-in commands
    if (strcmp(pCmdLine->arguments[0], "cd") == 0) {
        if (pCmdLine->argCount < 2) {
            fprintf(stderr, "cd: missing argument\n");
        }
        else {
            if (chdir(pCmdLine->arguments[1]) == -1) {
                perror("cd failed");
            }
        }
        return;
    }
    
    // Handle signal commands
    if (strcmp(pCmdLine->arguments[0], "zzzz") == 0 ||
        strcmp(pCmdLine->arguments[0], "kuku") == 0 ||
        strcmp(pCmdLine->arguments[0], "blast") == 0) {

        if (pCmdLine->argCount < 2) {
            fprintf(stderr, "%s: missing PID argument\n", pCmdLine->arguments[0]);
            return;
        }

        pid_t target = atoi(pCmdLine->arguments[1]);
        int sig;
        int new_status;

        if (strcmp(pCmdLine->arguments[0], "zzzz") == 0) {
            sig = SIGSTOP;
            new_status = SUSPENDED;
        }
        else if (strcmp(pCmdLine->arguments[0], "kuku") == 0) {
            sig = SIGCONT;
            new_status = RUNNING;
        }
        else { // blast
            sig = SIGINT;
            new_status = TERMINATED;
        }

        if (kill(target, sig) == -1) {
            perror("signal failed");
        } else {
            // Update the process status in our list
            updateProcessStatus(process_list, target, new_status);
        }

        return;
    }

    // Check if this is a pipe command
    if (pCmdLine->next != NULL) {
        execute_pipe(pCmdLine);
        return;
    }

    // Regular command (no pipe)
    pid_t pid = fork();

    if (pid < 0) {
        perror("fork failed");
        exit(1);
    }

    if (pid == 0) {
        // CHILD PROCESS
        if (debug_mode) {
            fprintf(stderr, "PID: %d\n", getpid());
            fprintf(stderr, "Executing command: %s\n", pCmdLine->arguments[0]);
        }
        
        execute_single(pCmdLine);
    }
    else {
        // PARENT PROCESS
        // Add process to list - make a copy of cmdLine
        // We need to NOT free pCmdLine in main if we add it to the list
        addProcess(&process_list, pCmdLine, pid);
        
        if (pCmdLine->blocking == 1) {
            waitpid(pid, NULL, 0);
            // Update status after waiting
            updateProcessStatus(process_list, pid, TERMINATED);
        }
        else {
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
    cmdLine *current = cmd;
    while (current != NULL) {
        for (int i = 0; i < current->argCount; i++) {
            char *a = current->arguments[i];
            for (int j = 0; a[j]; j++)
                if (a[j] == 7)
                    a[j] = ' ';
        }
        current = current->next;
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
        
        // Store original input for history
        char *original_input = strdup(input);
        
        // ========================================================
        // CHECK FOR HISTORY RECALL COMMANDS BEFORE PARSING
        // This is where !! and !n are detected and processed
        // ========================================================
        
        if (strncmp(input, "!!", 2) == 0) {
            // !! - Execute last command
            char *last_cmd = getLastCommand();
            if (last_cmd == NULL) {
                fprintf(stderr, "Error: No commands in history\n");
                free(original_input);
                continue;
            }
            
            // Print the command being executed
            printf("%s\n", last_cmd);
            
            // REPLACE input buffer with the retrieved command
            // This makes it as if the user typed that command
            snprintf(input, MAX_INPUT, "%s\n", last_cmd);
            free(original_input);
            original_input = strdup(input);
        }
        else if (input[0] == '!' && input[1] >= '0' && input[1] <= '9') {
            // !n - Execute command at index n
            int n = atoi(input + 1);
            char *hist_cmd = getHistoryCommand(n);
            
            if (hist_cmd == NULL) {
                fprintf(stderr, "Error: No such command in history: %d\n", n);
                free(original_input);
                continue;
            }
            
            // Print the command being executed
            printf("%s\n", hist_cmd);
            
            // REPLACE input buffer with the retrieved command
            // This makes it as if the user typed that command
            snprintf(input, MAX_INPUT, "%s\n", hist_cmd);
            free(original_input);
            original_input = strdup(input);
        }
        
        // ========================================================
        // After the above checks, 'input' now contains either:
        // - The original command (if not !! or !n)
        // - The retrieved command from history (if !! or !n)
        // ========================================================
        
        // 3. Parse input
        protectSpacesInQuotes(input);
        cmdLine *cmd = parseCmdLines(input);
        if (cmd == NULL) {
            free(original_input);
            continue;  // Empty line
        }
        restoreSpaces(cmd);
        
        // 4. Check for quit
        if (strcmp(cmd->arguments[0], "quit") == 0) {
            freeCmdLines(cmd);
            free(original_input);
            break;
        }
        
        // Add to history AFTER parsing succeeds
        // Exclude only !! and !n (recall commands)
        addToHistory(original_input);
        free(original_input);
        
        // 5. Execute command
        execute(cmd);
        
        // 6. Clean up - but NOT if we added it to process list!
        // Only free if it's a built-in command or pipe
        if (strcmp(cmd->arguments[0], "history") == 0 ||
            strcmp(cmd->arguments[0], "procs") == 0 ||
            strcmp(cmd->arguments[0], "cd") == 0 ||
            strcmp(cmd->arguments[0], "zzzz") == 0 ||
            strcmp(cmd->arguments[0], "kuku") == 0 ||
            strcmp(cmd->arguments[0], "blast") == 0 ||
            cmd->next != NULL) {
            freeCmdLines(cmd);
        }
        // For regular commands, cmd is now owned by process_list
    }
    
    // Clean up process list and history before exiting
    freeProcessList(process_list);
    freeHistory();
    
    return 0;
}
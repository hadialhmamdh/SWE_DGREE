#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/wait.h>

int main() {
    int pipefd[2];
    pid_t child1_pid, child2_pid;
    
    // Step 1: Create a pipe
    if (pipe(pipefd) == -1) {
        perror("pipe");
        exit(EXIT_FAILURE);
    }
    
    // Step 2: Fork first child process
    fprintf(stderr, "(parent_process>forking...)\n");
    child1_pid = fork();
    
    if (child1_pid == -1) {
        perror("fork");
        exit(EXIT_FAILURE);
    }
    
    if (child1_pid == 0) {
        // Step 3: In child1 process
        fprintf(stderr, "(child1>redirecting stdout to the write end of the pipe...)\n");
        
        // 3.1: Close standard output
        close(STDOUT_FILENO);
        
        // 3.2: Duplicate the write-end of the pipe
        dup(pipefd[1]);
        
        // 3.3: Close the file descriptor that was duplicated
        close(pipefd[1]);
        
        // Also close the read end in child1
        close(pipefd[0]);
        
        // 3.4: Execute "ls -lsa"
        fprintf(stderr, "(child1>going to execute cmd: ls -lsa)\n");
        char *args[] = {"ls", "-lsa", NULL};
        execvp("ls", args);
        
        // If execvp returns, it failed
        perror("execvp ls");
        exit(EXIT_FAILURE);
    }
    
    // Step 4: In parent process - close write end of pipe
    fprintf(stderr, "(parent_process>created process with id: %d)\n", child1_pid);
    fprintf(stderr, "(parent_process>closing the write end of the pipe...)\n");
    close(pipefd[1]);
    
    // Step 5: Fork second child process
    fprintf(stderr, "(parent_process>forking...)\n");
    child2_pid = fork();
    
    if (child2_pid == -1) {
        perror("fork");
        exit(EXIT_FAILURE);
    }
    
    if (child2_pid == 0) {
        // Step 6: In child2 process
        fprintf(stderr, "(child2>redirecting stdin to the read end of the pipe...)\n");
        
        // 6.1: Close standard input
        close(STDIN_FILENO);
        
        // 6.2: Duplicate the read-end of the pipe
        dup(pipefd[0]);
        
        // 6.3: Close the file descriptor that was duplicated
        close(pipefd[0]);
        
        // 6.4: Execute "tail -n 3"
        fprintf(stderr, "(child2>going to execute cmd: tail -n 3)\n");
        char *args[] = {"tail", "-n", "3", NULL};
        execvp("tail", args);
        
        // If execvp returns, it failed
        perror("execvp tail");
        exit(EXIT_FAILURE);
    }
    
    // Step 7: In parent process - close read end of pipe
    fprintf(stderr, "(parent_process>created process with id: %d)\n", child2_pid);
    fprintf(stderr, "(parent_process>closing the read end of the pipe...)\n");
    close(pipefd[0]);
    
    // Step 8: Wait for child processes to terminate
    fprintf(stderr, "(parent_process>waiting for child processes to terminate...)\n");
    waitpid(child1_pid, NULL, 0);
    waitpid(child2_pid, NULL, 0);
    
    fprintf(stderr, "(parent_process>exiting...)\n");
    
    return 0;
}
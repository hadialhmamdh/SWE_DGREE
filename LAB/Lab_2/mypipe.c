#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/wait.h>

int main(int argc, char *argv[]) {
    int fd[2];  // fd[0] = read end, fd[1] = write end
    pid_t pid;
    char buffer[1024];
    
    // Check if message argument is provided
    if (argc != 2){
        fprintf(stderr, "Usage: %s <message>\n", argv[0]);
        exit(1);
    }
    
    // Create the pipe
    if (pipe(fd) == -1) {
        perror("pipe");
        exit(1);
    }
    
    // Fork a child process
    pid = fork();
    
    if (pid == -1) {
        perror("fork");
        exit(1);
    }
    
    if (pid == 0) {
        // CHILD PROCESS: Write message to pipe
        close(fd[0]);  // Close unused read end
        
        // Write argv[1] to the pipe
        write(fd[1], argv[1], strlen(argv[1]) + 1);  // +1 for null terminator
        close(fd[1]);  // Close write end after writing
        exit(0);       // Child terminates
        
    } else {
        // PARENT PROCESS: Read message from pipe
        close(fd[1]);  // Close unused write end
        
        // Read message from pipe
        read(fd[0], buffer, sizeof(buffer));
        
        // Print the incoming message
        printf("%s\n", buffer);
        
        close(fd[0]);  // Close read end after reading
        wait(NULL);    // Wait for child to finish
    }
    
    return 0;
}
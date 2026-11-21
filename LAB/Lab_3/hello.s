section .data
    msg db 'hello world', 10    ; 10 is ASCII for newline (\n)
    msg_len equ  $ - msg          ; Calculate length of message

section .text
    global _start

_start:
    ; Write system call
    ; syscall number 4 = sys_write
    ; ebx = file descriptor (1 = stdout)
    ; ecx = pointer to message
    ; edx = message length
    
    mov eax, 4          ; System call number (sys_write)
    mov ebx, 1          ; File descriptor (stdout)
    mov ecx, msg        ; Pointer to message
    mov edx, msg_len    ; Message length
    int 0x80            ; Make system call
    
    ; Exit system call
    ; syscall number 1 = sys_exit
    ; ebx = exit code
    
    mov eax, 1          ; System call number (sys_exit)
    mov ebx, 0          ; Exit code (0 = success)
    int 0x80            ; Make system call
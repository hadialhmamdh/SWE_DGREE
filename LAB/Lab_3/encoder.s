section .data
    Infile  dd 0            ; File descriptor for input (stdin by default)
    Outfile dd 1            ; File descriptor for output (stdout by default)
    newline db 10           ; Newline character

section .bss
    buffer resb 1           ; 1-byte buffer for reading characters

section .text
    global main
    extern strlen
    extern system_call

main:
    push ebp
    mov ebp, esp
    
    ; Get argc and argv from stack
    ; [ebp+8] = argc
    ; [ebp+12] = argv
    ; [ebp+16] = envp
    
    mov ecx, [ebp+8]        ; ecx = argc
    mov esi, [ebp+12]       ; esi = argv
    
    ; Task 1.A: Print all arguments to stdout
    xor edi, edi            ; i = 0
    
print_args_loop:
    cmp edi, ecx            ; if i >= argc
    jge parse_args_start
    
    ; Get argv[i]
    mov ebx, [esi + edi*4]  ; ebx = argv[i]
    
    ; Call strlen to get length of argv[i]
    push ecx                ; Save ecx (argc)
    push esi                ; Save esi (argv)
    push edi                ; Save edi (counter)
    
    push ebx                ; Push argument for strlen
    call strlen
    add esp, 4              ; Clean up stack
    
    pop edi                 ; Restore edi
    pop esi                 ; Restore esi
    pop ecx                 ; Restore ecx
    
    ; Now eax has the length, ebx still has argv[i]
    ; Write to stdout using system_call
    push ecx                ; Save ecx
    push esi                ; Save esi
    push edi                ; Save edi
    
    push eax                ; arg4: length
    push ebx                ; arg3: buffer (argv[i])
    push dword 1            ; arg2: stdout
    push dword 4            ; arg1: sys_write
    call system_call
    add esp, 16             ; Clean up 4 arguments
    
    pop edi                 ; Restore edi
    pop esi                 ; Restore esi
    pop ecx                 ; Restore ecx
    
    ; Write newline
    push ecx
    push esi
    push edi
    
    push dword 1            ; arg4: length = 1
    push dword newline      ; arg3: buffer
    push dword 1            ; arg2: stdout
    push dword 4            ; arg1: sys_write
    call system_call
    add esp, 16
    
    pop edi
    pop esi
    pop ecx
    
    inc edi                 ; i++
    jmp print_args_loop

parse_args_start:
    ; Task 1.C: Parse command-line arguments
    mov ecx, [ebp+8]        ; argc
    mov esi, [ebp+12]       ; argv
    xor edi, edi            ; i = 0
    
parse_args_loop:
    cmp edi, ecx
    jge done_parsing
    
    mov eax, [esi + edi*4]  ; eax = argv[i]
    
    ; Check for -i flag
    cmp byte [eax], '-'
    jne next_arg
    cmp byte [eax+1], 'i'
    jne check_output_flag
    
    ; Open input file: -i{filename}
    lea ebx, [eax+2]        ; ebx = pointer to filename
    
    push ecx
    push esi
    push edi
    
    push dword 0            ; arg4: mode
    push dword 0            ; arg3: O_RDONLY
    push ebx                ; arg2: filename
    push dword 5            ; arg1: sys_open
    call system_call
    add esp, 16
    
    pop edi
    pop esi
    pop ecx
    
    cmp eax, 0
    jl exit_error
    
    mov [Infile], eax       ; Save file descriptor
    jmp next_arg

check_output_flag:
    mov eax, [esi + edi*4]  ; Reload argv[i]
    cmp byte [eax], '-'
    jne next_arg
    cmp byte [eax+1], 'o'
    jne next_arg
    
    ; Open output file: -o{filename}
    lea ebx, [eax+2]        ; ebx = pointer to filename
    
    push ecx
    push esi
    push edi
    
    push dword 0644o        ; arg4: mode (octal 644)
    push dword 577          ; arg3: O_WRONLY|O_CREAT|O_TRUNC
    push ebx                ; arg2: filename
    push dword 5            ; arg1: sys_open
    call system_call
    add esp, 16
    
    pop edi
    pop esi
    pop ecx
    
    cmp eax, 0
    jl exit_error
    
    mov [Outfile], eax      ; Save file descriptor

next_arg:
    inc edi
    jmp parse_args_loop

done_parsing:
    ; Task 1.B: Encode from input to output
    call encode
    
    ; Close input file if not stdin
    mov ebx, [Infile]
    cmp ebx, 0
    je skip_close_in
    
    push dword 0
    push dword 0
    push ebx
    push dword 6            ; sys_close
    call system_call
    add esp, 16

skip_close_in:
    ; Close output file if not stdout
    mov ebx, [Outfile]
    cmp ebx, 1
    je skip_close_out
    
    push dword 0
    push dword 0
    push ebx
    push dword 6            ; sys_close
    call system_call
    add esp, 16

skip_close_out:
    ; Return 0
    xor eax, eax
    mov esp, ebp
    pop ebp
    ret

exit_error:
    ; Return 1
    mov eax, 1
    mov esp, ebp
    pop ebp
    ret

encode:
    push ebp
    mov ebp, esp
    
encode_loop:
    ; Read one character
    push dword 1            ; arg4: count = 1
    push dword buffer       ; arg3: buffer
    push dword [Infile]     ; arg2: file descriptor
    push dword 3            ; arg1: sys_read
    call system_call
    add esp, 16
    
    ; Check return value
    cmp eax, 0
    jle encode_done         ; EOF or error
    
    ; Get character from buffer
    mov al, [buffer]
    
    ; Check if 'A' <= char <= 'Z'
    cmp al, 'A'
    jl write_char
    cmp al, 'Z'
    jg write_char
    
    ; Encode: add 3
    add al, 3
    
    ; Check for wrap-around
    cmp al, 'Z'
    jle store_char
    sub al, 26              ; Wrap back to beginning
    
store_char:
    mov [buffer], al

write_char:
    ; Write the character
    push dword 1            ; arg4: count = 1
    push dword buffer       ; arg3: buffer
    push dword [Outfile]    ; arg2: file descriptor
    push dword 4            ; arg1: sys_write
    call system_call
    add esp, 16
    
    jmp encode_loop

encode_done:
    mov esp, ebp
    pop ebp
    ret
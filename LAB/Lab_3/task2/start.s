section .text
global _start
global system_call
global infection
global infector
global code_start
global code_end

extern main

; ============================
; program entry (provided)
; ============================
_start:
    pop    dword ecx
    mov    esi,esp
    mov     eax,ecx
    shl     eax,2
    add     eax,esi
    add     eax,4
    push    dword eax
    push    dword esi
    push    dword ecx
    call    main
    mov     ebx,eax
    mov     eax,1
    int     0x80

; ============================
; system_call (provided)
; ============================
system_call:
    push    ebp
    mov     ebp,esp
    sub     esp,4
    pushad
    mov     eax,[ebp+8]
    mov     ebx,[ebp+12]
    mov     ecx,[ebp+16]
    mov     edx,[ebp+20]
    int     0x80
    mov     [ebp-4],eax
    popad
    mov     eax,[ebp-4]
    add     esp,4
    pop     ebp
    ret

code_start:

; --------------------------
; void infection()
; --------------------------
infection:
    push ebp
    mov ebp, esp
    
    ; Use position-independent code - get current address
    call .get_msg
    db "Hello, Infected File", 10
.get_msg:
    pop ecx             ; ecx now points to the message
    
    mov eax, 4          ; sys_write
    mov ebx, 1          ; stdout
    ; ecx already has pointer to message
    mov edx, 21         ; message length (20 chars + newline)
    int 0x80
    
    mov esp, ebp
    pop ebp
    ret


; --------------------------
; void infector(char* filename)
; --------------------------
infector:
    push ebp
    mov ebp, esp
    push ebx
    push edi
    push esi

    ; open(filename, O_WRONLY|O_APPEND, 0644)
    mov eax, 5              ; sys_open
    mov ebx, [ebp+8]        ; filename
    mov ecx, 0x401          ; O_WRONLY (1) | O_APPEND (0x400)
    mov edx, 0644           ; permissions (octal)
    int 0x80

    test eax, eax           ; check if fd < 0
    js .done                ; if negative, skip
    mov edi, eax            ; save fd

    ; Get address of code_start using position-independent code
    call .get_code_addr
.get_code_addr:
    pop esi
    ; Calculate offset from current position to code_start
    ; esi now points to instruction after call
    ; We need to adjust it to point to code_start
    sub esi, (.get_code_addr - code_start)
    
    ; write(fd, code_start, code_end - code_start)
    mov eax, 4              ; sys_write
    mov ebx, edi            ; fd
    mov ecx, esi            ; address of code_start (calculated)
    mov edx, (code_end - code_start)  ; size (compile-time constant)
    int 0x80

    ; close(fd)
    mov eax, 6              ; sys_close
    mov ebx, edi            ; fd
    int 0x80

.done:
    pop esi
    pop edi
    pop ebx
    mov esp, ebp
    pop ebp
    ret

code_end:
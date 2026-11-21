global main
global print_multi
global getmulti
global add_multi
global get_max_min
extern printf
extern putchar
extern fgets
extern stdin
extern strlen
extern malloc
extern strcmp

section .data
    ; LFSR constants for PRNG
    STATE: dw 0xACE1          ; Initial seed (must not be 0)
    MASK: dw 0xB400           ; Taps at positions 16,15,13,4 (Fibonacci LFSR)
    
    ; Format strings
    fmt: db "%02hhx", 0
    
    ; Input buffer
    buffer: times 600 db 0
    result_struct: dd 0
                   times 300 db 0
    
    ; Predefined test numbers
    x_struct: dd 5
    x_num: db 0xaa, 1, 2, 0x44, 0x4f
    
    y_struct: dd 6
    y_num: db 0xaa, 1, 2, 3, 0x44, 0x4f
    
    ; Command line flags
    flag_I: db "-I", 0
    flag_R: db "-R", 0

section .bss
    carry_flag: resb 1

section .text

; ============================================
; main: Entry point
; ============================================
main:
    push ebp
    mov ebp, esp
    push ebx
    push esi
    push edi
    
    ; Get argc and argv
    mov ecx, [ebp + 8]      ; argc
    mov esi, [ebp + 12]     ; argv
    
    ; Check for command line arguments
    cmp ecx, 1
    je default_mode         ; No arguments - use default
    
    ; Check for -I flag
    mov eax, [esi + 4]      ; argv[1]
    push eax
    push flag_I
    call strcmp
    add esp, 8
    test eax, eax
    jz interactive_mode
    
    ; Check for -R flag
    mov eax, [esi + 4]
    push eax
    push flag_R
    call strcmp
    add esp, 8
    test eax, eax
    jz random_mode
    
default_mode:
    ; Use predefined x_struct and y_struct
    push x_struct
    call print_multi
    add esp, 4
    
    push y_struct
    call print_multi
    add esp, 4
    
    push y_struct
    push x_struct
    call add_multi
    add esp, 8
    
    test eax, eax
    jz exit_error
    
    push eax
    call print_multi
    add esp, 4
    
    jmp exit_success

interactive_mode:
    ; Read two numbers from stdin
    call getmulti
    test eax, eax
    jz exit_error
    mov ebx, eax            ; Save first number
    
    call getmulti
    test eax, eax
    jz exit_error
    mov edi, eax            ; Save second number
    
    ; Print first number
    push ebx
    call print_multi
    add esp, 4
    
    ; Print second number
    push edi
    call print_multi
    add esp, 4
    
    ; Add them
    push edi
    push ebx
    call add_multi
    add esp, 8
    
    test eax, eax
    jz exit_error
    
    push eax
    call print_multi
    add esp, 4
    
    jmp exit_success

random_mode:
    ; Generate two random numbers
    call PRmulti
    test eax, eax
    jz exit_error
    mov ebx, eax            ; Save first number
    
    push ebx
    call print_multi
    add esp, 4
    
    call PRmulti
    test eax, eax
    jz exit_error
    mov edi, eax            ; Save second number
    
    push edi
    call print_multi
    add esp, 4
    
    ; Add them
    push edi
    push ebx
    call add_multi
    add esp, 8
    
    test eax, eax
    jz exit_error
    
    push eax
    call print_multi
    add esp, 4
    
    jmp exit_success

exit_success:
    xor eax, eax
    pop edi
    pop esi
    pop ebx
    pop ebp
    ret

exit_error:
    mov eax, 1
    pop edi
    pop esi
    pop ebx
    pop ebp
    ret

; ============================================
; rand_num: Generate one random bit using LFSR
; Returns: random bit in AL (0 or 1)
; ============================================
rand_num:
    push ebx
    push ecx
    
    ; Load STATE
    movzx eax, word [STATE]
    
    ; Apply mask to get relevant bits
    mov bx, ax              ; Save original STATE
    and ax, [MASK]          ; Get tapped bits
    
    ; Compute parity by XOR reduction
    mov cx, ax
    shr cx, 8
    xor al, cl              ; XOR high and low bytes
    
    mov cl, al
    shr cl, 4
    xor al, cl              ; XOR nibbles
    
    mov cl, al
    shr cl, 2
    xor al, cl              ; XOR pairs
    
    mov cl, al
    shr cl, 1
    xor al, cl              ; XOR final bits
    
    and al, 1               ; Keep only LSB (parity bit)
    
    ; Shift STATE right and insert parity at MSB
    shr bx, 1               ; Shift right
    
    test al, 1
    jz parity_zero
    or bx, 0x8000           ; Set MSB if parity is 1
    
parity_zero:
    mov [STATE], bx         ; Update STATE
    
    pop ecx
    pop ebx
    ret

; ============================================
; rand_byte: Generate 8 random bits
; Returns: random byte in AL
; ============================================
rand_byte:
    push ebx
    push ecx
    
    xor ebx, ebx            ; Result accumulator
    mov ecx, 8              ; Generate 8 bits
    
rand_byte_loop:
    call rand_num           ; Get one bit in AL
    shl ebx, 1              ; Shift result left
    or bl, al               ; Insert new bit
    loop rand_byte_loop
    
    mov al, bl              ; Return byte in AL
    
    pop ecx
    pop ebx
    ret

; ============================================
; PRmulti: Generate random multi-precision integer
; Returns: pointer to allocated struct in EAX
; ============================================
PRmulti:
    push ebp
    mov ebp, esp
    push ebx
    push ecx
    push edi
    
get_nonzero_length:
    call rand_byte          ; Get random length
    test al, al
    jz get_nonzero_length   ; If zero, try again
    
    movzx ecx, al           ; ECX = length in bytes
    
    ; Allocate memory: 4 + length bytes
    mov eax, ecx
    add eax, 4
    push ecx                ; Save length
    push eax
    call malloc
    add esp, 4
    pop ecx                 ; Restore length
    
    test eax, eax
    jz prmulti_error
    
    ; Store size
    mov [eax], ecx
    
    ; Generate random bytes
    lea edi, [eax + 4]      ; EDI = num array
    xor ebx, ebx            ; Index
    
prmulti_loop:
    cmp ebx, ecx
    jge prmulti_done
    
    push eax                ; Save struct pointer
    call rand_byte
    pop edx                 ; Restore struct pointer
    mov [edi + ebx], al
    inc ebx
    mov eax, edx            ; Restore for next iteration
    jmp prmulti_loop
    
prmulti_done:
    pop edi
    pop ecx
    pop ebx
    pop ebp
    ret

prmulti_error:
    xor eax, eax
    pop edi
    pop ecx
    pop ebx
    pop ebp
    ret

; ============================================
; get_max_min: Compare two multi structs
; Input: eax = pointer to first struct, ebx = pointer to second struct
; Output: eax = pointer to larger struct, ebx = pointer to smaller struct
; ============================================
get_max_min:
    push ecx
    push edx
    
    mov ecx, [eax]      ; ecx = size of first struct
    mov edx, [ebx]      ; edx = size of second struct
    
    cmp ecx, edx
    jge first_is_max
    
    ; Swap: second is larger
    mov ecx, eax
    mov eax, ebx
    mov ebx, ecx
    
first_is_max:
    pop edx
    pop ecx
    ret

; ============================================
; add_multi: Add two multi-precision integers
; C calling convention: add_multi(struct multi *p, struct multi *q)
; Returns: pointer to newly allocated result struct
; ============================================
add_multi:
    push ebp
    mov ebp, esp
    sub esp, 16             ; Local vars: [ebp-4]=result, [ebp-8]=max_len, [ebp-12]=min_len, [ebp-16]=max_ptr
    push ebx
    push esi
    push edi
    
    ; Get arguments
    mov eax, [ebp + 8]      ; First struct
    mov ebx, [ebp + 12]     ; Second struct
    
    ; Determine max and min
    call get_max_min
    ; eax = max struct, ebx = min struct
    
    ; Get sizes
    mov esi, [eax]          ; max_len
    mov edi, [ebx]          ; min_len
    
    mov [ebp - 8], esi      ; Save max_len
    mov [ebp - 12], edi     ; Save min_len
    mov [ebp - 16], eax     ; Save max pointer
    
    ; Allocate result struct: 4 + (max_len + 1) bytes
    mov eax, esi
    inc eax
    add eax, 4
    push eax
    call malloc
    add esp, 4
    
    test eax, eax
    jz add_malloc_error
    
    mov [ebp - 4], eax      ; Save result pointer
    
    ; Set result size
    mov esi, [ebp - 8]      ; max_len
    inc esi
    mov [eax], esi          ; result->size = max_len + 1
    
    ; Setup pointers to arrays
    mov esi, [ebp - 16]     ; max struct
    lea esi, [esi + 4]      ; esi = max->num
    lea ebx, [ebx + 4]      ; ebx = min->num
    mov edi, [ebp - 4]      ; result struct
    lea edi, [edi + 4]      ; edi = result->num
    
    ; Initialize loop
    xor ecx, ecx            ; i = 0
    mov byte [carry_flag], 0
    
add_both_loop:
    ; Phase 1: i < min_len
    mov edx, [ebp - 12]     ; min_len
    cmp ecx, edx
    jge add_max_only_loop
    
    ; Add max[i] + min[i] + carry
    movzx eax, byte [esi + ecx]
    movzx edx, byte [ebx + ecx]
    add eax, edx
    movzx edx, byte [carry_flag]
    add eax, edx
    
    ; Store result
    mov [edi + ecx], al
    
    ; Calculate carry
    mov byte [carry_flag], 0
    cmp eax, 255
    jbe add_no_carry1
    mov byte [carry_flag], 1
    
add_no_carry1:
    inc ecx
    jmp add_both_loop
    
add_max_only_loop:
    ; Phase 2: min_len <= i < max_len
    mov edx, [ebp - 8]      ; max_len
    cmp ecx, edx
    jge add_final_carry
    
    ; Add max[i] + carry
    movzx eax, byte [esi + ecx]
    movzx edx, byte [carry_flag]
    add eax, edx
    
    ; Store result
    mov [edi + ecx], al
    
    ; Calculate carry
    mov byte [carry_flag], 0
    cmp eax, 255
    jbe add_no_carry2
    mov byte [carry_flag], 1
    
add_no_carry2:
    inc ecx
    jmp add_max_only_loop
    
add_final_carry:
    ; Phase 3: Store final carry
    movzx eax, byte [carry_flag]
    mov [edi + ecx], al
    
    ; Return result
    mov eax, [ebp - 4]
    
    pop edi
    pop esi
    pop ebx
    mov esp, ebp
    pop ebp
    ret

add_malloc_error:
    xor eax, eax
    pop edi
    pop esi
    pop ebx
    mov esp, ebp
    pop ebp
    ret

; ============================================
; print_multi
; ============================================
print_multi:
    push ebp
    mov ebp, esp
    push ebx
    push ecx
    push edx
    push esi
    
    mov esi, [ebp + 8]
    mov ecx, [esi]
    add esi, 4
    mov ebx, ecx
    dec ebx
    
print_loop:
    cmp ebx, 0
    jl print_newline
    
    movzx eax, byte [esi + ebx]
    push eax
    push fmt
    call printf
    add esp, 8
    
    dec ebx
    jmp print_loop
    
print_newline:
    push 10
    call putchar
    add esp, 4
    
    pop esi
    pop edx
    pop ecx
    pop ebx
    pop ebp
    ret

; ============================================
; getmulti
; ============================================
getmulti:
    push ebp
    mov ebp, esp
    push ebx
    push ecx
    push edx
    push esi
    push edi
    
    mov eax, [stdin]
    push eax
    push 600
    push buffer
    call fgets
    add esp, 12
    
    test eax, eax
    jz read_error
    
    push buffer
    call strlen
    add esp, 4
    mov ecx, eax
    
    cmp ecx, 0
    je read_error
    cmp byte [buffer + ecx - 1], 10
    jne no_newline
    dec ecx
    mov byte [buffer + ecx], 0
    
no_newline:
    cmp ecx, 0
    je read_error
    
    test ecx, 1
    jz even_length
    
    mov esi, ecx
    
shift_loop:
    cmp esi, 0
    jle shift_done
    mov al, [buffer + esi - 1]
    mov [buffer + esi], al
    dec esi
    jmp shift_loop
    
shift_done:
    mov byte [buffer], '0'
    inc ecx
    
even_length:
    shr ecx, 1              ; ecx = size in bytes
    
    ; Allocate memory for result struct
    push ecx                ; Save size
    mov eax, ecx
    add eax, 4              ; Size + 4 bytes for size field
    push eax
    call malloc
    add esp, 4
    pop ecx                 ; Restore size
    
    test eax, eax
    jz read_error
    
    ; Store size
    mov [eax], ecx
    
    ; Copy data to allocated struct
    lea edi, [eax + 4]      ; edi = destination (allocated struct num array)
    
    mov ebx, 0
    mov esi, ecx
    dec esi
    
convert_loop:
    cmp esi, 0
    jl conversion_done
    
    mov edx, esi
    shl edx, 1
    
    push eax                ; Save allocated struct pointer
    movzx eax, byte [buffer + edx]
    call hex_char_to_nibble
    shl eax, 4
    push eax
    
    mov eax, esi
    shl eax, 1
    inc eax
    movzx eax, byte [buffer + eax]
    call hex_char_to_nibble
    
    pop edx
    or eax, edx
    
    mov [edi + ebx], al
    pop eax                 ; Restore allocated struct pointer
    
    inc ebx
    dec esi
    jmp convert_loop
    
conversion_done:
    ; eax already contains the allocated struct pointer
    
    pop edi
    pop esi
    pop edx
    pop ecx
    pop ebx
    pop ebp
    ret

read_error:
    xor eax, eax
    pop edi
    pop esi
    pop edx
    pop ecx
    pop ebx
    pop ebp
    ret

hex_char_to_nibble:
    cmp al, '9'
    jle is_digit
    
    or al, 0x20
    sub al, 'a'
    add al, 10
    ret
    
is_digit:
    sub al, '0'
    ret
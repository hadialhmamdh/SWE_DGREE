#include <stdio.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>
#include <elf.h>
#include <string.h>

/* External function from startup.o */
extern int startup(int argc, char **argv, void (*start)());

/* Function to convert segment type to string */
const char* get_segment_type(Elf32_Word p_type) {
    switch(p_type) {
        case PT_NULL:    return "NULL";
        case PT_LOAD:    return "LOAD";
        case PT_DYNAMIC: return "DYNAMIC";
        case PT_INTERP:  return "INTERP";
        case PT_NOTE:    return "NOTE";
        case PT_SHLIB:   return "SHLIB";
        case PT_PHDR:    return "PHDR";
        case PT_TLS:     return "TLS";
        case PT_GNU_EH_FRAME: return "GNU_EH_FRAME";
        case PT_GNU_STACK:    return "GNU_STACK";
        case PT_GNU_RELRO:    return "GNU_RELRO";
        default:         return "UNKNOWN";
    }
}

/* Function to get protection flags for mmap from ELF flags */
int get_prot_flags(Elf32_Word p_flags) {
    int prot = 0;
    
    if (p_flags & PF_R) prot |= PROT_READ;
    if (p_flags & PF_W) prot |= PROT_WRITE;
    if (p_flags & PF_X) prot |= PROT_EXEC;
    
    return prot;
}

/* Function to convert protection flags to string */
void prot_flags_to_string(int prot, char *buf) {
    buf[0] = (prot & PROT_READ)  ? 'R' : '-';
    buf[1] = (prot & PROT_WRITE) ? 'W' : '-';
    buf[2] = (prot & PROT_EXEC)  ? 'X' : '-';
    buf[3] = '\0';
}

/* Function to convert ELF flags to readable format */
void flags_to_string(Elf32_Word p_flags, char *buf) {
    buf[0] = (p_flags & PF_R) ? 'R' : ' ';
    buf[1] = (p_flags & PF_W) ? 'W' : ' ';
    buf[2] = (p_flags & PF_X) ? 'E' : ' ';
    buf[3] = '\0';
}

/* Task 1a: Print detailed program header information */
void print_phdr_details(Elf32_Phdr *phdr, int index) {
    char flags[4];
    flags_to_string(phdr->p_flags, flags);
    
    printf("%-14s 0x%06x 0x%08x 0x%08x 0x%05x 0x%05x %-3s 0x%-4x\n",
           get_segment_type(phdr->p_type),
           phdr->p_offset,
           phdr->p_vaddr,
           phdr->p_paddr,
           phdr->p_filesz,
           phdr->p_memsz,
           flags,
           phdr->p_align);
}

/* Task 1b: Print program header with mmap flags */
void print_phdr_with_mmap_flags(Elf32_Phdr *phdr, int index) {
    char flags[4];
    char prot_str[4];
    int prot;
    
    flags_to_string(phdr->p_flags, flags);
    
    /* Get protection flags for mmap */
    prot = get_prot_flags(phdr->p_flags);
    prot_flags_to_string(prot, prot_str);
    
    printf("%-14s 0x%06x 0x%08x 0x%08x 0x%05x 0x%05x %-3s 0x%-4x",
           get_segment_type(phdr->p_type),
           phdr->p_offset,
           phdr->p_vaddr,
           phdr->p_paddr,
           phdr->p_filesz,
           phdr->p_memsz,
           flags,
           phdr->p_align);
    
    /* Task 1b: Print mmap flags for LOAD segments */
    if (phdr->p_type == PT_LOAD) {
        printf("  Prot: %s (0x%x), MAP_PRIVATE | MAP_FIXED",
               prot_str, prot);
    }
    
    printf("\n");
}

/* Task 2b: Load a program header into memory */
void load_phdr(Elf32_Phdr *phdr, int fd) {
    void *map_result;
    int prot;
    void *vaddr;
    off_t offset;
    size_t padding;
    
    /* Only map PT_LOAD segments */
    if (phdr->p_type != PT_LOAD) {
        return;
    }
    
    /* Print information about the segment being mapped */
    char flags[4];
    char prot_str[4];
    flags_to_string(phdr->p_flags, flags);
    prot = get_prot_flags(phdr->p_flags);
    prot_flags_to_string(prot, prot_str);
    
    printf("Loading segment: VirtAddr=0x%08x, Offset=0x%06x, FileSiz=0x%05x, MemSiz=0x%05x, Flags=%s\n",
           phdr->p_vaddr, phdr->p_offset, phdr->p_filesz, phdr->p_memsz, flags);
    
    /* Calculate page-aligned addresses and offset
     * mmap requires addresses aligned to PAGE_SIZE (0x1000 = 4096 bytes)
     */
    vaddr = (void *)(phdr->p_vaddr & 0xfffff000);      /* Round down to page boundary */
    offset = phdr->p_offset & 0xfffff000;              /* Round down to page boundary */
    padding = phdr->p_vaddr & 0xfff;                   /* Calculate padding from alignment */
    
    printf("  Adjusted: VirtAddr=0x%08x, Offset=0x%06x, Padding=0x%03lx\n",
           (unsigned int)vaddr, (unsigned int)offset, (unsigned long)padding);
    
    /* Map the segment from the file into memory */
    map_result = mmap(vaddr, 
                      phdr->p_memsz + padding,     /* Size including padding */
                      prot,                         /* Protection flags */
                      MAP_PRIVATE | MAP_FIXED,      /* Mapping flags */
                      fd,                           /* File descriptor */
                      offset);                      /* File offset (page-aligned) */
    
    if (map_result == MAP_FAILED) {
        perror("Error: mmap failed");
        fprintf(stderr, "  Failed to map VirtAddr=0x%08x, Size=0x%lx, Prot=%s\n",
                (unsigned int)vaddr, (unsigned long)(phdr->p_memsz + padding), prot_str);
        exit(1);
    }
    
    /* Verify the mapping was placed at the correct address */
    if (map_result != vaddr) {
        fprintf(stderr, "Error: mmap returned wrong address (expected %p, got %p)\n",
                vaddr, map_result);
        exit(1);
    }
    
    printf("  Successfully mapped to 0x%08x with protection %s\n",
           (unsigned int)map_result, prot_str);
    
    /* If MemSiz > FileSiz, we need to zero out the extra bytes
     * This handles the .bss section (uninitialized data)
     */
    if (phdr->p_memsz > phdr->p_filesz) {
        size_t zero_start = phdr->p_vaddr + phdr->p_filesz;
        size_t zero_size = phdr->p_memsz - phdr->p_filesz;
        
        printf("  Zeroing BSS: 0x%08lx to 0x%08lx (%lu bytes)\n",
               (unsigned long)zero_start,
               (unsigned long)(zero_start + zero_size),
               (unsigned long)zero_size);
        
        memset((void *)zero_start, 0, zero_size);
    }
}

/* Function to print program header information (Task 0) */
void print_phdr(Elf32_Phdr *phdr, int index) {
    printf("Program header number %d at address %p\n", index, (void *)phdr);
}

/* Iterator function that applies func to each program header */
int foreach_phdr(void *map_start, void (*func)(Elf32_Phdr *, int), int arg) {
    Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start;
    Elf32_Phdr *phdr_table;
    int i;
    
    /* Get pointer to program header table */
    phdr_table = (Elf32_Phdr *)((char *)map_start + elf_header->e_phoff);
    
    /* Iterate over all program headers */
    for (i = 0; i < elf_header->e_phnum; i++) {
        func(&phdr_table[i], i);
    }
    
    return 0;
}

/* Wrapper function for load_phdr to work with foreach_phdr */
typedef struct {
    int fd;
} load_phdr_arg;

void load_phdr_wrapper(Elf32_Phdr *phdr, int index) {
    /* The fd is passed via a global or we need to modify foreach_phdr */
    /* For now, we'll handle this differently in main */
}

/* Function to load all PT_LOAD segments */
void load_segments(void *map_start, int fd) {
    Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start;
    Elf32_Phdr *phdr_table;
    int i;
    
    printf("\n=== Loading PT_LOAD segments ===\n");
    
    /* Get pointer to program header table */
    phdr_table = (Elf32_Phdr *)((char *)map_start + elf_header->e_phoff);
    
    /* Iterate and load each segment */
    for (i = 0; i < elf_header->e_phnum; i++) {
        load_phdr(&phdr_table[i], fd);
    }
    
    printf("=== Finished loading segments ===\n\n");
}

/* Task 2c: Start execution at entry point (without arguments) */
void start_execution(void *map_start) {
    Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start;
    void (*entry_point)();
    int ret_val;
    
    /* Get the entry point address from ELF header */
    entry_point = (void (*)())elf_header->e_entry;
    
    printf("=== Starting execution (Task 2c - no arguments) ===\n");
    printf("Entry point: 0x%08x\n", elf_header->e_entry);
    printf("Transferring control to loaded program...\n\n");
    
    /* Transfer control to the loaded program */
    /* This calls the function at the entry point address */
    entry_point();
    
    /* If we reach here, the program returned */
    printf("\n=== Program returned from entry point ===\n");
    printf("Note: Most programs don't return; they call exit() instead.\n");
}

/* Task 2d: Start execution with command-line arguments */
void start_execution_with_args(void *map_start, int argc, char **argv) {
    Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start;
    void (*entry_point)();
    
    /* Get the entry point address from ELF header */
    entry_point = (void (*)())elf_header->e_entry;
    
    printf("=== Starting execution (Task 2d - with arguments) ===\n");
    printf("Entry point: 0x%08x\n", elf_header->e_entry);
    printf("Passing %d arguments to loaded program\n", argc);
    
    /* Print the arguments being passed */
    printf("Arguments: ");
    for (int i = 0; i < argc; i++) {
        printf("%s ", argv[i]);
    }
    printf("\n\n");
    
    /* Use startup function to execute with proper argument setup */
    /* startup(argc, argv, entry_point) sets up the stack and calls entry_point */
    startup(argc, argv, entry_point);
    
    /* If we reach here, the program returned */
    printf("\n=== Program returned from startup ===\n");
    printf("Note: Most programs don't return; they call exit() instead.\n");
}

/* Function to print table header for readelf-like output */
void print_table_header() {
    printf("\nProgram Headers:\n");
    printf("%-14s %-8s %-10s %-10s %-7s %-7s %-3s %-5s\n",
           "Type", "Offset", "VirtAddr", "PhysAddr", "FileSiz", "MemSiz", "Flg", "Align");
}

int main(int argc, char *argv[]) {
    int fd;
    struct stat st;
    void *map_start;
    int task = 0;
    
    /* Check command line arguments */
    if (argc < 2) {
        fprintf(stderr, "Usage: %s <ELF_file> [task_number] [args...]\n", argv[0]);
        fprintf(stderr, "  task_number: 0 (simple), 1a (readelf), 1b (with mmap flags), 2b (load), 2c (execute), 2d (execute with args)\n");
        fprintf(stderr, "  For task 2d: %s <ELF_file> 2d [program_args...]\n", argv[0]);
        return 1;
    }
    
    /* Optional: specify which task to run */
    if (argc >= 3) {
        if (strcmp(argv[2], "1a") == 0) {
            task = 1;
        } else if (strcmp(argv[2], "1b") == 0) {
            task = 2;
        } else if (strcmp(argv[2], "2b") == 0) {
            task = 3;
        } else if (strcmp(argv[2], "2c") == 0) {
            task = 4;
        } else if (strcmp(argv[2], "2d") == 0) {
            task = 5;
        } else {
            task = atoi(argv[2]);
        }
    } else {
        /* Default to task 2d (full loader with arguments) */
        task = 5;
    }
    
    /* Open the ELF file */
    fd = open(argv[1], O_RDONLY);
    if (fd < 0) {
        perror("Error opening file");
        return 1;
    }
    
    /* Get file size */
    if (fstat(fd, &st) < 0) {
        perror("Error getting file status");
        close(fd);
        return 1;
    }
    
    /* Map the file into memory */
    map_start = mmap(NULL, st.st_size, PROT_READ, MAP_PRIVATE, fd, 0);
    if (map_start == MAP_FAILED) {
        perror("Error mapping file");
        close(fd);
        return 1;
    }
    
    /* Verify it's an ELF file (do this once in main) */
    Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start;
    if (elf_header->e_ident[EI_MAG0] != ELFMAG0 ||
        elf_header->e_ident[EI_MAG1] != ELFMAG1 ||
        elf_header->e_ident[EI_MAG2] != ELFMAG2 ||
        elf_header->e_ident[EI_MAG3] != ELFMAG3) {
        fprintf(stderr, "Error: Not a valid ELF file\n");
        munmap(map_start, st.st_size);
        close(fd);
        return 1;
    }
    
    /* Execute the appropriate task */
    if (task == 0) {
        /* Task 0: Simple iteration */
        printf("Task 0: Simple program header iteration\n");
        foreach_phdr(map_start, print_phdr, 0);
    } else if (task == 1) {
        /* Task 1a: readelf -l functionality */
        printf("Task 1a: readelf -l style output\n");
        print_table_header();
        foreach_phdr(map_start, print_phdr_details, 0);
    } else if (task == 2) {
        /* Task 1b: Include mmap flags */
        printf("Task 1b: With mmap protection and mapping flags\n");
        print_table_header();
        foreach_phdr(map_start, print_phdr_with_mmap_flags, 0);
    } else if (task == 3) {
        /* Task 2b: Load segments into memory */
        printf("Task 2b: Loading segments into memory\n");
        print_table_header();
        foreach_phdr(map_start, print_phdr_details, 0);
        
        /* Now actually load the segments */
        load_segments(map_start, fd);
    } else if (task == 4) {
        /* Task 2c: Load and execute without arguments */
        printf("Task 2c: Load and execute program (no arguments)\n");
        print_table_header();
        foreach_phdr(map_start, print_phdr_details, 0);
        
        /* Load the segments */
        load_segments(map_start, fd);
        
        /* Start execution at entry point (no arguments) */
        start_execution(map_start);
    } else if (task == 5) {
        /* Task 2d: Load and execute with command-line arguments */
        printf("Task 2d: Load and execute program (with arguments)\n");
        print_table_header();
        foreach_phdr(map_start, print_phdr_details, 0);
        
        /* Load the segments */
        load_segments(map_start, fd);
        
        /* Prepare arguments for the loaded program
         * argv[0] = loader name (skip)
         * argv[1] = ELF file name (becomes argv[0] for loaded program)
         * argv[2] = task number "2d" (skip)
         * argv[3...] = actual arguments for the loaded program
         * 
         * We need to create a new argv array:
         * new_argv[0] = argv[1] (program name)
         * new_argv[1] = argv[3] (first real argument)
         * new_argv[2] = argv[4] (second real argument)
         * ...
         */
        int prog_argc;
        char **prog_argv;
        
        if (argc > 3) {
            /* We have arguments after the task number */
            prog_argc = argc - 2;        /* Total args minus loader name and task number */
            
            /* Create new argv array with program name + actual arguments */
            prog_argv = malloc(sizeof(char*) * (prog_argc + 1));
            prog_argv[0] = argv[1];      /* Program name */
            
            /* Copy actual arguments (skip loader, program name, and task number) */
            for (int i = 1; i < prog_argc; i++) {
                prog_argv[i] = argv[i + 2];  /* argv[3], argv[4], ... */
            }
            prog_argv[prog_argc] = NULL;  /* NULL terminate */
        } else {
            /* No additional arguments, just pass the program name */
            prog_argc = 1;
            prog_argv = malloc(sizeof(char*) * 2);
            prog_argv[0] = argv[1];
            prog_argv[1] = NULL;
        }
        
        /* Start execution with arguments using startup() */
        start_execution_with_args(map_start, prog_argc, prog_argv);
        
        /* Free the allocated argv array */
        free(prog_argv);
    }
    
    /* Cleanup - Note: This may not execute if the loaded program calls exit() */
    munmap(map_start, st.st_size);
    close(fd);
    
    return 0;
}
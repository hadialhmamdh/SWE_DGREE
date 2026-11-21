#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <elf.h>

// Global variables for up to 2 ELF files
char debug_mode = 0;
int fd1 = -1;
int fd2 = -1;
void *map_start1 = NULL;
void *map_start2 = NULL;
size_t file_size1 = 0;
size_t file_size2 = 0;
int current_file_count = 0;

// Function declarations
void toggle_debug_mode();
void examine_elf_file();
void print_section_names();
void print_symbols();
void print_relocations();
void check_files_for_merge();
void merge_elf_files();
void quit();
void cleanup_file(int *fd, void **map_start, size_t *file_size);

// Menu structure
struct menu_item {
    char *name;
    void (*func)();
};

struct menu_item menu[] = {
    {"Toggle Debug Mode", toggle_debug_mode},
    {"Examine ELF File", examine_elf_file},
    {"Print Section Names", print_section_names},
    {"Print Symbols", print_symbols},
    {"Print Relocations", print_relocations},
    {"Check Files for Merge", check_files_for_merge},
    {"Merge ELF Files", merge_elf_files},
    {"Quit", quit},
    {NULL, NULL}
};

void toggle_debug_mode() {
    if (debug_mode) {
        debug_mode = 0;
        fprintf(stderr, "Debug flag now off\n");
    } else {
        debug_mode = 1;
        fprintf(stderr, "Debug flag now on\n");
    }
}

void cleanup_file(int *fd, void **map_start, size_t *file_size) {
    if (*map_start != NULL && *map_start != MAP_FAILED) {
        munmap(*map_start, *file_size);
        *map_start = NULL;
    }
    if (*fd != -1) {
        close(*fd);
        *fd = -1;
    }
    *file_size = 0;
}

void examine_elf_file() {
    char filename[256];
    
    // Check if we already have 2 files open
    if (current_file_count >= 2) {
        fprintf(stderr, "Error: Already examining 2 files. Close them first.\n");
        return;
    }
    
    // Get filename from user
    printf("Enter ELF file name: ");
    if (fgets(filename, sizeof(filename), stdin) == NULL) {
        fprintf(stderr, "Error reading filename\n");
        return;
    }
    // Remove newline
    filename[strcspn(filename, "\n")] = 0;
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Examining file '%s'\n", filename);
    }
    
    // Determine which slot to use
    int *fd_ptr;
    void **map_ptr;
    size_t *size_ptr;
    int file_num;
    
    if (fd1 == -1) {
        fd_ptr = &fd1;
        map_ptr = &map_start1;
        size_ptr = &file_size1;
        file_num = 1;
    } else if (fd2 == -1) {
        fd_ptr = &fd2;
        map_ptr = &map_start2;
        size_ptr = &file_size2;
        file_num = 2;
    } else {
        fprintf(stderr, "Error: Already examining 2 files\n");
        return;
    }
    
    // Open the file
    *fd_ptr = open(filename, O_RDONLY);
    if (*fd_ptr == -1) {
        fprintf(stderr, "Error: Could not open file '%s'\n", filename);
        perror("open");
        return;
    }
    
    // Get file size
    struct stat st;
    if (fstat(*fd_ptr, &st) == -1) {
        fprintf(stderr, "Error: Could not get file size\n");
        perror("fstat");
        close(*fd_ptr);
        *fd_ptr = -1;
        return;
    }
    *size_ptr = st.st_size;
    
    if (debug_mode) {
        fprintf(stderr, "Debug: File size = %zu bytes\n", *size_ptr);
    }
    
    // Map the entire file into memory
    *map_ptr = mmap(NULL, *size_ptr, PROT_READ, MAP_PRIVATE, *fd_ptr, 0);
    if (*map_ptr == MAP_FAILED) {
        fprintf(stderr, "Error: Could not map file\n");
        perror("mmap");
        close(*fd_ptr);
        *fd_ptr = -1;
        return;
    }
    
    if (debug_mode) {
        fprintf(stderr, "Debug: File mapped at address %p\n", *map_ptr);
    }
    
    // Cast to ELF header
    Elf32_Ehdr *elf_header = (Elf32_Ehdr *)*map_ptr;
    
    // Check ELF magic number
    if (elf_header->e_ident[EI_MAG0] != ELFMAG0 ||
        elf_header->e_ident[EI_MAG1] != ELFMAG1 ||
        elf_header->e_ident[EI_MAG2] != ELFMAG2 ||
        elf_header->e_ident[EI_MAG3] != ELFMAG3) {
        fprintf(stderr, "Error: Not a valid ELF file\n");
        cleanup_file(fd_ptr, map_ptr, size_ptr);
        return;
    }
    
    // Successfully opened and mapped - increment counter
    current_file_count++;
    
    // Print ELF header information
    printf("\n");
    printf("ELF Header for file%d:\n", file_num);
    
    // Print magic number (bytes 1, 2, 3 - which are indices 1, 2, 3)
    printf("  Magic:   %c%c%c\n", 
           elf_header->e_ident[EI_MAG1],
           elf_header->e_ident[EI_MAG2],
           elf_header->e_ident[EI_MAG3]);
    
    // Print data encoding scheme
    printf("  Data:                              ");
    switch(elf_header->e_ident[EI_DATA]) {
        case ELFDATA2LSB:
            printf("2's complement, little endian\n");
            break;
        case ELFDATA2MSB:
            printf("2's complement, big endian\n");
            break;
        default:
            printf("Unknown\n");
            break;
    }
    
    // Print entry point
    printf("  Entry point address:               0x%x\n", elf_header->e_entry);
    
    // Print section header table information
    printf("  Start of section headers:          %u (bytes into file)\n", 
           elf_header->e_shoff);
    printf("  Number of section headers:         %u\n", 
           elf_header->e_shnum);
    printf("  Size of section headers:           %u (bytes)\n", 
           elf_header->e_shentsize);
    
    // Print program header table information
    printf("  Start of program headers:          %u (bytes into file)\n", 
           elf_header->e_phoff);
    printf("  Number of program headers:         %u\n", 
           elf_header->e_phnum);
    printf("  Size of program headers:           %u (bytes)\n", 
           elf_header->e_phentsize);
    
    printf("\n");
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Successfully examined ELF file\n");
        fprintf(stderr, "Debug: Current file count = %d\n", current_file_count);
    }
}

void print_section_names() {
    // Check if any files are mapped
    if (fd1 == -1 && fd2 == -1) {
        fprintf(stderr, "Error: No ELF files are currently mapped\n");
        return;
    }
    
    // Process file 1 if it exists
    if (fd1 != -1 && map_start1 != NULL) {
        Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start1;
        
        printf("\nFile: file1\n");
        
        // Get section header table information
        Elf32_Shdr *section_headers = (Elf32_Shdr *)(map_start1 + elf_header->e_shoff);
        int num_sections = elf_header->e_shnum;
        int shstrndx = elf_header->e_shstrndx;
        
        if (debug_mode) {
            fprintf(stderr, "Debug: Section header table offset = %u\n", elf_header->e_shoff);
            fprintf(stderr, "Debug: Number of sections = %d\n", num_sections);
            fprintf(stderr, "Debug: Section header string table index = %d\n", shstrndx);
        }
        
        // Get pointer to section header string table
        Elf32_Shdr *shstrtab_header = &section_headers[shstrndx];
        char *string_table = (char *)(map_start1 + shstrtab_header->sh_offset);
        
        if (debug_mode) {
            fprintf(stderr, "Debug: String table offset = %u\n", shstrtab_header->sh_offset);
        }
        
        // Print header
        printf("[index] %-20s %-10s %-10s %-10s %s\n", 
               "section_name", "section_addr", "section_offset", "section_size", "section_type");
        
        // Iterate through all sections
        for (int i = 0; i < num_sections; i++) {
            Elf32_Shdr *section = &section_headers[i];
            char *section_name = string_table + section->sh_name;
            
            if (debug_mode) {
                fprintf(stderr, "Debug: Section[%d] name offset = %u\n", i, section->sh_name);
            }
            
            printf("[%2d]   %-20s 0x%08x 0x%08x 0x%08x %u\n",
                   i,
                   section_name,
                   section->sh_addr,
                   section->sh_offset,
                   section->sh_size,
                   section->sh_type);
        }
        printf("\n");
    }
    
    // Process file 2 if it exists
    if (fd2 != -1 && map_start2 != NULL) {
        Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start2;
        
        printf("\nFile: file2\n");
        
        // Get section header table information
        Elf32_Shdr *section_headers = (Elf32_Shdr *)(map_start2 + elf_header->e_shoff);
        int num_sections = elf_header->e_shnum;
        int shstrndx = elf_header->e_shstrndx;
        
        if (debug_mode) {
            fprintf(stderr, "Debug: Section header table offset = %u\n", elf_header->e_shoff);
            fprintf(stderr, "Debug: Number of sections = %d\n", num_sections);
            fprintf(stderr, "Debug: Section header string table index = %d\n", shstrndx);
        }
        
        // Get pointer to section header string table
        Elf32_Shdr *shstrtab_header = &section_headers[shstrndx];
        char *string_table = (char *)(map_start2 + shstrtab_header->sh_offset);
        
        if (debug_mode) {
            fprintf(stderr, "Debug: String table offset = %u\n", shstrtab_header->sh_offset);
        }
        
        // Print header
        printf("[index] %-20s %-10s %-10s %-10s %s\n", 
               "section_name", "section_addr", "section_offset", "section_size", "section_type");
        
        // Iterate through all sections
        for (int i = 0; i < num_sections; i++) {
            Elf32_Shdr *section = &section_headers[i];
            char *section_name = string_table + section->sh_name;
            
            if (debug_mode) {
                fprintf(stderr, "Debug: Section[%d] name offset = %u\n", i, section->sh_name);
            }
            
            printf("[%2d]   %-20s 0x%08x 0x%08x 0x%08x %u\n",
                   i,
                   section_name,
                   section->sh_addr,
                   section->sh_offset,
                   section->sh_size,
                   section->sh_type);
        }
        printf("\n");
    }
}

void print_symbols() {
    // Check if any files are mapped
    if (fd1 == -1 && fd2 == -1) {
        fprintf(stderr, "Error: No ELF files are currently mapped\n");
        return;
    }
    
    // Process file 1 if it exists
    if (fd1 != -1 && map_start1 != NULL) {
        Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start1;
        Elf32_Shdr *section_headers = (Elf32_Shdr *)(map_start1 + elf_header->e_shoff);
        int num_sections = elf_header->e_shnum;
        int shstrndx = elf_header->e_shstrndx;
        
        // Get section name string table
        Elf32_Shdr *shstrtab_header = &section_headers[shstrndx];
        char *section_string_table = (char *)(map_start1 + shstrtab_header->sh_offset);
        
        printf("\nFile: file1\n\n");
        
        // Find symbol table sections (.symtab and .dynsym)
        int found_symbols = 0;
        
        for (int s = 0; s < num_sections; s++) {
            Elf32_Shdr *section = &section_headers[s];
            
            // Check if this is a symbol table (SYMTAB or DYNSYM)
            if (section->sh_type == SHT_SYMTAB || section->sh_type == SHT_DYNSYM) {
                found_symbols = 1;
                
                // Get symbol table
                Elf32_Sym *symbol_table = (Elf32_Sym *)(map_start1 + section->sh_offset);
                int num_symbols = section->sh_size / section->sh_entsize;
                
                // Get the string table for symbol names (sh_link points to string table)
                Elf32_Shdr *strtab_header = &section_headers[section->sh_link];
                char *symbol_string_table = (char *)(map_start1 + strtab_header->sh_offset);
                
                if (debug_mode) {
                    fprintf(stderr, "Debug: Symbol table '%s' found at section %d\n", 
                            section_string_table + section->sh_name, s);
                    fprintf(stderr, "Debug: Symbol table size = %u bytes\n", section->sh_size);
                    fprintf(stderr, "Debug: Number of symbols = %d\n", num_symbols);
                    fprintf(stderr, "Debug: Symbol string table at section %u\n", section->sh_link);
                }
                
                printf("Symbol table '%s':\n", section_string_table + section->sh_name);
                printf("[index] %-10s %-15s %-20s %s\n", 
                       "value", "section_index", "section_name", "symbol_name");
                
                // Iterate through all symbols
                for (int i = 0; i < num_symbols; i++) {
                    Elf32_Sym *symbol = &symbol_table[i];
                    char *symbol_name = symbol_string_table + symbol->st_name;
                    
                    // Get section name
                    char *section_name;
                    if (symbol->st_shndx == SHN_UNDEF) {
                        section_name = "UND";
                    } else if (symbol->st_shndx == SHN_ABS) {
                        section_name = "ABS";
                    } else if (symbol->st_shndx == SHN_COMMON) {
                        section_name = "COM";
                    } else if (symbol->st_shndx < num_sections) {
                        Elf32_Shdr *sym_section = &section_headers[symbol->st_shndx];
                        section_name = section_string_table + sym_section->sh_name;
                    } else {
                        section_name = "???";
                    }
                    
                    printf("[%4d]  0x%08x %-15d %-20s %s\n",
                           i,
                           symbol->st_value,
                           symbol->st_shndx,
                           section_name,
                           symbol_name);
                }
                printf("\n");
            }
        }
        
        if (!found_symbols) {
            fprintf(stderr, "Error: No symbol tables found in file1\n");
        }
    }
    
    // Process file 2 if it exists
    if (fd2 != -1 && map_start2 != NULL) {
        Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start2;
        Elf32_Shdr *section_headers = (Elf32_Shdr *)(map_start2 + elf_header->e_shoff);
        int num_sections = elf_header->e_shnum;
        int shstrndx = elf_header->e_shstrndx;
        
        // Get section name string table
        Elf32_Shdr *shstrtab_header = &section_headers[shstrndx];
        char *section_string_table = (char *)(map_start2 + shstrtab_header->sh_offset);
        
        printf("\nFile: file2\n\n");
        
        // Find symbol table sections
        int found_symbols = 0;
        
        for (int s = 0; s < num_sections; s++) {
            Elf32_Shdr *section = &section_headers[s];
            
            if (section->sh_type == SHT_SYMTAB || section->sh_type == SHT_DYNSYM) {
                found_symbols = 1;
                
                Elf32_Sym *symbol_table = (Elf32_Sym *)(map_start2 + section->sh_offset);
                int num_symbols = section->sh_size / section->sh_entsize;
                
                Elf32_Shdr *strtab_header = &section_headers[section->sh_link];
                char *symbol_string_table = (char *)(map_start2 + strtab_header->sh_offset);
                
                if (debug_mode) {
                    fprintf(stderr, "Debug: Symbol table '%s' found at section %d\n", 
                            section_string_table + section->sh_name, s);
                    fprintf(stderr, "Debug: Symbol table size = %u bytes\n", section->sh_size);
                    fprintf(stderr, "Debug: Number of symbols = %d\n", num_symbols);
                    fprintf(stderr, "Debug: Symbol string table at section %u\n", section->sh_link);
                }
                
                printf("Symbol table '%s':\n", section_string_table + section->sh_name);
                printf("[index] %-10s %-15s %-20s %s\n", 
                       "value", "section_index", "section_name", "symbol_name");
                
                for (int i = 0; i < num_symbols; i++) {
                    Elf32_Sym *symbol = &symbol_table[i];
                    char *symbol_name = symbol_string_table + symbol->st_name;
                    
                    char *section_name;
                    if (symbol->st_shndx == SHN_UNDEF) {
                        section_name = "UND";
                    } else if (symbol->st_shndx == SHN_ABS) {
                        section_name = "ABS";
                    } else if (symbol->st_shndx == SHN_COMMON) {
                        section_name = "COM";
                    } else if (symbol->st_shndx < num_sections) {
                        Elf32_Shdr *sym_section = &section_headers[symbol->st_shndx];
                        section_name = section_string_table + sym_section->sh_name;
                    } else {
                        section_name = "???";
                    }
                    
                    printf("[%4d]  0x%08x %-15d %-20s %s\n",
                           i,
                           symbol->st_value,
                           symbol->st_shndx,
                           section_name,
                           symbol_name);
                }
                printf("\n");
            }
        }
        
        if (!found_symbols) {
            fprintf(stderr, "Error: No symbol tables found in file2\n");
        }
    }
}

void print_relocations() {
    // Check if any files are mapped
    if (fd1 == -1 && fd2 == -1) {
        fprintf(stderr, "Error: No ELF files are currently mapped\n");
        return;
    }
    
    // Process file 1 if it exists
    if (fd1 != -1 && map_start1 != NULL) {
        Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start1;
        Elf32_Shdr *section_headers = (Elf32_Shdr *)(map_start1 + elf_header->e_shoff);
        int num_sections = elf_header->e_shnum;
        int shstrndx = elf_header->e_shstrndx;
        
        // Get section name string table
        Elf32_Shdr *shstrtab_header = &section_headers[shstrndx];
        char *section_string_table = (char *)(map_start1 + shstrtab_header->sh_offset);
        
        printf("\nFile: file1 relocations\n\n");
        
        // Find relocation sections (.rel.* or .rela.*)
        int found_relocations = 0;
        
        for (int s = 0; s < num_sections; s++) {
            Elf32_Shdr *section = &section_headers[s];
            
            // Check if this is a relocation section (REL or RELA)
            if (section->sh_type == SHT_REL) {
                found_relocations = 1;
                
                // Get relocation table
                Elf32_Rel *rel_table = (Elf32_Rel *)(map_start1 + section->sh_offset);
                int num_relocations = section->sh_size / section->sh_entsize;
                
                // Get the symbol table (sh_link points to symbol table)
                Elf32_Shdr *symtab_header = &section_headers[section->sh_link];
                Elf32_Sym *symbol_table = (Elf32_Sym *)(map_start1 + symtab_header->sh_offset);
                
                // Get string table for symbol names
                Elf32_Shdr *strtab_header = &section_headers[symtab_header->sh_link];
                char *symbol_string_table = (char *)(map_start1 + strtab_header->sh_offset);
                
                if (debug_mode) {
                    fprintf(stderr, "Debug: Relocation section '%s' found at section %d\n", 
                            section_string_table + section->sh_name, s);
                    fprintf(stderr, "Debug: Relocation table size = %u bytes\n", section->sh_size);
                    fprintf(stderr, "Debug: Number of relocations = %d\n", num_relocations);
                    fprintf(stderr, "Debug: Related symbol table at section %u\n", section->sh_link);
                }
                
                printf("Relocation section '%s':\n", section_string_table + section->sh_name);
                printf("[index] %-10s %-25s %-10s %s\n", 
                       "location", "related_symbol_name", "size", "type");
                
                // Iterate through all relocations
                for (int i = 0; i < num_relocations; i++) {
                    Elf32_Rel *rel = &rel_table[i];
                    
                    // Extract symbol index and type from r_info
                    int sym_index = ELF32_R_SYM(rel->r_info);
                    int rel_type = ELF32_R_TYPE(rel->r_info);
                    
                    // Get symbol name
                    Elf32_Sym *symbol = &symbol_table[sym_index];
                    char *symbol_name = symbol_string_table + symbol->st_name;
                    
                    // Determine size based on relocation type
                    int size = 4; // Default for most 32-bit relocations
                    switch(rel_type) {
                        case R_386_8:
                            size = 1;
                            break;
                        case R_386_16:
                        case R_386_PC16:
                            size = 2;
                            break;
                        case R_386_32:
                        case R_386_PC32:
                        case R_386_GOT32:
                        case R_386_PLT32:
                        case R_386_GLOB_DAT:
                        case R_386_JMP_SLOT:
                        case R_386_RELATIVE:
                        case R_386_GOTOFF:
                        case R_386_GOTPC:
                        default:
                            size = 4;
                            break;
                    }
                    
                    printf("[%4d]  0x%08x %-25s %-10d %d\n",
                           i,
                           rel->r_offset,
                           symbol_name,
                           size,
                           rel_type);
                }
                printf("\n");
            }
            else if (section->sh_type == SHT_RELA) {
                found_relocations = 1;
                
                // Get relocation table with addend
                Elf32_Rela *rela_table = (Elf32_Rela *)(map_start1 + section->sh_offset);
                int num_relocations = section->sh_size / section->sh_entsize;
                
                // Get the symbol table
                Elf32_Shdr *symtab_header = &section_headers[section->sh_link];
                Elf32_Sym *symbol_table = (Elf32_Sym *)(map_start1 + symtab_header->sh_offset);
                
                // Get string table for symbol names
                Elf32_Shdr *strtab_header = &section_headers[symtab_header->sh_link];
                char *symbol_string_table = (char *)(map_start1 + strtab_header->sh_offset);
                
                if (debug_mode) {
                    fprintf(stderr, "Debug: Relocation section '%s' found at section %d\n", 
                            section_string_table + section->sh_name, s);
                    fprintf(stderr, "Debug: Relocation table size = %u bytes\n", section->sh_size);
                    fprintf(stderr, "Debug: Number of relocations = %d\n", num_relocations);
                }
                
                printf("Relocation section '%s':\n", section_string_table + section->sh_name);
                printf("[index] %-10s %-25s %-10s %s\n", 
                       "location", "related_symbol_name", "size", "type");
                
                for (int i = 0; i < num_relocations; i++) {
                    Elf32_Rela *rela = &rela_table[i];
                    
                    int sym_index = ELF32_R_SYM(rela->r_info);
                    int rel_type = ELF32_R_TYPE(rela->r_info);
                    
                    Elf32_Sym *symbol = &symbol_table[sym_index];
                    char *symbol_name = symbol_string_table + symbol->st_name;
                    
                    int size = 4;
                    switch(rel_type) {
                        case R_386_8:
                            size = 1;
                            break;
                        case R_386_16:
                        case R_386_PC16:
                            size = 2;
                            break;
                        default:
                            size = 4;
                            break;
                    }
                    
                    printf("[%4d]  0x%08x %-25s %-10d %d\n",
                           i,
                           rela->r_offset,
                           symbol_name,
                           size,
                           rel_type);
                }
                printf("\n");
            }
        }
        
        if (!found_relocations) {
            printf("No relocations\n\n");
        }
    }
    
    // Process file 2 if it exists
    if (fd2 != -1 && map_start2 != NULL) {
        Elf32_Ehdr *elf_header = (Elf32_Ehdr *)map_start2;
        Elf32_Shdr *section_headers = (Elf32_Shdr *)(map_start2 + elf_header->e_shoff);
        int num_sections = elf_header->e_shnum;
        int shstrndx = elf_header->e_shstrndx;
        
        Elf32_Shdr *shstrtab_header = &section_headers[shstrndx];
        char *section_string_table = (char *)(map_start2 + shstrtab_header->sh_offset);
        
        printf("\nFile: file2 relocations\n\n");
        
        int found_relocations = 0;
        
        for (int s = 0; s < num_sections; s++) {
            Elf32_Shdr *section = &section_headers[s];
            
            if (section->sh_type == SHT_REL) {
                found_relocations = 1;
                
                Elf32_Rel *rel_table = (Elf32_Rel *)(map_start2 + section->sh_offset);
                int num_relocations = section->sh_size / section->sh_entsize;
                
                Elf32_Shdr *symtab_header = &section_headers[section->sh_link];
                Elf32_Sym *symbol_table = (Elf32_Sym *)(map_start2 + symtab_header->sh_offset);
                
                Elf32_Shdr *strtab_header = &section_headers[symtab_header->sh_link];
                char *symbol_string_table = (char *)(map_start2 + strtab_header->sh_offset);
                
                if (debug_mode) {
                    fprintf(stderr, "Debug: Relocation section '%s' found at section %d\n", 
                            section_string_table + section->sh_name, s);
                    fprintf(stderr, "Debug: Number of relocations = %d\n", num_relocations);
                }
                
                printf("Relocation section '%s':\n", section_string_table + section->sh_name);
                printf("[index] %-10s %-25s %-10s %s\n", 
                       "location", "related_symbol_name", "size", "type");
                
                for (int i = 0; i < num_relocations; i++) {
                    Elf32_Rel *rel = &rel_table[i];
                    
                    int sym_index = ELF32_R_SYM(rel->r_info);
                    int rel_type = ELF32_R_TYPE(rel->r_info);
                    
                    Elf32_Sym *symbol = &symbol_table[sym_index];
                    char *symbol_name = symbol_string_table + symbol->st_name;
                    
                    int size = 4;
                    switch(rel_type) {
                        case R_386_8:
                            size = 1;
                            break;
                        case R_386_16:
                        case R_386_PC16:
                            size = 2;
                            break;
                        default:
                            size = 4;
                            break;
                    }
                    
                    printf("[%4d]  0x%08x %-25s %-10d %d\n",
                           i,
                           rel->r_offset,
                           symbol_name,
                           size,
                           rel_type);
                }
                printf("\n");
            }
            else if (section->sh_type == SHT_RELA) {
                found_relocations = 1;
                
                Elf32_Rela *rela_table = (Elf32_Rela *)(map_start2 + section->sh_offset);
                int num_relocations = section->sh_size / section->sh_entsize;
                
                Elf32_Shdr *symtab_header = &section_headers[section->sh_link];
                Elf32_Sym *symbol_table = (Elf32_Sym *)(map_start2 + symtab_header->sh_offset);
                
                Elf32_Shdr *strtab_header = &section_headers[symtab_header->sh_link];
                char *symbol_string_table = (char *)(map_start2 + strtab_header->sh_offset);
                
                if (debug_mode) {
                    fprintf(stderr, "Debug: Relocation section '%s' found at section %d\n", 
                            section_string_table + section->sh_name, s);
                    fprintf(stderr, "Debug: Number of relocations = %d\n", num_relocations);
                }
                
                printf("Relocation section '%s':\n", section_string_table + section->sh_name);
                printf("[index] %-10s %-25s %-10s %s\n", 
                       "location", "related_symbol_name", "size", "type");
                
                for (int i = 0; i < num_relocations; i++) {
                    Elf32_Rela *rela = &rela_table[i];
                    
                    int sym_index = ELF32_R_SYM(rela->r_info);
                    int rel_type = ELF32_R_TYPE(rela->r_info);
                    
                    Elf32_Sym *symbol = &symbol_table[sym_index];
                    char *symbol_name = symbol_string_table + symbol->st_name;
                    
                    int size = 4;
                    switch(rel_type) {
                        case R_386_8:
                            size = 1;
                            break;
                        case R_386_16:
                        case R_386_PC16:
                            size = 2;
                            break;
                        default:
                            size = 4;
                            break;
                    }
                    
                    printf("[%4d]  0x%08x %-25s %-10d %d\n",
                           i,
                           rela->r_offset,
                           symbol_name,
                           size,
                           rel_type);
                }
                printf("\n");
            }
        }
        
        if (!found_relocations) {
            printf("No relocations\n\n");
        }
    }
}

void check_files_for_merge() {
    // Check that exactly 2 files are mapped
    if (fd1 == -1 || fd2 == -1) {
        fprintf(stderr, "Error: Need exactly 2 ELF files mapped for merge\n");
        return;
    }
    
    printf("\nChecking files for merge...\n\n");
    
    // Get ELF headers
    Elf32_Ehdr *elf_header1 = (Elf32_Ehdr *)map_start1;
    Elf32_Ehdr *elf_header2 = (Elf32_Ehdr *)map_start2;
    
    // Get section headers
    Elf32_Shdr *section_headers1 = (Elf32_Shdr *)(map_start1 + elf_header1->e_shoff);
    Elf32_Shdr *section_headers2 = (Elf32_Shdr *)(map_start2 + elf_header2->e_shoff);
    
    // Find symbol tables
    Elf32_Shdr *symtab1 = NULL;
    Elf32_Shdr *symtab2 = NULL;
    int symtab1_count = 0;
    int symtab2_count = 0;
    
    for (int i = 0; i < elf_header1->e_shnum; i++) {
        if (section_headers1[i].sh_type == SHT_SYMTAB) {
            symtab1 = &section_headers1[i];
            symtab1_count++;
        }
    }
    
    for (int i = 0; i < elf_header2->e_shnum; i++) {
        if (section_headers2[i].sh_type == SHT_SYMTAB) {
            symtab2 = &section_headers2[i];
            symtab2_count++;
        }
    }
    
    // Check that each file has exactly one symbol table
    if (symtab1_count != 1 || symtab2_count != 1) {
        fprintf(stderr, "Error: Feature not supported - need exactly one symbol table per file\n");
        return;
    }
    
    // Get symbol tables
    Elf32_Sym *symbols1 = (Elf32_Sym *)(map_start1 + symtab1->sh_offset);
    Elf32_Sym *symbols2 = (Elf32_Sym *)(map_start2 + symtab2->sh_offset);
    int num_symbols1 = symtab1->sh_size / symtab1->sh_entsize;
    int num_symbols2 = symtab2->sh_size / symtab2->sh_entsize;
    
    // Get string tables for symbol names
    Elf32_Shdr *strtab1 = &section_headers1[symtab1->sh_link];
    Elf32_Shdr *strtab2 = &section_headers2[symtab2->sh_link];
    char *string_table1 = (char *)(map_start1 + strtab1->sh_offset);
    char *string_table2 = (char *)(map_start2 + strtab2->sh_offset);
    
    if (debug_mode) {
        fprintf(stderr, "Debug: File 1 has %d symbols\n", num_symbols1);
        fprintf(stderr, "Debug: File 2 has %d symbols\n", num_symbols2);
    }
    
    int errors_found = 0;
    
    // Check symbols in file 1
    printf("Checking symbols in file 1:\n");
    for (int i = 1; i < num_symbols1; i++) {  // Skip symbol 0 (dummy)
        Elf32_Sym *sym1 = &symbols1[i];
        char *sym_name = string_table1 + sym1->st_name;
        
        // Skip empty names
        if (sym_name[0] == '\0') {
            continue;
        }
        
        if (debug_mode) {
            fprintf(stderr, "Debug: Checking symbol '%s' (shndx=%d)\n", sym_name, sym1->st_shndx);
        }
        
        // Search for this symbol in file 2
        Elf32_Sym *sym2 = NULL;
        for (int j = 1; j < num_symbols2; j++) {
            char *sym2_name = string_table2 + symbols2[j].st_name;
            if (strcmp(sym_name, sym2_name) == 0) {
                sym2 = &symbols2[j];
                break;
            }
        }
        
        // Check for errors
        if (sym1->st_shndx == SHN_UNDEF) {
            // Symbol is undefined in file 1
            if (sym2 == NULL || sym2->st_shndx == SHN_UNDEF) {
                printf("Error: Symbol %s undefined\n", sym_name);
                errors_found++;
            }
        } else {
            // Symbol is defined in file 1
            if (sym2 != NULL && sym2->st_shndx != SHN_UNDEF) {
                printf("Error: Symbol %s multiply defined\n", sym_name);
                errors_found++;
            }
        }
    }
    
    // Check symbols in file 2
    printf("\nChecking symbols in file 2:\n");
    for (int i = 1; i < num_symbols2; i++) {  // Skip symbol 0 (dummy)
        Elf32_Sym *sym2 = &symbols2[i];
        char *sym_name = string_table2 + sym2->st_name;
        
        // Skip empty names
        if (sym_name[0] == '\0') {
            continue;
        }
        
        if (debug_mode) {
            fprintf(stderr, "Debug: Checking symbol '%s' (shndx=%d)\n", sym_name, sym2->st_shndx);
        }
        
        // Search for this symbol in file 1
        Elf32_Sym *sym1 = NULL;
        for (int j = 1; j < num_symbols1; j++) {
            char *sym1_name = string_table1 + symbols1[j].st_name;
            if (strcmp(sym_name, sym1_name) == 0) {
                sym1 = &symbols1[j];
                break;
            }
        }
        
        // Check for errors
        if (sym2->st_shndx == SHN_UNDEF) {
            // Symbol is undefined in file 2
            if (sym1 == NULL || sym1->st_shndx == SHN_UNDEF) {
                printf("Error: Symbol %s undefined\n", sym_name);
                errors_found++;
            }
        } else {
            // Symbol is defined in file 2
            if (sym1 != NULL && sym1->st_shndx != SHN_UNDEF) {
                printf("Error: Symbol %s multiply defined\n", sym_name);
                errors_found++;
            }
        }
    }
    
    if (errors_found == 0) {
        printf("\nNo errors found. Files can be merged.\n");
    } else {
        printf("\nFound %d error(s).\n", errors_found);
    }
}

void merge_elf_files() {
    // Check that exactly 2 files are mapped
    if (fd1 == -1 || fd2 == -1) {
        fprintf(stderr, "Error: Need exactly 2 ELF files mapped for merge\n");
        return;
    }
    
    printf("\nMerging ELF files...\n");
    
    // Get ELF headers
    Elf32_Ehdr *elf_header1 = (Elf32_Ehdr *)map_start1;
    Elf32_Ehdr *elf_header2 = (Elf32_Ehdr *)map_start2;
    
    // Get section headers
    Elf32_Shdr *section_headers1 = (Elf32_Shdr *)(map_start1 + elf_header1->e_shoff);
    Elf32_Shdr *section_headers2 = (Elf32_Shdr *)(map_start2 + elf_header2->e_shoff);
    
    // Get section name string table from file 1
    Elf32_Shdr *shstrtab1 = &section_headers1[elf_header1->e_shstrndx];
    char *section_names1 = (char *)(map_start1 + shstrtab1->sh_offset);
    
    // Get section name string table from file 2
    Elf32_Shdr *shstrtab2 = &section_headers2[elf_header2->e_shstrndx];
    char *section_names2 = (char *)(map_start2 + shstrtab2->sh_offset);
    
    // Find symbol tables in both files
    Elf32_Shdr *symtab1 = NULL;
    Elf32_Shdr *symtab2 = NULL;
    int symtab1_idx = -1;
    
    for (int i = 0; i < elf_header1->e_shnum; i++) {
        if (section_headers1[i].sh_type == SHT_SYMTAB) {
            symtab1 = &section_headers1[i];
            symtab1_idx = i;
            break;
        }
    }
    
    for (int i = 0; i < elf_header2->e_shnum; i++) {
        if (section_headers2[i].sh_type == SHT_SYMTAB) {
            symtab2 = &section_headers2[i];
            break;
        }
    }
    
    // Get symbol tables and string tables
    Elf32_Sym *symbols1 = NULL;
    Elf32_Sym *symbols2 = NULL;
    char *strtab1 = NULL;
    char *strtab2 = NULL;
    int num_symbols1 = 0;
    int num_symbols2 = 0;
    
    if (symtab1) {
        symbols1 = (Elf32_Sym *)(map_start1 + symtab1->sh_offset);
        num_symbols1 = symtab1->sh_size / symtab1->sh_entsize;
        Elf32_Shdr *strtab1_hdr = &section_headers1[symtab1->sh_link];
        strtab1 = (char *)(map_start1 + strtab1_hdr->sh_offset);
    }
    
    if (symtab2) {
        symbols2 = (Elf32_Sym *)(map_start2 + symtab2->sh_offset);
        num_symbols2 = symtab2->sh_size / symtab2->sh_entsize;
        Elf32_Shdr *strtab2_hdr = &section_headers2[symtab2->sh_link];
        strtab2 = (char *)(map_start2 + strtab2_hdr->sh_offset);
    }
    
    // BONUS PART: Create resolved symbol table
    Elf32_Sym *resolved_symbols = NULL;
    if (symtab1) {
        resolved_symbols = malloc(symtab1->sh_size);
        if (resolved_symbols == NULL) {
            fprintf(stderr, "Error: Memory allocation failed\n");
            return;
        }
        memcpy(resolved_symbols, symbols1, symtab1->sh_size);
        
        if (debug_mode) {
            fprintf(stderr, "Debug: Resolving symbols...\n");
        }
        
        // Keep track of section size increases for symbol value adjustments
        int section_size_increases[elf_header1->e_shnum];
        for (int i = 0; i < elf_header1->e_shnum; i++) {
            section_size_increases[i] = 0;
        }
        
        // Calculate size increases for mergeable sections
        for (int i = 0; i < elf_header1->e_shnum; i++) {
            char *section_name = section_names1 + section_headers1[i].sh_name;
            if (strcmp(section_name, ".text") == 0 || 
                strcmp(section_name, ".data") == 0 || 
                strcmp(section_name, ".rodata") == 0) {
                // Find this section's size in file1
                section_size_increases[i] = section_headers1[i].sh_size;
            }
        }
        
        // Resolve undefined symbols from file 2
        for (int i = 1; i < num_symbols1; i++) {
            Elf32_Sym *sym1 = &resolved_symbols[i];
            char *sym_name = strtab1 + sym1->st_name;
            
            if (sym_name[0] == '\0') continue;
            
            // If symbol is undefined in file 1, try to resolve from file 2
            if (sym1->st_shndx == SHN_UNDEF && symtab2) {
                for (int j = 1; j < num_symbols2; j++) {
                    char *sym2_name = strtab2 + symbols2[j].st_name;
                    if (strcmp(sym_name, sym2_name) == 0) {
                        // Found matching symbol in file 2
                        if (symbols2[j].st_shndx != SHN_UNDEF) {
                            // Symbol is defined in file 2 - resolve it!
                            
                            // Find the matching section in file 2
                            char *section_name2 = section_names2 + section_headers2[symbols2[j].st_shndx].sh_name;
                            
                            // Find corresponding section in file 1
                            int target_section = -1;
                            for (int k = 0; k < elf_header1->e_shnum; k++) {
                                char *section_name1 = section_names1 + section_headers1[k].sh_name;
                                if (strcmp(section_name1, section_name2) == 0) {
                                    target_section = k;
                                    break;
                                }
                            }
                            
                            if (target_section != -1) {
                                // Update symbol to point to merged section
                                sym1->st_shndx = target_section;
                                // Adjust value: original value from file2 + size of section from file1
                                sym1->st_value = symbols2[j].st_value + section_size_increases[target_section];
                                sym1->st_size = symbols2[j].st_size;
                                sym1->st_info = symbols2[j].st_info;
                                sym1->st_other = symbols2[j].st_other;
                                
                                if (debug_mode) {
                                    fprintf(stderr, "Debug: Resolved symbol '%s': section=%d, value=0x%x\n",
                                           sym_name, sym1->st_shndx, sym1->st_value);
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
    
    // Create output file
    int out_fd = open("out.ro", O_WRONLY | O_CREAT | O_TRUNC, 0644);
    if (out_fd == -1) {
        fprintf(stderr, "Error: Could not create output file 'out.ro'\n");
        perror("open");
        if (resolved_symbols) free(resolved_symbols);
        return;
    }
    
    // Step 1: Write initial ELF header (copy from file 1)
    Elf32_Ehdr out_header = *elf_header1;
    if (write(out_fd, &out_header, sizeof(Elf32_Ehdr)) != sizeof(Elf32_Ehdr)) {
        fprintf(stderr, "Error: Failed to write ELF header\n");
        close(out_fd);
        if (resolved_symbols) free(resolved_symbols);
        return;
    }
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Wrote ELF header (%zu bytes)\n", sizeof(Elf32_Ehdr));
    }
    
    // Step 2: Create modified section header table (in memory)
    Elf32_Shdr *out_section_headers = malloc(elf_header1->e_shnum * sizeof(Elf32_Shdr));
    if (out_section_headers == NULL) {
        fprintf(stderr, "Error: Memory allocation failed\n");
        close(out_fd);
        if (resolved_symbols) free(resolved_symbols);
        return;
    }
    memcpy(out_section_headers, section_headers1, elf_header1->e_shnum * sizeof(Elf32_Shdr));
    
    // Step 3: Process each section and write to output file
    for (int i = 0; i < elf_header1->e_shnum; i++) {
        char *section_name = section_names1 + out_section_headers[i].sh_name;
        
        if (debug_mode) {
            fprintf(stderr, "Debug: Processing section [%d] '%s'\n", i, section_name);
        }
        
        // Get current file position (this will be the new offset)
        off_t current_offset = lseek(out_fd, 0, SEEK_CUR);
        out_section_headers[i].sh_offset = current_offset;
        
        // Check if this is the symbol table section
        if (i == symtab1_idx && resolved_symbols) {
            // Write the RESOLVED symbol table
            if (write(out_fd, resolved_symbols, symtab1->sh_size) != (ssize_t)symtab1->sh_size) {
                fprintf(stderr, "Error: Failed to write resolved symbol table\n");
                free(out_section_headers);
                free(resolved_symbols);
                close(out_fd);
                return;
            }
            
            if (debug_mode) {
                fprintf(stderr, "Debug: Wrote resolved symbol table: size=%u\n", symtab1->sh_size);
            }
        }
        // Check if this is a mergeable section
        else if (strcmp(section_name, ".text") == 0 || 
            strcmp(section_name, ".data") == 0 || 
            strcmp(section_name, ".rodata") == 0) {
            
            // Mergeable section - concatenate contents from both files
            
            // Write contents from file 1
            void *section_data1 = map_start1 + section_headers1[i].sh_offset;
            size_t size1 = section_headers1[i].sh_size;
            
            if (size1 > 0) {
                if (write(out_fd, section_data1, size1) != (ssize_t)size1) {
                    fprintf(stderr, "Error: Failed to write section data from file 1\n");
                    free(out_section_headers);
                    if (resolved_symbols) free(resolved_symbols);
                    close(out_fd);
                    return;
                }
            }
            
            // Find matching section in file 2
            size_t size2 = 0;
            for (int j = 0; j < elf_header2->e_shnum; j++) {
                char *section_name2 = section_names2 + section_headers2[j].sh_name;
                if (strcmp(section_name, section_name2) == 0) {
                    // Found matching section - append its contents
                    void *section_data2 = map_start2 + section_headers2[j].sh_offset;
                    size2 = section_headers2[j].sh_size;
                    
                    if (size2 > 0) {
                        if (write(out_fd, section_data2, size2) != (ssize_t)size2) {
                            fprintf(stderr, "Error: Failed to write section data from file 2\n");
                            free(out_section_headers);
                            if (resolved_symbols) free(resolved_symbols);
                            close(out_fd);
                            return;
                        }
                    }
                    break;
                }
            }
            
            // Update section size to be sum of both
            out_section_headers[i].sh_size = size1 + size2;
            
            if (debug_mode) {
                fprintf(stderr, "Debug: Merged section '%s': size1=%zu, size2=%zu, total=%u\n",
                        section_name, size1, size2, out_section_headers[i].sh_size);
            }
            
        } else if (section_headers1[i].sh_type != SHT_NULL && section_headers1[i].sh_size > 0) {
            // Non-mergeable section - just copy from file 1
            void *section_data = map_start1 + section_headers1[i].sh_offset;
            size_t size = section_headers1[i].sh_size;
            
            if (write(out_fd, section_data, size) != (ssize_t)size) {
                fprintf(stderr, "Error: Failed to write section data\n");
                free(out_section_headers);
                if (resolved_symbols) free(resolved_symbols);
                close(out_fd);
                return;
            }
            
            if (debug_mode) {
                fprintf(stderr, "Debug: Copied section '%s': size=%zu\n", section_name, size);
            }
        }
    }
    
    // Step 4: Write the section header table at the end
    off_t shoff = lseek(out_fd, 0, SEEK_CUR);
    size_t sht_size = elf_header1->e_shnum * sizeof(Elf32_Shdr);
    
    if (write(out_fd, out_section_headers, sht_size) != (ssize_t)sht_size) {
        fprintf(stderr, "Error: Failed to write section header table\n");
        free(out_section_headers);
        if (resolved_symbols) free(resolved_symbols);
        close(out_fd);
        return;
    }
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Wrote section header table at offset %ld (%zu bytes)\n", 
                shoff, sht_size);
    }
    
    // Step 5: Update ELF header with correct e_shoff
    out_header.e_shoff = shoff;
    
    if (lseek(out_fd, 0, SEEK_SET) == -1) {
        fprintf(stderr, "Error: Failed to seek to beginning of file\n");
        free(out_section_headers);
        if (resolved_symbols) free(resolved_symbols);
        close(out_fd);
        return;
    }
    
    if (write(out_fd, &out_header, sizeof(Elf32_Ehdr)) != sizeof(Elf32_Ehdr)) {
        fprintf(stderr, "Error: Failed to update ELF header\n");
        free(out_section_headers);
        if (resolved_symbols) free(resolved_symbols);
        close(out_fd);
        return;
    }
    
    // Cleanup
    free(out_section_headers);
    if (resolved_symbols) free(resolved_symbols);
    close(out_fd);
    
    printf("Successfully merged files into 'out.ro'\n");
    printf("You can verify with: readelf -s out.ro\n");
    printf("You can link with: ld -m elf_i386 out.ro -o test_out\n");
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Merge complete with symbol resolution\n");
    }
}

void quit() {
    if (debug_mode) {
        fprintf(stderr, "Debug: Cleaning up and quitting\n");
    }
    
    // Clean up file 1
    cleanup_file(&fd1, &map_start1, &file_size1);
    
    // Clean up file 2
    cleanup_file(&fd2, &map_start2, &file_size2);
    
    printf("Quitting\n");
    exit(0);
}

void print_menu() {
    printf("\nChoose action:\n");
    for (int i = 0; menu[i].name != NULL; i++) {
        printf("%d-%s\n", i, menu[i].name);
    }
    printf("\n");
}

int main() {
    int choice;
    
    while (1) {
        print_menu();
        printf("Enter your choice: ");
        
        if (scanf("%d", &choice) != 1) {
            // Clear invalid input
            while (getchar() != '\n');
            printf("Invalid input\n");
            continue;
        }
        getchar(); // Clear newline
        
        // Check if choice is valid
        int valid = 0;
        for (int i = 0; menu[i].name != NULL; i++) {
            if (i == choice) {
                valid = 1;
                menu[i].func();
                break;
            }
        }
        
        if (!valid) {
            printf("Invalid choice\n");
        }
    }
    
    return 0;
}
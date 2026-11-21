#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>

// Global variables
char debug_mode = 0;
char display_mode = 0;  // 0 = hexadecimal, 1 = decimal
char file_name[128] = "";
int unit_size = 1;
unsigned char mem_buf[10000];
size_t mem_count = 0;

// Format arrays for printing
static char* hex_formats[] = {"%#hhx\n", "%#hx\n", "No such unit", "%#x\n"};
static char* dec_formats[] = {"%hhu\n", "%hu\n", "No such unit", "%u\n"};

// Function declarations
void toggle_debug_mode();
void set_file_name();
void set_unit_size();
void load_into_memory();
void toggle_display_mode();
void memory_display();
void save_into_file();
void memory_modify();
void quit();

// Menu structure
struct menu_item {
    char *name;
    void (*func)();
};

struct menu_item menu[] = {
    {"Toggle Debug Mode", toggle_debug_mode},
    {"Set File Name", set_file_name},
    {"Set Unit Size", set_unit_size},
    {"Load Into Memory", load_into_memory},
    {"Toggle Display Mode", toggle_display_mode},
    {"Memory Display", memory_display},
    {"Save Into File", save_into_file},
    {"Memory Modify", memory_modify},
    {"Quit", quit},
    {NULL, NULL}
};

// Function implementations
void toggle_debug_mode() {
    if (debug_mode) {
        debug_mode = 0;
        fprintf(stderr, "Debug flag now off\n");
    } else {
        debug_mode = 1;
        fprintf(stderr, "Debug flag now on\n");
    }
}

void set_file_name() {
    printf("Enter file name: ");
    fgets(file_name, 128, stdin);
    // Remove newline character
    file_name[strcspn(file_name, "\n")] = 0;
    
    if (debug_mode) {
        fprintf(stderr, "Debug: file name set to '%s'\n", file_name);
    }
}

void set_unit_size() {
    int size;
    printf("Enter unit size (1, 2, or 4): ");
    scanf("%d", &size);
    getchar(); // Clear newline from input buffer
    
    if (size == 1 || size == 2 || size == 4) {
        unit_size = size;
        if (debug_mode) {
            fprintf(stderr, "Debug: set size to %d\n", unit_size);
        }
    } else {
        fprintf(stderr, "Error: Invalid unit size. Must be 1, 2, or 4.\n");
    }
}

void load_into_memory() {
    // Check if file_name is empty
    if (strcmp(file_name, "") == 0) {
        fprintf(stderr, "Error: File name is empty\n");
        return;
    }
    
    // Open file for reading
    FILE *file = fopen(file_name, "r");
    if (file == NULL) {
        fprintf(stderr, "Error: Could not open file '%s'\n", file_name);
        return;
    }
    
    // Prompt user for location and length
    char input[256];
    int location;
    int length;
    
    printf("Please enter <location> <length>\n");
    fgets(input, sizeof(input), stdin);
    
    if (sscanf(input, "%x %d", &location, &length) != 2) {
        fprintf(stderr, "Error: Invalid input format\n");
        fclose(file);
        return;
    }
    
    // Debug output
    if (debug_mode) {
        fprintf(stderr, "Debug: file_name='%s', location=%#x, length=%d\n", 
                file_name, location, length);
    }
    
    // Calculate number of bytes to read
    size_t bytes_to_read = length * unit_size;
    
    // Check if it exceeds buffer size
    if (bytes_to_read > 10000) {
        fprintf(stderr, "Error: Requested size exceeds buffer limit\n");
        fclose(file);
        return;
    }
    
    // Seek to location
    if (fseek(file, location, SEEK_SET) != 0) {
        fprintf(stderr, "Error: Could not seek to location %#x\n", location);
        fclose(file);
        return;
    }
    
    // Read data into mem_buf
    size_t bytes_read = fread(mem_buf, 1, bytes_to_read, file);
    
    if (bytes_read != bytes_to_read) {
        fprintf(stderr, "Warning: Only read %zu bytes out of %zu\n", 
                bytes_read, bytes_to_read);
    }
    
    // Update mem_count
    mem_count = bytes_read;
    
    // Close file
    fclose(file);
    
    printf("Loaded %d units into memory\n", length);
}

void toggle_display_mode() {
    if (display_mode) {
        display_mode = 0;
        printf("Decimal display flag now off, hexadecimal representation\n");
    } else {
        display_mode = 1;
        printf("Decimal display flag now on, decimal representation\n");
    }
}

void memory_display() {
    char input[256];
    unsigned int addr;
    int u;
    
    printf("Enter address and length\n");
    fgets(input, sizeof(input), stdin);
    
    if (sscanf(input, "%x %d", &addr, &u) != 2) {
        fprintf(stderr, "Error: Invalid input format\n");
        return;
    }
    
    if (debug_mode) {
        fprintf(stderr, "Debug: addr=%#x, u=%d\n", addr, u);
    }
    
    // Calculate starting position in mem_buf
    unsigned char *start_ptr;
    if (addr == 0) {
        start_ptr = mem_buf;
    } else {
        start_ptr = (unsigned char*)addr;
    }
    
    // Print header based on display mode
    if (display_mode) {
        printf("Decimal\n");
        printf("=======\n");
    } else {
        printf("Hexadecimal\n");
        printf("===========\n");
    }
    
    // Display u units
    for (int i = 0; i < u; i++) {
        unsigned char *current_ptr = start_ptr + (i * unit_size);
        
        // Read the value based on unit_size
        if (unit_size == 1) {
            unsigned char val = *current_ptr;
            if (display_mode) {
                printf(dec_formats[0], val);
            } else {
                printf(hex_formats[0], val);
            }
        } else if (unit_size == 2) {
            unsigned short val = *(unsigned short*)current_ptr;
            if (display_mode) {
                printf(dec_formats[1], val);
            } else {
                printf(hex_formats[1], val);
            }
        } else if (unit_size == 4) {
            unsigned int val = *(unsigned int*)current_ptr;
            if (display_mode) {
                printf(dec_formats[3], val);
            } else {
                printf(hex_formats[3], val);
            }
        }
    }
}

void save_into_file() {
    // Check if file_name is empty
    if (strcmp(file_name, "") == 0) {
        fprintf(stderr, "Error: File name is empty\n");
        return;
    }
    
    // Prompt user for source-address, target-location, and length
    char input[256];
    unsigned long source_address;
    unsigned int target_location;
    int length;
    
    printf("Please enter <source-address> <target-location> <length>\n");
    fgets(input, sizeof(input), stdin);
    
    if (sscanf(input, "%lx %x %d", &source_address, &target_location, &length) != 3) {
        fprintf(stderr, "Error: Invalid input format\n");
        return;
    }
    
    // Debug output
    if (debug_mode) {
        fprintf(stderr, "Debug: file_name='%s', source_address=%#lx, target_location=%#x, length=%d\n",
                file_name, source_address, target_location, length);
    }
    
    // Determine source pointer
    unsigned char *source_ptr;
    if (source_address == 0) {
        source_ptr = mem_buf;
    } else {
        source_ptr = (unsigned char*)source_address;
    }
    
    // Open file for reading and writing (r+b = don't truncate)
    FILE *file = fopen(file_name, "r+b");
    if (file == NULL) {
        fprintf(stderr, "Error: Could not open file '%s'\n", file_name);
        return;
    }
    
    // Get file size to check if target_location is valid
    struct stat st;
    if (stat(file_name, &st) != 0) {
        fprintf(stderr, "Error: Could not get file size\n");
        fclose(file);
        return;
    }
    
    // Check if target_location exceeds file size
    if (target_location > st.st_size) {
        fprintf(stderr, "Error: Target location exceeds file size\n");
        fclose(file);
        return;
    }
    
    // Calculate number of bytes to write
    size_t bytes_to_write = length * unit_size;
    
    // Seek to target location
    if (fseek(file, target_location, SEEK_SET) != 0) {
        fprintf(stderr, "Error: Could not seek to target location %#x\n", target_location);
        fclose(file);
        return;
    }
    
    // Write data from source to file
    size_t bytes_written = fwrite(source_ptr, 1, bytes_to_write, file);
    
    if (bytes_written != bytes_to_write) {
        fprintf(stderr, "Warning: Only wrote %zu bytes out of %zu\n",
                bytes_written, bytes_to_write);
    }
    
    // Close file
    fclose(file);
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Wrote %zu bytes to file\n", bytes_written);
    }
}

void memory_modify() {
    char input[256];
    unsigned int location;
    unsigned int val;
    
    printf("Please enter <location> <val>\n");
    fgets(input, sizeof(input), stdin);
    
    if (sscanf(input, "%x %x", &location, &val) != 2) {
        fprintf(stderr, "Error: Invalid input format\n");
        return;
    }
    
    // Debug output
    if (debug_mode) {
        fprintf(stderr, "Debug: location=%#x, val=%#x\n", location, val);
    }
    
    // Check if location is valid given the unit size
    if (location + unit_size > sizeof(mem_buf)) {
        fprintf(stderr, "Error: Location exceeds memory buffer size\n");
        return;
    }
    
    // Check if location is within loaded memory
    if (mem_count > 0 && location + unit_size > mem_count) {
        fprintf(stderr, "Warning: Location exceeds loaded memory\n");
    }
    
    // Write the value to memory buffer based on unit_size
    if (unit_size == 1) {
        mem_buf[location] = (unsigned char)val;
    } else if (unit_size == 2) {
        *(unsigned short*)(&mem_buf[location]) = (unsigned short)val;
    } else if (unit_size == 4) {
        *(unsigned int*)(&mem_buf[location]) = val;
    }
    
    if (debug_mode) {
        fprintf(stderr, "Debug: Modified %d bytes at location %#x\n", unit_size, location);
    }
}

void quit() {
    if (debug_mode) {
        fprintf(stderr, "quitting\n");
    }
    exit(0);
}

void print_menu() {
    printf("Choose action:\n");
    for (int i = 0; menu[i].name != NULL; i++) {
        printf("%d-%s\n", i, menu[i].name);
    }
}

int main() {
    int choice;
    
    while (1) {
        // Print debug info if debug mode is on
        if (debug_mode) {
            fprintf(stderr, "unit_size=%d, file_name='%s', mem_count=%zu\n", 
                    unit_size, file_name, mem_count);
        }
        
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
        
        printf("\n");
    }
    
    return 0;
}

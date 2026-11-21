#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define BUFFER_SIZE 1024
#define SCAN_BUFFER_SIZE 10240  // 10K bytes
#define RET_INSTRUCTION 0xC3    // x86 near return instruction

// Virus structure
typedef struct virus {
    unsigned short SigSize;
    char virusName[16];
    unsigned char* sig;
} virus;

// Link structure
typedef struct link link;
struct link {
    link *nextVirus;
    virus *vir;
};

// Global variables
link* SignaturesList = NULL;
int isBigEndian = 0;

// ==================== Helper Functions ====================

unsigned short swapBytes(unsigned short value) {
    return (value << 8) | (value >> 8);
}

// ==================== Virus Functions ====================

virus* readVirus(FILE* file) {
    virus* v = (virus*)malloc(sizeof(virus));
    if (v == NULL) {
        return NULL;
    }
    
    // Read first 18 bytes (SigSize + virusName)
    if (fread(v, 1, 18, file) != 18) {
        free(v);
        return NULL;
    }
    
    // Handle endianness
    if (isBigEndian) {
        v->SigSize = swapBytes(v->SigSize);
    }
    
    // Allocate memory for signature
    v->sig = (unsigned char*)malloc(v->SigSize);
    if (v->sig == NULL) {
        free(v);
        return NULL;
    }
    
    // Read signature bytes
    if (fread(v->sig, 1, v->SigSize, file) != v->SigSize) {
        free(v->sig);
        free(v);
        return NULL;
    }
    
    return v;
}

void printVirus(virus* v, FILE* output) {
    if (v == NULL) return;
    
    fprintf(output, "Virus name: %s\n", v->virusName);
    fprintf(output, "Virus size: %d\n", v->SigSize);
    fprintf(output, "signature:\n");
    
    for (int i = 0; i < v->SigSize; i++) {
        fprintf(output, "%02X ", v->sig[i]);
    }
    fprintf(output, "\n\n");
}

// ==================== Linked List Functions ====================

void list_print(link *virus_list, FILE* output) {
    link* current = virus_list;
    
    while (current != NULL) {
        printVirus(current->vir, output);
        current = current->nextVirus;
    }
}

link* list_append(link* virus_list, virus* data) {
    link* new_link = (link*)malloc(sizeof(link));
    if (new_link == NULL) {
        return virus_list;
    }
    
    new_link->vir = data;
    new_link->nextVirus = NULL;
    
    // If list is empty, return new link as head
    if (virus_list == NULL) {
        return new_link;
    }
    
    // Find the end and append
    link* current = virus_list;
    while (current->nextVirus != NULL) {
        current = current->nextVirus;
    }
    current->nextVirus = new_link;
    
    return virus_list;
}

void list_free(link *virus_list) {
    link* current = virus_list;
    link* next;
    
    while (current != NULL) {
        next = current->nextVirus;
        
        if (current->vir != NULL) {
            free(current->vir->sig);
            free(current->vir);
        }
        
        free(current);
        current = next;
    }
}

// ==================== Menu Functions ====================

void LoadSignatures() {
    char filename[BUFFER_SIZE];
    char magic[5];
    FILE* file;
    
    // Free existing list if any
    if (SignaturesList != NULL) {
        list_free(SignaturesList);
        SignaturesList = NULL;
    }
    
    // Get filename from user
    printf("Enter signature file name: ");
    fgets(filename, BUFFER_SIZE, stdin);
    filename[strcspn(filename, "\n")] = 0;
    
    // Open file
    file = fopen(filename, "rb");
    if (file == NULL) {
        printf("Error: Cannot open file '%s'\n", filename);
        return;
    }
    
    // Read and check magic number
    if (fread(magic, 1, 4, file) != 4) {
        printf("Error: Cannot read magic number\n");
        fclose(file);
        return;
    }
    magic[4] = '\0';
    
    // Check magic number and set endianness
    if (strcmp(magic, "VIRL") == 0) {
        isBigEndian = 0;
        printf("Magic number: VIRL (little-endian)\n");
    } else if (strcmp(magic, "VIRB") == 0) {
        isBigEndian = 1;
        printf("Magic number: VIRB (big-endian)\n");
    } else {
        printf("Error: Invalid magic number '%s'\n", magic);
        fclose(file);
        return;
    }
    
    // Read viruses into linked list
    virus* v;
    int count = 0;
    while ((v = readVirus(file)) != NULL) {
        SignaturesList = list_append(SignaturesList, v);
        count++;
    }
    
    printf("Loaded %d signatures\n", count);
    fclose(file);
}

void PrintSignatures() {
    if (SignaturesList == NULL) {
        printf("No signatures\n");
        return;
    }
    
    list_print(SignaturesList, stdout);
}

void detect_virus(unsigned char* buffer, unsigned int size) {
    if (SignaturesList == NULL) {
        printf("No signatures loaded. Please load signatures first.\n");
        return;
    }
    
    link* current = SignaturesList;
    int detected = 0;
    
    // Scan through the buffer
    while (current != NULL) {
        virus* v = current->vir;
        
        // Compare virus signature with buffer contents
        for (unsigned int i = 0; i <= size - v->SigSize; i++) {
            if (memcmp(buffer + i, v->sig, v->SigSize) == 0) {
                printf("Virus detected:\n");
                printf("Starting byte location: %u\n", i);
                printf("Virus name: %s\n", v->virusName);
                printf("Virus signature size: %d\n\n", v->SigSize);
                detected = 1;
            }
        }
        
        current = current->nextVirus;
    }
    
    if (!detected) {
        printf("No viruses detected.\n");
    }
}

void DetectViruses() {
    char filename[BUFFER_SIZE];
    unsigned char buffer[SCAN_BUFFER_SIZE];
    FILE* file;
    size_t bytesRead;
    
    if (SignaturesList == NULL) {
        printf("No signatures loaded. Please load signatures first.\n");
        return;
    }
    
    // Get suspected file name
    printf("Enter suspected file name: ");
    fgets(filename, BUFFER_SIZE, stdin);
    filename[strcspn(filename, "\n")] = 0;
    
    // Open file
    file = fopen(filename, "rb");
    if (file == NULL) {
        printf("Error: Cannot open file '%s'\n", filename);
        return;
    }
    
    // Read file into buffer (up to 10K)
    bytesRead = fread(buffer, 1, SCAN_BUFFER_SIZE, file);
    printf("Scanning %zu bytes...\n", bytesRead);
    
    // Detect viruses
    detect_virus(buffer, bytesRead);
    
    fclose(file);
}

// ==================== Part 2b: Fix File Implementation ====================

void neutralize_virus(char *fileName, int signatureOffset) {
    FILE* file;
    unsigned char ret_byte = RET_INSTRUCTION;
    
    // Open file in read-write binary mode
    file = fopen(fileName, "r+b");
    if (file == NULL) {
        fprintf(stderr, "Error: Could not open file %s for writing\n", fileName);
        return;
    }
    
    // Seek to the virus location
    if (fseek(file, signatureOffset, SEEK_SET) != 0) {
        fprintf(stderr, "Error: Could not seek to offset %d\n", signatureOffset);
        fclose(file);
        return;
    }
    
    // Write RET instruction to neutralize the virus
    if (fwrite(&ret_byte, 1, 1, file) != 1) {
        fprintf(stderr, "Error: Could not write to file at offset %d\n", signatureOffset);
        fclose(file);
        return;
    }
    
    printf("Virus neutralized at byte offset %d\n", signatureOffset);
    fclose(file);
}

void FixFile() {
    char filename[BUFFER_SIZE];
    unsigned char buffer[SCAN_BUFFER_SIZE];
    FILE* file;
    size_t bytesRead;
    link* current;
    int virusCount = 0;
    
    // Check if signatures are loaded
    if (SignaturesList == NULL) {
        printf("No signatures loaded. Please load signatures first.\n");
        return;
    }
    
    // Get suspected file name
    printf("Enter suspected file name: ");
    fgets(filename, BUFFER_SIZE, stdin);
    filename[strcspn(filename, "\n")] = 0;
    
    // Open file for reading
    file = fopen(filename, "rb");
    if (file == NULL) {
        printf("Error: Cannot open file '%s'\n", filename);
        return;
    }
    
    // Read file into buffer (up to 10K)
    bytesRead = fread(buffer, 1, SCAN_BUFFER_SIZE, file);
    fclose(file);
    
    if (bytesRead == 0) {
        printf("File is empty or could not be read.\n");
        return;
    }
    
    printf("Scanning and fixing file %s (%zu bytes)...\n", filename, bytesRead);
    
    // Scan for viruses and neutralize them
    current = SignaturesList;
    while (current != NULL) {
        virus* v = current->vir;
        
        // Compare virus signature with buffer contents
        for (unsigned int i = 0; i <= bytesRead - v->SigSize; i++) {
            if (memcmp(buffer + i, v->sig, v->SigSize) == 0) {
                // Virus detected!
                printf("\nVirus detected:\n");
                printf("Starting byte location: %u\n", i);
                printf("Virus name: %s\n", v->virusName);
                printf("Virus signature size: %d\n", v->SigSize);
                
                // Neutralize the virus
                neutralize_virus(filename, i);
                virusCount++;
                
                // Update buffer to reflect the change (prevent re-detection)
                buffer[i] = RET_INSTRUCTION;
            }
        }
        
        current = current->nextVirus;
    }
    
    if (virusCount == 0) {
        printf("No viruses detected.\n");
    } else {
        printf("\nTotal viruses neutralized: %d\n", virusCount);
    }
}
// ==================== Part 2c: AI Analysis (Stub) ====================

void AIAnalysis() {
    printf("Not implemented yet\n");
}

void Quit() {
    if (SignaturesList != NULL) {
        list_free(SignaturesList);
    }
    printf("Quitting program\n");
    exit(0);
}

// ==================== Main Menu ====================

int main() {
    char input[256];
    int choice;
    
    while (1) {
        printf("1) Load signatures\n");
        printf("2) Print signatures\n");
        printf("3) Detect viruses\n");
        printf("4) Fix file\n");
        printf("5) AI analysis of file\n");
        printf("6) Quit\n");
        printf("\nSelect one of the above options: ");
        
        fgets(input, sizeof(input), stdin);
        sscanf(input, "%d", &choice);
        
        switch (choice) {
            case 1:
                LoadSignatures();
                break;
            case 2:
                PrintSignatures();
                break;
            case 3:
                DetectViruses();
                break;
            case 4:
                FixFile();
                break;
            case 5:
                AIAnalysis();
                break;
            case 6:
                Quit();
                break;
            default:
                printf("Invalid option\n");
        }
        
        printf("\n");
    }
    
    return 0;
}
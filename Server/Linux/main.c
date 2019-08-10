#include <stdio.h>
#include <stdlib.h>
#include <xdo.h>
#include <unistd.h>
#include <netdb.h> 
#include <netinet/in.h> 
#include <string.h> 
#include <sys/socket.h> 
#include <sys/types.h>

#define MAX 80 
#define PORT 54001
#define SA struct sockaddr

const char* VK_MEDIA_PLAY_PAUSE = "XF86AudioPlay";
const char* VK_MEDIA_NEXT_TRACK = "XF86AudioNext";
const char* VK_MEDIA_PREV_TRACK = "XF86AudioPrev";
const char* VK_VOLUME_DOWN      = "XF86AudioLowerVolume";
const char* VK_VOLUME_UP        = "XF86AudioRaiseVolume";
const char* VK_VOLUME_MUTE      = "XF86AudioMute";

void keybd_event(xdo_t* xdo, const char* sequence){
    xdo_send_keysequence_window(xdo, CURRENTWINDOW, sequence, 0);
}

void acceptClient(int sockfd){
    xdo_t* xdo;
    char buff[MAX];
    int n;

    bzero(buff, MAX);
    

    if(recv(sockfd, buff, sizeof(buff), 0) == -1){
        // responds null

        bzero(buff, MAX);

        write(sockfd, "INVALID REQUEST", 17UL);

        return;
    }
    printf("Recv: %i - %s\n", n, buff);

    xdo = xdo_new(NULL);

    if(strstr(buff, "VK_MEDIA_PLAY_PAUSE"))
        keybd_event(xdo, VK_MEDIA_PLAY_PAUSE);
    else if (strstr(buff, "VK_MEDIA_NEXT_TRACK"))
        keybd_event(xdo, VK_MEDIA_NEXT_TRACK);
    else if(strstr(buff, "VK_MEDIA_PREV_TRACK"))
        keybd_event(xdo, VK_MEDIA_PREV_TRACK);
    else if(strstr(buff, "VK_VOLUME_DOWN"))
        keybd_event(xdo, VK_VOLUME_DOWN);
    else if (strstr(buff, "VK_VOLUME_UP"))
        keybd_event(xdo, VK_VOLUME_UP);
    else if(strstr(buff, "VK_VOLUME_MUTE"))
        keybd_event(xdo, VK_VOLUME_MUTE);
}

int main() {    
    int sockfd, connfd, len; 
    struct sockaddr_in servaddr, cli; 
  
    sockfd = socket(AF_INET, SOCK_STREAM, 0); 
    if (sockfd == -1) { 
        printf("socket creation failed...\n"); 
        exit(0); 
    } 
    else
        printf("Socket successfully created..\n"); 
    bzero(&servaddr, sizeof(servaddr)); 
  
    servaddr.sin_family = AF_INET; 
    servaddr.sin_addr.s_addr = htonl(INADDR_ANY); 
    servaddr.sin_port = htons(PORT); 
  
    if ((bind(sockfd, (SA*)&servaddr, sizeof(servaddr))) != 0) { 
        printf("socket bind failed...\n"); 
        exit(0); 
    } 
    else
        printf("Socket successfully binded..\n"); 
  
    listen(sockfd, 5);

    printf("Server listening..\n"); 

    len = sizeof(cli); 

    while(True){
        connfd = accept(sockfd, (SA*)&cli, &len);

        if(connfd == -1)
            break;
    
        acceptClient(connfd); 
    
        close(connfd); 
    }
    
    close(sockfd);

    return 0;
}
#include <iostream>
using namespace std;
#include "../include/ConnectionHandler.h"

#include <vector>
#include "../include/StompProtocol.h"
#include <thread>

void ReadFromSocket(StompProtocol &protocol, ConnectionHandler &handler,bool &terminate){
    
    string frameFromServer;
    while(!terminate){
        frameFromServer ="";
		handler.getLine(frameFromServer);
		string output = protocol.MessageFromServer(frameFromServer);   
        vector<string> splitframe = protocol.split(frameFromServer,"\n");
        if(splitframe[0]== "MESSAGE")  {
          continue;
        }
        cout << output << endl;
        if(output=="Logout successful")
        {
            terminate=true;
            handler.close();
        }
    }
}

int main(int argc, char *argv[]) {
	// TODO: implement the STOMP client

    cout << "the cleint is runnnig" << endl;

   while(true){
 
    bool terminate=false;
    
    std::string line;
    bool loggedIn = false;
    StompProtocol protocol;
    ConnectionHandler connectionHandler(protocol.host, protocol.port);

    while(!loggedIn){
		getline(cin, line);
        string frame = protocol.MessageToServer(line);
        string frameFromServer="";
        connectionHandler.host_ = protocol.host;
        connectionHandler.port_ = protocol.port;
	    if(protocol.split(line," ")[0]=="login"){	
        if (!connectionHandler.connect()) {
            std::cerr << "Cannot connect to " << protocol.host << ":" << protocol.port << std::endl;
            continue;
        }
        connectionHandler.sendLine(frame);
        connectionHandler.getLine(frameFromServer);
		string output = protocol.MessageFromServer(frameFromServer);
        cout << output << endl;
        if(output!="Login successful"){
            connectionHandler.close();
            continue;
        }
        loggedIn = true;

    }else {cout<<"should log in first"<<endl; continue;}

    thread reader(&ReadFromSocket, ref(protocol), ref(connectionHandler),ref(terminate)); // i think it should be inside the loop 

    while(loggedIn && !terminate){           // i add logeedin!
		getline(cin, line);
        
        if((protocol.split(line," ")[0]=="login") & (protocol.loggedIn)){
        cout << "The client is already logged in,log out before trying again" << endl;
        continue;
        }

        if(protocol.split(line," ")[0]=="report"){
            size_t pos = line.find(" ");
            string jsonfile=line.substr(pos+1);
            vector<string> myEvents = protocol.sendEvents(jsonfile);
            if(myEvents.size()>0){
            for(string event : myEvents){
               connectionHandler.sendLine(event);
            }
            continue;
        }
        else {
            cout << "you are not subscribed to this channel, you cant report "<< endl;
            continue;
        }
        }
        string frame = protocol.MessageToServer(line);
        if(frame == "You are already subscribed to this channel"){
            cout<<frame<< endl;
            continue;
        }

        if(line=="summary")    // change here to: frame -> line 
          continue;
		

		if(!connectionHandler.sendLine(frame)){
        cout << "Disconnected. Exiting...\n" << endl;
            break;
        }
        if(line=="logout"){
            reader.join();
            break;
        }
	}

   }
	return 0;
    
   
}
}
#pragma once

#include "../include/ConnectionHandler.h"
#include <iostream>
#include <string>
#include <boost/asio.hpp>
using boost::asio::ip::tcp;
using namespace std;
#include <map>
#include "event.h"
// TODO: implement the STOMP protocol
class StompProtocol
{
private:

public:
    map<int,string> receiptidtoFrame; // reciptID -> frames
    map<string,int> ChanneltoSubid; // chaneel -> id for the cleint to the chaneel 
    map<int,string> SubidtoChannel; // id for the cleint to the chaneel -> chaneel
    map<string,map<string,vector<Event>>> usersEvents; // <chaneel , < users-client ,events> >
    vector<Event> emergencyupdate ; // my emergencyupdate
    int subId;     
    int receiptId; 
    bool loggedIn;
    string myUser;
    string currChannel;   
    string host;
    short port;

    StompProtocol();
    string MessageToServer(string msg);
    string MessageFromServer(string frame);
    vector<string> split (string s, string delimiter);
    vector<string> sendEvents(string file);
    string summarytostring(const Event e);
    std::string getusername();
    std::string trim(const std::string &str);

 
 
};

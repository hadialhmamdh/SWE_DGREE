#include "../include/StompProtocol.h"
#include <iostream>
#include <fstream>


// constructor
StompProtocol::StompProtocol()
    : receiptidtoFrame(),      
      ChanneltoSubid(),        
      SubidtoChannel(),      
      usersEvents(),           
      emergencyupdate(),       
      subId(0),               
      receiptId(0),
      loggedIn(false),       
      myUser(""),              
      currChannel(""),
      host(""),
      port(0)                  
{}
std::string StompProtocol::trim(const std::string &str)
{
  size_t first = str.find_first_not_of(' ');
  if (first == std::string::npos)
    return "";
  size_t last = str.find_last_not_of(' ');
  return str.substr(first, (last - first + 1));
}
//axullary function
std::string StompProtocol::summarytostring(const Event e) {
    std::string result ="";
    result += "city:" + e.get_city();
    result += "\ndate time:" + std::to_string(e.get_date_time());
    result += "\nevent name:" + e.get_name();
    result += "\nsummary:" + e.get_description();
    return result;
}
//----------------------------------------------------------------------------------------------------------------------------------------------------
//splitting the user string line comand into a vector.
vector<string>  StompProtocol::split (string s, string delimiter) {     
    size_t pos_start = 0, pos_end, delim_len = delimiter.length();
    string token;
    vector<string> res;

    while ((pos_end = s.find (delimiter, pos_start)) != string::npos) {
        token = s.substr (pos_start, pos_end - pos_start);
        pos_start = pos_end + delim_len;
        res.push_back (token);
    }
    res.push_back (s.substr (pos_start));
    return res;
}

//----------------------------------------------------------------------------------------------------------------------------------------------------
//gets a msg from the keyboard(user), converts it to a proper frame(to send it to the server).
string StompProtocol::MessageToServer(string msg){ 
vector<string> splitmsg = split(msg," ");
string cases=splitmsg[0];
string frame="";    

// login
if(cases=="login"){
  if(!loggedIn)
   {frame="CONNECT\naccept-version:1.2\nhost:stomp.cs.bgu.ac.il\nlogin:"+splitmsg[2]+"\npasscode:"+splitmsg[3]+"\n";
   myUser=splitmsg[2];
   host=splitmsg[1].substr(0,splitmsg[1].find(":"));
   string strport=splitmsg[1].substr(splitmsg[1].find(":")+1);
   port=stoi(strport);
   }
 }
 //join
  if(cases=="join") {
  std::string cannelname = splitmsg[1];
  bool isSubscribed=false;
  for (map<string,int>::iterator ii=ChanneltoSubid.begin();ii!=ChanneltoSubid.end()&&!isSubscribed;++ii){
      if ((*ii).first==cannelname)
      isSubscribed = true;
    }
  if(!isSubscribed){
  frame="SUBSCRIBE\ndestination:"+splitmsg[1]+"\nid:"+to_string(subId)+"\nreceipt:"+to_string(receiptId);
  receiptidtoFrame[receiptId]=frame;
  ChanneltoSubid[splitmsg[1]]=subId;
  SubidtoChannel[subId]=splitmsg[1];
  subId++;
  receiptId++;
 }else{
    frame = "You are already subscribed to this channel";
  }
 }
 //exit
 if(cases=="exit"){
  bool isSubscribed=false;
  for (map<string,int>::iterator ii=ChanneltoSubid.begin();ii!=ChanneltoSubid.end()&&!isSubscribed;++ii){
    if((*ii).first==splitmsg[1])
       isSubscribed=true;
    }
   int id=-1;
   if(isSubscribed){
   id=ChanneltoSubid[splitmsg[1]];
   }
   frame="UNSUBSCRIBE\nid:"+to_string(id)+"\nreceipt:"+to_string(receiptId)+"\n";
   receiptidtoFrame[receiptId]=frame;
   receiptId++;
}

//summary 
if(cases=="summary"){

bool isfound=false;
string chaneel = splitmsg[1];
string username = splitmsg[2];
string filePath=splitmsg[3];
bool isSubscribed = false; 
for (map<string,int>::iterator ii=ChanneltoSubid.begin();ii!=ChanneltoSubid.end()&&!isSubscribed;++ii){
      if ((*ii).first==chaneel)
      isSubscribed = true;
    }
  if(isSubscribed){
     std::map<string,vector<Event>> bb = usersEvents[chaneel];
     vector<Event> gg = bb[username];
     if(gg.size()>0)
      isfound = true;
  if(isfound){
  map<string, vector<Event>> eventsuser = usersEvents[chaneel];
  vector<Event> events = eventsuser[username];
  int sumofreports = events.size();
  int sumofactivereports = 0;
  int sumofFAAS = 0;
  for(Event E : events){
      const map<string, string> &general_info = E.get_general_information();
    for (auto it = general_info.begin(); it != general_info.end(); ++it){
        if (trim(it->first) == "active"){
           if (trim(it->second) == "true" || trim(it->second) == "1"){
              sumofactivereports++;
              }
            }
     else
       if (trim(it->first) == "forces_arrival_at_scene"){
        if (trim(it->second) == "true" || trim(it->second) == "1"){
           sumofFAAS++;
            }
        }
      }
  }

  
    frame += "Channel" + chaneel + "\nStats:\nTotal:" + to_string(sumofreports) + 
         " \nactive:" + to_string(sumofactivereports) + "\nforces arrival at scene:"  
          +to_string(sumofFAAS) + "\nEvent reports:\n";

  int i = 1; 
  for(Event ee : events){
    frame += "Report_" +to_string(i)+ ":\n" + summarytostring(ee) +"\n\n";
    i = i + 1 ;
  }
  std::ofstream outfile(filePath);
  outfile << frame ;
  outfile.close();
  }
  else{
    cout << "there is no reports for this user to this channel" <<endl;
  }
  }
  else{
    cout << "you are not subscribed to this channel." <<endl;
  }
}
//logout
if(cases=="logout"){
  if(loggedIn){
   frame="DISCONNECT\nreceipt:"+to_string(receiptId)+"\n";
   receiptidtoFrame[receiptId]=frame;
   receiptId++; 
   }
  else{
    cout <<"login before you logout"<< endl ;                        
   }
}
return frame;
}


// if the user command was report
 vector<string> StompProtocol::sendEvents(string file){
    vector<string> result;
    names_and_events aa = parseEventsFile(file); 
    string chaneelname = aa.channel_name;
    vector<Event> events = aa.events;
    string frametosend;
    bool isSubscribed = false; 
   for (map<string,int>::iterator ii=ChanneltoSubid.begin();ii!=ChanneltoSubid.end()&&!isSubscribed;++ii){
      if ((*ii).first==chaneelname)
      isSubscribed = true;
    }
    if(isSubscribed){
    for(Event e : events){
        frametosend = "";
        string frametosend = "SEND\ndestination:" + chaneelname + "\n\nuser:" + getusername() + 
        "\ncity:" + e.get_city() + "\nevent name:" + e.get_name() + "\ndate time:" + std::to_string(e.get_date_time()) + 
        "\ngeneral information:\n";
        for (auto &update : e.get_general_information()){
           string keytoadd = update.first;
           string valuetoadd = update.second;
           frametosend +="    " + keytoadd + ":" + valuetoadd + "\n";  
        }      
        frametosend += "description:\n" + e.get_description();
        result.push_back(frametosend);
    }
    return result;
}
    else return result;
}
std::string StompProtocol::getusername(){return myUser;}



//-------------------------------------------------------------------------------------------------------------------------------------
//gets a frame from the server, converts it to a proper msg(to prints it).
string StompProtocol::MessageFromServer(string frame){ 
  string msg="";
  vector<string> splitframe = split(frame,"\n");
  string cases=splitframe[0];

  //connected 
  if(cases=="CONNECTED"){
  if(!loggedIn){
  loggedIn=true;
  msg="Login successful";
  }
  }

  //receipt
  if(cases=="RECEIPT"){
    string receipt=splitframe[1].substr(splitframe[1].find(":")+1);
    int receipt_id=stoi(receipt);
    string myframe=receiptidtoFrame[receipt_id];
    vector<string> splitted = split(myframe,"\n");
    string mycase=splitted[0];

    if(mycase=="SUBSCRIBE"){
      string id=splitted[2].substr(splitted[2].find(":")+1);
      int sub_id=stoi(id);
      string mychannel=SubidtoChannel[sub_id];
      msg="Joined channel "+mychannel;
      currChannel=mychannel;                    
      vector<Event> myevents;                   
      usersEvents[mychannel][myUser]=myevents;   // change myuser<->myuser
    } 

    if(mycase=="UNSUBSCRIBE"){
      string id=splitted[1].substr(splitted[1].find(":")+1);
      int sub_id=stoi(id);
      string mychannel=SubidtoChannel[sub_id];
      SubidtoChannel.erase(sub_id);
      ChanneltoSubid.erase(mychannel);
      msg="Exited channel "+mychannel;
    }

    if(mycase=="DISCONNECT") {
      msg="Logout successful";
      loggedIn=false;
    }
}

//message
if(cases=="MESSAGE"){
  Event recevedevent = Event(frame);
  string eventowner = recevedevent.getEventOwnerUser();
  string tobic = recevedevent.get_channel_name();
  string eventname = recevedevent.get_name();
  usersEvents[tobic][eventowner].push_back(recevedevent);  
}

//error
   if(cases=="ERROR"){
    
      string errorname=splitframe[1].substr(9);
      if(errorname=="User already logged in")
        cout<<errorname;
      if(errorname=="Wrong password")
        cout<<errorname;
      else{                // i add that
        msg = frame; 
      }
    }

 return msg;
}
// mvn exec:java -Dexec.mainClass="bgu.spl.net.impl.stomp.StompServer" -Dexec.args="4444 reactor"   


# Before Our Time Business Logic

That means the game

Before Our Time provides a small framework for autonomous javascript objects 
to communicate with each other. Clients will be able to connect with the
server via telnet, websockets, or other technology. Each client (terminal)
appropriates one javascript object as it's avatar in order to interface
with the other objects. One day it might become a text adventure game.

## Requirements

* [Before Our Time Repository](https://github.com/delphian/before-our-time-repository)
* Microsoft.AspNetCore.App 2.0.6 (Net Core SDK >=2.0.0)

## Installation

* `git clone git@github.com:delphian/before-our-time-business.git`
* `cd before-our-time-business`
* `cp appsettings.json.sample appsettings.json`
* Modify the defaultConnection value in appsettings.json to reflect the path to the SQLiteDatabase.db file located in the BeforeOurTime.Repository repository

## Code Overview

### IORequests

All requests to the environment are made through IORequest objects:

* Telnet Client > Terminal > IOManager > IOHandler > API

#### Clients
Each client is specially coded to handle connections from a specific protocol.
As an example, the telnet client interprets free form textual input from port
23 and packages it into an IORequest object. The IORequest is then forwarded 
to the Terminal.

#### Terminals
A terminal is responsible for associating an account, character, and a client's
connection status. IO code, and the rest of the environment api, know nothing 
of clients, but do understand terminals. The primary purpose of a terminal is 
to abstract actual client connects, be they through telnet, websockets, 
signalR, etc.

#### IOManager
The IOManager is a message delivery service. It uses reflection at startup to 
register any classes that wishes to handle a particular message type (A child 
class of IORequest). During runtime the IORequest is forwarded to the
classes that have registed for it' type.

#### IOHandler
Each IOHandler is responsible for interpreting the details of a terminal
request, communicating with the API to gather information or possible
restrictions against the request, and sending an IOUpdate in answer back
to the terminal.

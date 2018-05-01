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


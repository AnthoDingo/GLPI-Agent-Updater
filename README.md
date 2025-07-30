# GLPI Agent Updater

This tools allow to auto update GLPI Agent throught Github official Release or from own GLPI task.

## Requirements

- [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Features:

- Client
    - Global
        - [X] Set time bewteen each scan
        - [X] Targeted version
        - [ ] Multi-plateform support. 
            - [X] Windows
            - [ ] Linux
            - [ ] MacOS
    - Github (Update official release)
        - [X] Windows
        - [ ] Linux
        - [ ] MacOS        
    - SMB Share (Update from network share)
        - [X] Windows
        - [ ] Linux
        - [ ] MacOS
    - GLPI Server (Get package from server)
        - [ ] Windows
        - [ ] Linux
        - [ ] MacOS
- Server (on GLPI server)
    - [ ] Get glpiinventory packages
    - [ ] Define package for each plateform

## Install
### Using Setup

By launching setup with the graphical, you will have options between Github and Server. Second option is not implemented in the service. Please use only Github now.

### Command line
```cmd
GLPI_Agent_Updater_{version}_x64.exe /VERYSILENT /MODE=0 [/VERSION="Latest"]

GLPI_Agent_Updater_{version}_x64.exe /VERYSILENT /MODE=1 /PATH="\\server\sharedfolder" [/VERSION="Latest"]
```

## FAQ

#### Why drop 32 bits support ?
GLPI Agent exist as 32 bits. However, most system on now 64 bits and Microsoft has annonce that in the futur, the 32 bits support will be drop too as Operating system. To maintain a 32 bits support may require efforts.

#### Why create a support for Linux and MacOS ?
GLPI Agent exist for this plateform. The main target is to create a tool to help Sys Admin to update the agent and to maintain security up to date.

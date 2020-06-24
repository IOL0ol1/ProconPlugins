# PRoCon Plugins

## Description

This is a PRoCon plug-in project solution.

**ConsoleApp1** for some test code.     
**ExampleCollection** some other's example that's not mine.    
**FriendManager** not completed.    
**RemoteManager** can execute remote powershell or cmd commends through the local host    
**ServerLogger** can record all player soldiername,eagudi,pbguid,ipaddress,country that enters the server    
**AutoIdleTime** can auto enable idleTimeout DefaultTimeout when player count >= Threshold     


## How to use project template

1. Copy PRoConPlugoin.zip to %HOMEPATH%\Documents\Visual Studio xxxx\Templates\ProjectTemplates.
2. Open visual studio and create "PRoCon Library" project.

## Some Event Trigger
trigger|event
-|-
admin.killPlayer | OnPlayerKilled and OnPlayerKilledByAdmin
!mm??? | OnPlayerTeamChange
admin.movePlayer | OnPlayerMovedByAdmin and OnPlayerTeamChange
player.isAlive | OnPlayerIsAlive
LevelLoaded | OnLevelLoaded
RoundOver | OnRoundOver OnRoundOverPlayers OnRoundOverTeamScores
TeamID | Left:1 Right:2


## Event Index

OnPlayerAuthenticated   
OnPlayerJoin   
OnPlayerDisconnected   
OnPlayerLeft   
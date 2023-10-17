# Dracon.Project.IronWhisper
IronWhisper is an end-to-end AI assistant tool aimed at creating ease of access for high-frequency routines. 

AI Steps: 
1. Voice Input. This uses whisper.cpp, a ggerganov library for parsing OpenAI Whisper models in a GGML format.
2. NLP. Currently implemented but unused, this is a spacy NLP parsing layer.
3. Processor. This is the 'Central Command' layer that takes the input and feeds it to various actions to be acted upon
4. TTS. Using the Mimic3 TTS tool, the AI responds verbally to the user.

# TODO's

Update REST client with new log events
- Requires either SSE or WebSocket implementation
- GenHTTP has no WebSocket implementation right now, so it either needs to be on top or wait for that
  
API
- Create a data structure for instructions etc to be fed to devices.
- Return this structure as part of the response to the device update call
- Create a new endpoint to consume part of, or all, of the data, allowing a device to confirm that it has received it.
- Idea; I need a device to response quickly to certain events. Aka, if a timer is set for 3 minutes, and the device checks every 5, it wont work.
    - What if i combine the http approach with a TCP ping? The device is already registering its IP via an update, the controller can connect, advise of need to update, and disconnect. 
  
Client instance 
- add 'client' mode to the controller. It will act as a node, or transfer station for commands and actions.
- Example flow,
  - Terminal A gives input of 'test input'. This input is fed into the client node, which sends it to the main controller.
  - The main controller determines the appropriate action, and sends the instruction back to the node.
  - The node executes the instruction.
- This method can lead into the following flow:
  - A central controller, which has access to all database content, and makes all core decisions
  - Node controllers, which report local input and data to the central controller
  - Input modules (aka Terminals), which provide local input to the nearest / associated node
  - Output modules, such as speakers, screens, lights, etc, which can be instructed via the associated node.

The above is a good flow when considered from the perspective of a system that operates both inside and outside a home. However, it forms issues of when to use local sockets vs rest api calls, and does not solve issues regarding the identification of which output devices should be used to effectively communicate to the user. 

It also doesnt solve the problem of system rigidity regarding multiple users. 

Therefore, the following becomes its own issue to consider: 

User Identification
- Locate user and use location to determine the appropriate method for communication.
  - User at desk means screen popups, or speaker output, can be used
  - User on couch means that speakers must be used
  - Unless user is on phone, then a notification could be sent

 - Refactor system to support multiple users.
   - Tie devices to a user. E.g, a terminal should report the user entering input
   - Create shared data storage/operation areas for each user. User B should not be affected by User A's timer, and User A shouldnt be able to access project data from User B.
   - This does lie in contrast to the current god-level administrator approach to the project, so perhaps a permissions system too.
     
Mimic3
 - registry: add wavs to project, linnk to enum and add calls to cached speak method
 - "get online" action: create wavs, link and call, add dynamic call links

Device Registration Refactor
- change device registration from UDP Broadcast to API reference
- Terminals provide input through api instead of local socket

Unity Integration
- Post registration refactor
- Add 'live interfaces' to the registry
- Interface identifies itself via api
- Live Interface Type 1: Game
  - Game can provide non-terminal input
  - Game can request configuration files
- Live Interface Type 2: Editor
  - Registered projects can expose editor configs
  - Controller can serve often implemented packages or scripts
    
# Ideas
- Stream audio to device through TCP port. E.g, play TTS on phone, or wireless speaker.
- Create a scoped package repository accessible through controller?
  
# Notes

to get into mimic, navigate to directory, then say "source .venv/bin/activate" to activate the virtual environment
Good voices: 
english (US) - cmu-artic_low - jmk - gka
english (UK) - apope/low

https://stephango.com/flexoki

Resource Registry / Permissions
- Track projects, files and other resources
- Track access points (targets that can run operations)
- Track input points (targets that receive command inputs)
- Map input points to access points to ensure context
- Tag system that assigns capabilities and systems associated with resources
- Modular data association combined with tags. E.g, HasDataStorage+HasProjectData will have associated ProjectData which contains a json structure with a list of projects, file locations etc).
- Systems that have storage and internet capacity will have structures for global download location etc
- Operations will also be modular, such as DownloadGlobal which will download a file through the supplied URI to the global download location. This is compared to DownloadRelative, which would download to the current 'focus' location, but thats more difficult. 
- Each 



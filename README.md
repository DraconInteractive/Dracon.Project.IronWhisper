# Dracon.Project.IronWhisper
IronWhisper is an end-to-end AI assistant tool aimed at creating ease of access for high-frequency routines. 

AI Steps: 
1. Voice Input. This uses whisper.cpp, a ggerganov library for parsing OpenAI Whisper models in a GGML format.
2. NLP. Currently implemented but unused, this is a spacy NLP parsing layer.
3. Processor. This is the 'Central Command' layer that takes the input and feeds it to various actions to be acted upon
4. TTS. Using the Mimic3 TTS tool, the AI responds verbally to the user.

# TODO's

Mimic3
 - registry: add wavs to project, linnk to enum and add calls to cached speak method
 - "get online" action: create wavs, link and call, add dynamic call links

Device Registration Refactor
- Create rest api interface in central controller
- Expose api tunnel and port using NGROK
- change device registration from UDP Broadcast to API reference
- Terminals provide input through api instead of local socket
  
# Ideas
- Stream audio to device through TCP port. E.g, play TTS on phone, or wireless speaker.
- 
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



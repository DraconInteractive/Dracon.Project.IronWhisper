#Iron Whisper

This is a fork of ggorganov' whisper.cpp library located here: 

IronWhisper is an end-to-end AI assistant tool aimed at creating ease of access for high-frequency routines. 

The plan! 
1. Voice transcription via Whisper 
2. Transmit transcription to a C# reciever
3. Feed transcription through a spacy model trained for text classification to output key commands
4. Feed spacy output into an action processor
5. Act on whatever the command was, and return back a 'status'
6. Feed action and status into a LLAMA model to get a 'human' response
7. TTS the response back to the user

Status: 
Steps 1&2 have gone well! While im looking into the viablility of using spacy for command extraction, i'm using more strict keywording where each command has an array of possible inputs, and will only accept one of those. 
Work on the action processor has begun as part of that, and thats about where im up to. 

Notes:

Resource Registry
- Track projects, files and other resources
- Track access points
- Tag system that assigns capabilities and systems associated with resources
- Modular data association combined with tags. E.g, HasDataStorage+HasProjectData will have associated ProjectData which contains a json structure with a list of projects, file locations etc).
- Systems that have storage and internet capacity will have structures for global download location etc
- Operations will also be modular, such as DownloadGlobal which will download a file through the supplied URI to the global download location. This is compared to DownloadRelative, which would download to the current 'focus' location, but thats more difficult. 


TTS:
http://festvox.org/festival/

Note to self: annotation
label-studio start
localhost:8080

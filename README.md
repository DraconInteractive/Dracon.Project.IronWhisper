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


Note to self: spacy TODO
https://catherinebreslin.medium.com/text-classification-with-spacy-3-0-d945e2e8fc44

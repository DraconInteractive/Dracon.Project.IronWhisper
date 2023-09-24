// Voice assistant example
//
// Speak short text commands to the microphone.
// This program will detect your voice command and convert them to text.
//
// ref: https://github.com/ggerganov/whisper.cpp/issues/171
//

#include "common-sdl.h"
#include "common.h"
#include "whisper.h"
#include "NonBlockingTCPServer.h"

#include <iostream>
#include <sstream>
#include <cassert>
#include <cstdio>
#include <fstream> 
#include <mutex>
#include <regex>
#include <string>
#include <thread>
#include <vector>
#include <map>
#include<signal.h>
#include<unistd.h>

#ifdef CSPACY
#include <spacy/spacy>
#endif

// command-line parameters
struct whisper_params {
    int32_t n_threads  = std::min(4, (int32_t) std::thread::hardware_concurrency());
    int32_t prompt_ms  = 5000;
    int32_t command_ms = 8000;
    int32_t capture_id = -1;
    int32_t max_tokens = 32;
    int32_t audio_ctx  = 0;

    float vad_thold    = 0.6f;
    float freq_thold   = 100.0f;

    bool speed_up      = false;
    bool translate     = false;
    bool print_special = false;
    bool print_energy  = false;
    bool no_timestamps = true;

    std::string language  = "en";
    std::string model     = "models/ggml-base.en.bin";
    std::string fname_out;
};

NonBlockingTCPServer server;
std::string lastHeard;
std::string lastSent;

volatile sig_atomic_t flag = 0;

void handle_signal(int sig){
    flag = 1;
}

void whisper_print_usage(int argc, char ** argv, const whisper_params & params);

#ifdef CSPACY
std::string serialize_spacy_token(const Spacy::Token& token)
{
    std::ostringstream oss;
    oss << token.text() << '|'
        << token.lemma_() << '|'
        << token.pos_() << '|'
        << token.dep_();
    return oss.str();
}

std::vector<uint8_t> serialize_spacy_tokens(const std::string &txt, const std::vector<Spacy::Token>& tokens)
{
    std::string serialized_data;
    serialized_data += "SP**" + txt + ">>";
    for (const auto& token : tokens)
    {
        serialized_data += serialize_spacy_token(token) + "&";
    }
    fprintf(stdout, "Sending data: %s\n", serialized_data.c_str());
    return std::vector<uint8_t>(serialized_data.begin(), serialized_data.end());
}

std::vector<uint8_t> spacy_process(const std::string &txt, const Spacy::Nlp &nlp)
{
    fprintf(stderr, "%s\n", "Loading spacy parse");
    // Spacy parse
    auto doc = nlp.parse(txt);
    /*
    for (auto& token : doc.tokens())
        std::cout << token.text() << " [" << token.pos_() << "]\n";
    */
    /*
    std::vector<Spacy::Token> tokens = doc.tokens();
    for (const Spacy::Token& token : tokens)
    {
        fprintf(stdout, "T: %s, %s, [%s]\n", token.text().c_str(), token.lemma_().c_str(), token.pos_().c_str());
    }
    */
    return serialize_spacy_tokens(txt, doc.tokens());
}
#endif

void socket_tick() {
    server.acceptConnections();
    server.receiveDataFromClients(); 
}

bool whisper_params_parse(int argc, char ** argv, whisper_params & params) {
    for (int i = 1; i < argc; i++) {
        std::string arg = argv[i]; 

        if (arg == "-h" || arg == "--help") {
            whisper_print_usage(argc, argv, params);
            exit(0);
        }
        else if (arg == "-t"   || arg == "--threads")       { params.n_threads     = std::stoi(argv[++i]); } 
        else if (arg == "-pms" || arg == "--prompt-ms")     { params.prompt_ms     = std::stoi(argv[++i]); }
        else if (arg == "-cms" || arg == "--command-ms")    { params.command_ms    = std::stoi(argv[++i]); }
        else if (arg == "-c"   || arg == "--capture")       { params.capture_id    = std::stoi(argv[++i]); }
        else if (arg == "-mt"  || arg == "--max-tokens")    { params.max_tokens    = std::stoi(argv[++i]); }
        else if (arg == "-ac"  || arg == "--audio-ctx")     { params.audio_ctx     = std::stoi(argv[++i]); }
        else if (arg == "-vth" || arg == "--vad-thold")     { params.vad_thold     = std::stof(argv[++i]); }
        else if (arg == "-fth" || arg == "--freq-thold")    { params.freq_thold    = std::stof(argv[++i]); }
        else if (arg == "-su"  || arg == "--speed-up")      { params.speed_up      = true; }
        else if (arg == "-tr"  || arg == "--translate")     { params.translate     = true; }
        else if (arg == "-ps"  || arg == "--print-special") { params.print_special = true; }
        else if (arg == "-pe"  || arg == "--print-energy")  { params.print_energy  = true; }
        else if (arg == "-l"   || arg == "--language")      { params.language      = argv[++i]; }
        else if (arg == "-m"   || arg == "--model")         { params.model         = argv[++i]; }
        else if (arg == "-f"   || arg == "--file")          { params.fname_out     = argv[++i]; }
        else {
            fprintf(stderr, "error: unknown argument: %s\n", arg.c_str());
            whisper_print_usage(argc, argv, params);
            exit(0);
        } 
    }

    return true;
} 

void whisper_print_usage(int /*argc*/, char ** argv, const whisper_params & params) {
    fprintf(stderr, "\n");
    fprintf(stderr, "usage: %s [options]\n", argv[0]);
    fprintf(stderr, "\n");
    fprintf(stderr, "options:\n");
    fprintf(stderr, "  -h,         --help           [default] show this help message and exit\n");
    fprintf(stderr, "  -t N,       --threads N      [%-7d] number of threads to use during computation\n", params.n_threads);
    fprintf(stderr, "  -pms N,     --prompt-ms N    [%-7d] prompt duration in milliseconds\n",             params.prompt_ms);
    fprintf(stderr, "  -cms N,     --command-ms N   [%-7d] command duration in milliseconds\n",            params.command_ms);
    fprintf(stderr, "  -c ID,      --capture ID     [%-7d] capture device ID\n",                           params.capture_id);
    fprintf(stderr, "  -mt N,      --max-tokens N   [%-7d] maximum number of tokens per audio chunk\n",    params.max_tokens);
    fprintf(stderr, "  -ac N,      --audio-ctx N    [%-7d] audio context size (0 - all)\n",                params.audio_ctx);
    fprintf(stderr, "  -vth N,     --vad-thold N    [%-7.2f] voice activity detection threshold\n",        params.vad_thold);
    fprintf(stderr, "  -fth N,     --freq-thold N   [%-7.2f] high-pass frequency cutoff\n",                params.freq_thold);
    fprintf(stderr, "  -su,        --speed-up       [%-7s] speed up audio by x2 (reduced accuracy)\n",     params.speed_up ? "true" : "false");
    fprintf(stderr, "  -tr,        --translate      [%-7s] translate from source language to english\n",   params.translate ? "true" : "false");
    fprintf(stderr, "  -ps,        --print-special  [%-7s] print special tokens\n",                        params.print_special ? "true" : "false");
    fprintf(stderr, "  -pe,        --print-energy   [%-7s] print sound energy (for debugging)\n",          params.print_energy ? "true" : "false");
    fprintf(stderr, "  -l LANG,    --language LANG  [%-7s] spoken language\n",                             params.language.c_str());
    fprintf(stderr, "  -m FNAME,   --model FNAME    [%-7s] model path\n",                                  params.model.c_str());
    fprintf(stderr, "  -f FNAME,   --file FNAME     [%-7s] text output file name\n",                       params.fname_out.c_str());
    fprintf(stderr, "\n");
}

std::string transcribe(whisper_context * ctx, const whisper_params & params, const std::vector<float> & pcmf32, float & prob, int64_t & t_ms) {
    fprintf(stdout, "%s\n", "Transcribing...");
    const auto t_start = std::chrono::high_resolution_clock::now();

    prob = 0.0f;
    t_ms = 0;

    whisper_full_params wparams = whisper_full_default_params(WHISPER_SAMPLING_GREEDY);

    wparams.print_progress   = false;
    wparams.print_special    = params.print_special;
    wparams.print_realtime   = false;
    wparams.print_timestamps = !params.no_timestamps;
    wparams.translate        = params.translate;
    wparams.no_context       = true;
    wparams.single_segment   = true;
    wparams.max_tokens       = params.max_tokens;
    wparams.language         = params.language.c_str();
    wparams.n_threads        = params.n_threads;

    wparams.audio_ctx        = params.audio_ctx;
    wparams.speed_up         = params.speed_up;

    if (whisper_full(ctx, wparams, pcmf32.data(), pcmf32.size()) != 0) {
        return "";
    }

    int prob_n = 0;
    std::string result;

    const int n_segments = whisper_full_n_segments(ctx);
    for (int i = 0; i < n_segments; ++i) {
        const char * text = whisper_full_get_segment_text(ctx, i);

        result += text;

        const int n_tokens = whisper_full_n_tokens(ctx, i);
        for (int j = 0; j < n_tokens; ++j) {
            const auto token = whisper_full_get_token_data(ctx, i, j);

            prob += token.p;
            ++prob_n;
        }
    }

    if (prob_n > 0) {
        prob /= prob_n;
    }

    const auto t_end = std::chrono::high_resolution_clock::now();
    t_ms = std::chrono::duration_cast<std::chrono::milliseconds>(t_end - t_start).count();

    return result;
}

std::vector<std::string> get_words(const std::string &txt) {
    std::vector<std::string> words;

    std::istringstream iss(txt);
    std::string word;
    while (iss >> word) {
        words.push_back(word);
    }

    return words;
}

#ifdef CSPACY
// Function to process general transcription using Whisper AI
int process_general_transcription(struct whisper_context *ctx, audio_async &audio, const whisper_params &params, const Spacy::Nlp &nlp) {
#else
int process_general_transcription(struct whisper_context *ctx, audio_async &audio, const whisper_params &params) {
#endif

    bool is_running = true;
    float prob0 = 0.0f;  // Initial probability value
    float prob = 0.0f;   // Current probability value

    std::vector<float> pcmf32_cur;  // Container for audio data

    fprintf(stderr, "\n");
    fprintf(stderr, "%s: transcript-socket mode\n", __func__);

    // Main loop
    while (!flag) {
        // Check for Ctrl + C input to exit the loop
        is_running = sdl_poll_events();

        // Introduce a delay to control loop frequency
        std::this_thread::sleep_for(std::chrono::milliseconds(100));

        // Retrieve audio data into pcmf32_cur vector
        audio.get(2000, pcmf32_cur);

        // Perform voice activity detection (VAD) on the audio data
        if (::vad_simple(pcmf32_cur, WHISPER_SAMPLE_RATE, 1000, params.vad_thold, params.freq_thold, params.print_energy)) {
            fprintf(stdout, "%s: Speech detected! Processing ...\n", __func__);

            int64_t t_ms = 0;  // Timestamp in milliseconds

            // Capture audio after activation phrase for command processing
            audio.get(params.command_ms, pcmf32_cur);

            // Perform transcription and obtain transcribed text
            const auto txt = ::trim(::transcribe(ctx, params, pcmf32_cur, prob, t_ms));
            
            // Calculate and update the confidence probability
            prob = 100.0f * (prob - prob0);

            // Display the transcribed text and timestamp
            fprintf(stdout, "%s: Heard: '%s%s%s', (t = %d ms)\n", __func__, "\033[1m", txt.c_str(), "\033[0m", (int) t_ms);
            fprintf(stdout, "\n");
            fflush(stdout);

            // Store the last heard text
            lastHeard = txt;

            // Clear the audio buffer
            audio.clear();

            socket_tick();
            #ifdef CSPACY
                server.sendDataToClients(spacy_process(txt, nlp));
            #else
                std::string d = "NS**" + txt;
                fprintf(stdout, "Sending data: %s\n", d.c_str());
                server.sendDataToClients(d.c_str());
            #endif

            fflush(stdout);
        }
    }
 
    return 0;  // Return success
}


int main(int argc, char ** argv) {
    fprintf(stdout, "%s\n", "Registering signal.");
    signal(SIGINT, handle_signal);
    whisper_params params;

    if (whisper_params_parse(argc, argv, params) == false) {
        return 1;
    }

    if (whisper_lang_id(params.language.c_str()) == -1) {
        fprintf(stderr, "error: unknown language '%s'\n", params.language.c_str());
        whisper_print_usage(argc, argv, params);
        exit(0);
    }

#ifdef CSPACY
    // spacy init
    Spacy::Spacy spacy;
    auto nlp = spacy.load("en_core_web_sm");
    fprintf(stdout, "spacy loaded\n");
#endif

    // socket init  
    server.setupServer(31050); 
    fprintf(stderr, "[socket] communication opened. Port: 31050\n");

    // whisper init   

    struct whisper_context * ctx = whisper_init_from_file(params.model.c_str());

    // print some info about the processing
    {
        fprintf(stderr, "\n");
        if (!whisper_is_multilingual(ctx)) {
            if (params.language != "en" || params.translate) {
                params.language = "en";
                params.translate = false;
                fprintf(stderr, "%s: WARNING: model is not multilingual, ignoring language and translation options\n", __func__);
            }
        }
        fprintf(stderr, "%s: processing, %d threads, lang = %s, task = %s, timestamps = %d ...\n",
                __func__,
                params.n_threads,
                params.language.c_str(),
                params.translate ? "translate" : "transcribe", 
                params.no_timestamps ? 0 : 1);

        fprintf(stderr, "\n");
    }

    // init audio

    audio_async audio(30*1000);
    if (!audio.init(params.capture_id, WHISPER_SAMPLE_RATE)) {
        fprintf(stderr, "%s: audio.init() failed!\n", __func__);
        return 1;
    }

    audio.resume();

    // wait for 1 second to avoid any buffered noise
    std::this_thread::sleep_for(std::chrono::milliseconds(1000));
    audio.clear();

    int  ret_val = 0;

    #ifdef CSPACY
    ret_val = process_general_transcription(ctx, audio, params, nlp);
    #else
    ret_val = process_general_transcription(ctx, audio, params);
    #endif

    audio.pause();

    whisper_print_timings(ctx);
    whisper_free(ctx);
    server.closeServer();
    fprintf(stderr, "[socket] closed successfully\n");
    return ret_val;
}

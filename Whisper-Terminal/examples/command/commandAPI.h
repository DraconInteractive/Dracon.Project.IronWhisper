#include <string>

class CommandAPI {
public:
    static void SendGetRequest(const std::string &url);
    static void SendGetRequest(const std::string& url, const std::string& message);
};
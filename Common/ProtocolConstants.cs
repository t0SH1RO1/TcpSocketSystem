namespace Common;

public static class ProtocolConstants
{
    public const string LineEnding = "\n";
    
    public static class Commands
    {
        public const string Logout = "LOGOUT";
        public const string Car = "CAR";
        public const string Ping = "PING";
    }
    
    public static class ResponsePrefixes
    {
        public const string Ok = "OK";
        public const string Error = "ERROR";
    }
    
    public static class ErrorCodes
    {
        public const string UnknownCommand = "UnknownCommand";
        public const string EmptyCommand = "EmptyCommand";
        public const string MissingParameter = "MissingParameter";
        public const string InternalError = "InternalError";
    }
}
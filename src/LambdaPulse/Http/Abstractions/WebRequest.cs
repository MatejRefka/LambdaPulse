namespace LambdaPulse
{
    public class WebRequest
    {
        public required string Method { get; set; }
        public required string Path { get; set; }
        public required string Protocol { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? Body { get; set; }
        public string Payload { get; set; } = "Dummy Request";
    }
}

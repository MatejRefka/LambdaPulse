namespace LambdaPulse.Services.Http.Models
{
    public class WebResponse
    {
        public int? StatusCode { get; set; }
        public string? ResponsePhrase { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? Body { get; set; }
        public string? Payload { get; set; } = "Dummy Response";
    }
}

using System;

namespace BlackBoxControl.Models
{
    public class ReceiveApiInput : CauseInput
    {
        public string ListenUrl { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string ExpectedPath { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public override string GetDescription()
        {
            return $"{ExpectedPath} at {ListenUrl}";
        }
    }
}
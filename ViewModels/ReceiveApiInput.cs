using System;

namespace BlackBoxControl.Models
{
    public class ReceiveApiInput : CauseInput
    {
        public string ListenUrl { get; set; }
        public string HttpMethod { get; set; }
        public string ExpectedPath { get; set; }
        public string AuthToken { get; set; }
        public override string GetDescription()
        {
            return $"{ExpectedPath} at {ListenUrl}";
        }
    }
}
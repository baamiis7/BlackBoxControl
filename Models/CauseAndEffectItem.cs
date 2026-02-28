// In Models/CauseAndEffectItems.cs

using System;
using System.Net.Mail;

namespace BlackBoxControl.Models
{
    // Base class for all inputs
    public abstract class CauseInput
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public abstract string GetDescription();
    }

    // Represents a physical device input
    public class DeviceInput : CauseInput
    {
        public string DeviceId { get; set; } = string.Empty; // e.g., "Loop1-Address5"
        public string Type { get; set; } = string.Empty;
        public string LocationText { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;

        public override string GetDescription()
        {
            return $"{Type} at {LocationText}";
        }
    }

    // Represents a time of day input
    public class TimeOfDayInput : CauseInput
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public override string GetDescription()
        {
            return $"Between {StartTime:hh\\:mm} and {EndTime:hh\\:mm}";
        }
    }

    // Represents a specific date/time input
    public class DateTimeInput : CauseInput
    {
        public DateTime TriggerDateTime { get; set; }

        public override string GetDescription()
        {
            return $"At {TriggerDateTime:yyyy-MM-dd HH:mm}";
        }
    }

    // --- OUTPUTS ---

    // Base class for all outputs
    public abstract class EffectOutput
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public abstract string GetDescription();
    }

    // Represents a physical device output
    public class DeviceOutput : EffectOutput
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string LocationText { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;

        public override string GetDescription()
        {
            return $"{Type} at {LocationText}";
        }
    }

    // Represents a send text message action
    public class SendTextOutput : EffectOutput
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public override string GetDescription()
        {
            return $"Send SMS to {PhoneNumber}";
        }
    }

    // Represents a send email action
    public class SendEmailOutput : EffectOutput
    {
        public string EmailAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public override string GetDescription()
        {
            return $"Send Email to {EmailAddress}";
        }
    }
    public class SendApiOutput : EffectOutput
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;

        public override string GetDescription()
        {
            return $"Send API {ApiUrl}";
        }
    }
}

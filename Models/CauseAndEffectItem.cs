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
        public string DeviceId { get; set; } // e.g., "Loop1-Address5"
        public string Type { get; set; }
        public string LocationText { get; set; }
        public string ImagePath { get; set; }

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
        public string DeviceId { get; set; }
        public string Type { get; set; }
        public string LocationText { get; set; }
        public string ImagePath { get; set; }

        public override string GetDescription()
        {
            return $"{Type} at {LocationText}";
        }
    }

    // Represents a send text message action
    public class SendTextOutput : EffectOutput
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }

        public override string GetDescription()
        {
            return $"Send SMS to {PhoneNumber}";
        }
    }

    // Represents a send email action
    public class SendEmailOutput : EffectOutput
    {
        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public override string GetDescription()
        {
            return $"Send Email to {EmailAddress}";
        }
    }
    public class SendApiOutput : EffectOutput
    {
        public string ApiUrl { get; set; }
        public string HttpMethod { get; set; }
        public string ContentType { get; set; }
        public string RequestBody { get; set; }

        public override string GetDescription()
        {
            return $"Send API {ApiUrl}";
        }
    }
}

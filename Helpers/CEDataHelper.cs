using System;

namespace BlackBoxControl.Helpers
{
    public static class CEDataHelper
    {
        // AddressType constants for INPUTS
        public const byte DEVICE_INPUT = 0x01;
        public const byte TIME_OF_DAY_INPUT = 0x02;
        public const byte DATETIME_INPUT = 0x03;
        public const byte API_WEBHOOK_INPUT = 0x04;

        // AddressType constants for OUTPUTS
        public const byte DEVICE_OUTPUT = 0x10;
        public const byte SEND_SMS_OUTPUT = 0x11;
        public const byte SEND_EMAIL_OUTPUT = 0x12;
        public const byte SEND_API_OUTPUT = 0x13;

        // HTTP Method codes
        public const byte HTTP_GET = 0x00;
        public const byte HTTP_POST = 0x01;
        public const byte HTTP_PUT = 0x02;
        public const byte HTTP_DELETE = 0x03;

        // Content Type codes
        public const byte CONTENT_JSON = 0x00;
        public const byte CONTENT_XML = 0x01;
        public const byte CONTENT_TEXT = 0x02;

        public static string DecodeHttpMethod(byte methodCode)
        {
            return methodCode switch
            {
                HTTP_POST => "POST",
                HTTP_PUT => "PUT",
                HTTP_DELETE => "DELETE",
                _ => "GET"
            };
        }

        public static string DecodeContentType(byte contentCode)
        {
            return contentCode switch
            {
                CONTENT_XML => "application/xml",
                CONTENT_TEXT => "text/plain",
                _ => "application/json"
            };
        }

        public static string GetInputTypeName(byte addressType)
        {
            return addressType switch
            {
                DEVICE_INPUT => "Device",
                TIME_OF_DAY_INPUT => "TimeOfDay",
                DATETIME_INPUT => "DateTime",
                API_WEBHOOK_INPUT => "ReceiveApi",
                _ => "Unknown"
            };
        }

        public static string GetOutputTypeName(byte addressType)
        {
            return addressType switch
            {
                DEVICE_OUTPUT => "Device",
                SEND_SMS_OUTPUT => "SendText",
                SEND_EMAIL_OUTPUT => "SendEmail",
                SEND_API_OUTPUT => "SendApi",
                _ => "Unknown"
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackBoxControl.Models
{
    public static class DeviceTypes
    {
        // General constants
        public const int ACCESS_PASS_LEN = 4;
        public const int NODE_NAME_LEN = 15;
        public const int MAX_LOOPS = 2;
        public const int MAX_ZONES = 501; // 1 more than there really is

        public const int MAX_DEVICES_X = 255;
        public const int HOCHIKI_MAX_DEVICES = 254;
        public const int HOCHIKI_MAX_MAIN_DEVICES = 127;
        public const int APOLLO_MAX_DEVICES = 126;
        public const int MAX_SUB_DEVICES = 800; // Divisible by 4

        public const int HOCHIKI_BASE_SOUNDER_OFFSET = 127;

        // Type codes for Hochiki devices
        public const byte H_CALL_POINT = 0x00;
        public const byte H_BASE_MODULE = 0x12;
        public const byte H_BASE_MASTER = 0x14;
        public const byte H_MINI_ZONE = 0x15;
        public const byte H_LOOPS_CONTROLLER = 0x19; // or 0x60 for VIMPEX
        public const byte H_SWITCH_MODULE = 0x39;
        public const byte H_LOOP_BEACON = 0x41;
        public const byte H_ADR_REMOTE_IND = 0x42;
        public const byte H_LOOP_SOUNDER = 0x5e;
        public const byte H_BELL_MODULE = 0x78;
        public const byte H_MULTI_IO_UNIT = 0x7a;
        public const byte H_OUTPUT_MODULE = 0x7c;
        public const byte H_SINGLE_IO_MODULE = 0x7d;
        public const byte H_MULTI_SENSOR = 0xd8;
        public const byte H_NOT_PRESENT = 0xff;
        public const byte H_PHOTOELECTRIC = 0x88;
        public const byte H_HEAT_SENSOR = 0x98;
        public const byte H_HEAT_SENSOR_ACB = 0x99;
        public const byte H_IONISATION = 0xa8;
        public const byte H_LIOU = 0xfb;
        public const byte H_POM_OUTPUT_MODULE = 0x7e;
        public const byte H_ACD_SENSOR = 0xd9;
        public const byte H_PLANT_CONTROLLER = 0x79;

        // YBO and WS2 loop sounders
        public const byte H_YBO_LOOP_SOUNDER = 0xfe;
        public const byte H_WS2_LOOP_SOUNDER = 0xfd;
        public const byte H_YBO_B_LOOP_SOUNDER = 0xfc;

        // Type codes for Apollo devices
        public const byte AP_NOT_PRESENT = 0xff;
        public const byte AP_S90_SHOP_MONITOR_UNIT = 0x00;
        public const byte AP_XP_LOOP_SOUNDER = 0x01;
        public const byte AP_XP_D_CO_SOUNDER_BASE = 0x14;
        public const byte AP_S90_SOUNDER_CCT_CONTROLLER = 0x41;
        public const byte AP_XP_SOUNDER_CONTROL_UNIT = 0x81;
        public const byte AP_S90_3CH_OUTPUT_UNIT = 0x02;
        public const byte AP_S90_1CH_OUTPUT_UNIT = 0x22;
        public const byte AP_S90_SWITCH_MONITOR = 0x42;
        public const byte AP_XP_OUTPUT_UNIT = 0xa2;
        public const byte AP_S90_IONISATION = 0x03;
        public const byte AP_XP_IONISATION = 0x83;
        public const byte AP_D_IONISATION = 0xa3;
        public const byte AP_S90_ZONE_MONITOR_UNIT = 0x04;
        public const byte AP_S90_CONTROL_UNIT_MONITOR = 0x24;
        public const byte AP_XP_ZONE_MONITOR_UNIT = 0x84;
        public const byte AP_XP_RADIO_SENSOR = 0xc4;
        public const byte AP_XP_SWITCH_MONITOR = 0x8c;
        public const byte AP_XP_SWITCH_MONITOR_PLUS = 0xac;
        public const byte AP_XP_MINI_SWITCH_MONITOR = 0xcc;
        public const byte AP_S90_PHOTOELECTRIC = 0x05;
        public const byte AP_XP_PHOTOELECTRIC = 0x85;
        public const byte AP_XP_BEAM = 0xc5;
        public const byte AP_XP_INTELEGENT_BEAM = 0x8d;
        public const byte AP_XP_FLAME_DETECTOR = 0x95;
        public const byte AP_XP_MULTI_PHOTO = 0x9d;
        public const byte AP_D_PHOTOELECTRIC = 0xa5;
        public const byte AP_D_DUAL_SENSOR = 0xbd;
        public const byte AP_S90_HEAT_SENSOR = 0x06;
        public const byte AP_XP_HEAT_SENSOR = 0x86;
        public const byte AP_XP_HIGH_TEMP_SENSOR = 0x8e;
        public const byte AP_D_HEAT_SENSOR = 0xa6;
        public const byte AP_D_GASEOUS_FIRE_SENSOR = 0xab;
        public const byte AP_D_CO_HEAT_FIRE_SENSOR = 0xb3;
        public const byte AP_S90_CALL_POINT = 0x07;
        public const byte AP_S90_CALL_POINT_MONITOR = 0x27;
        public const byte AP_XP_CALL_POINT = 0x9f;
        public const byte AP_D_CALL_POINT = 0xbf;
        public const byte AP_XP_MINI_SWITCH_INTERRUPT = 0xdf;
        public const byte AP_XP_3CH_INPUT_OUTPUT_UNIT = 0x82;
        public const byte AP_D_SBB_BASE_SOUNDER = 0xb1;
        public const byte AP_XP_CLUSTER_ZONE_MONITOR = 0xfd;
        public const byte AP_XP_ANCILLARY_BASE_SOUNDER = 0xfe;

        // Define the protocol constants
        public const int HOCHIKI_PROTOCOL = 0x1000;  // Example value, replace with the actual value

        // Device type constants
        public const int K_H_CALL_POINT = HOCHIKI_PROTOCOL | H_CALL_POINT;
        public const int K_H_BASE_MODULE = HOCHIKI_PROTOCOL | H_BASE_MODULE;
        public const int K_H_BASE_MASTER = HOCHIKI_PROTOCOL | H_BASE_MASTER;
        public const int K_H_MINI_ZONE = HOCHIKI_PROTOCOL | H_MINI_ZONE;
        public const int K_H_LOOPS_CONTROLLER = HOCHIKI_PROTOCOL | H_LOOPS_CONTROLLER;
        public const int K_H_SWITCH_MODULE = HOCHIKI_PROTOCOL | H_SWITCH_MODULE;
        public const int K_H_LOOP_SOUNDER = HOCHIKI_PROTOCOL | H_LOOP_SOUNDER;
        public const int K_H_LOOP_BEACON = HOCHIKI_PROTOCOL | H_LOOP_BEACON;
        public const int K_H_ADR_REMOTE_IND = HOCHIKI_PROTOCOL | H_ADR_REMOTE_IND;
        public const int K_H_BELL_MODULE = HOCHIKI_PROTOCOL | H_BELL_MODULE;
        public const int K_H_MULTI_IO_UNIT = HOCHIKI_PROTOCOL | H_MULTI_IO_UNIT;
        public const int K_H_OUTPUT_MODULE = HOCHIKI_PROTOCOL | H_OUTPUT_MODULE;
        public const int K_H_SINGLE_IO_MODULE = HOCHIKI_PROTOCOL | H_SINGLE_IO_MODULE;
        public const int K_H_PHOTOELECTRIC = HOCHIKI_PROTOCOL | H_PHOTOELECTRIC;
        public const int K_H_HEAT_SENSOR = HOCHIKI_PROTOCOL | H_HEAT_SENSOR;
        public const int K_H_HEAT_SENSOR_ACB = HOCHIKI_PROTOCOL | H_HEAT_SENSOR_ACB;
        public const int K_H_IONISATION = HOCHIKI_PROTOCOL | H_IONISATION;
        public const int K_H_MULTI_SENSOR = HOCHIKI_PROTOCOL | H_MULTI_SENSOR;
        public const int K_H_LIOU = HOCHIKI_PROTOCOL | H_LIOU;
        public const int K_H_NOT_PRESENT = HOCHIKI_PROTOCOL | H_NOT_PRESENT;
        public const int K_H_UNKNOWN = HOCHIKI_PROTOCOL | H_NOT_PRESENT;  // Assuming 'H_UNKNOWN' is not defined, using H_NOT_PRESENT for now
        public const int K_H_YBO_LOOP_SOUNDER = HOCHIKI_PROTOCOL | H_YBO_LOOP_SOUNDER;
        public const int K_H_WS2_LOOP_SOUNDER = HOCHIKI_PROTOCOL | H_WS2_LOOP_SOUNDER;
        public const int K_H_YBO_B_LOOP_SOUNDER = HOCHIKI_PROTOCOL | H_YBO_B_LOOP_SOUNDER;
        public const int K_H_POM_OUTPUT_MODULE = HOCHIKI_PROTOCOL | H_POM_OUTPUT_MODULE;
        public const int K_H_ACD_SENSOR = HOCHIKI_PROTOCOL | H_ACD_SENSOR;
        public const int K_H_PLANT_CONTROLLER = HOCHIKI_PROTOCOL | H_PLANT_CONTROLLER;

        // Define the Apollo protocol constant
        public const int APOLLO_PROTOCOL = 0x2000;  // Example value, replace with actual value

        // Apollo device type constants
        public const int K_AP_NOT_PRESENT = APOLLO_PROTOCOL | AP_NOT_PRESENT;
        public const int K_AP_S90_SHOP_MONITOR_UNIT = APOLLO_PROTOCOL | AP_S90_SHOP_MONITOR_UNIT;
        public const int K_AP_XP_D_CO_SOUNDER_BASE = APOLLO_PROTOCOL | AP_XP_D_CO_SOUNDER_BASE;
        public const int K_AP_S90_SOUNDER_CCT_CONTROLLER = APOLLO_PROTOCOL | AP_S90_SOUNDER_CCT_CONTROLLER;
        public const int K_AP_XP_SOUNDER_CONTROL_UNIT = APOLLO_PROTOCOL | AP_XP_SOUNDER_CONTROL_UNIT;
        public const int K_AP_XP_LOOP_SOUNDER = APOLLO_PROTOCOL | AP_XP_LOOP_SOUNDER;
        public const int K_AP_S90_3CH_OUTPUT_UNIT = APOLLO_PROTOCOL | AP_S90_3CH_OUTPUT_UNIT;
        public const int K_AP_S90_1CH_OUTPUT_UNIT = APOLLO_PROTOCOL | AP_S90_1CH_OUTPUT_UNIT;
        public const int K_AP_S90_SWITCH_MONITOR = APOLLO_PROTOCOL | AP_S90_SWITCH_MONITOR;
        public const int K_AP_XP_OUTPUT_UNIT = APOLLO_PROTOCOL | AP_XP_OUTPUT_UNIT;
        public const int K_AP_S90_IONISATION = APOLLO_PROTOCOL | AP_S90_IONISATION;
        public const int K_AP_XP_IONISATION = APOLLO_PROTOCOL | AP_XP_IONISATION;
        public const int K_AP_D_IONISATION = APOLLO_PROTOCOL | AP_D_IONISATION;
        public const int K_AP_S90_ZONE_MONITOR_UNIT = APOLLO_PROTOCOL | AP_S90_ZONE_MONITOR_UNIT;
        public const int K_AP_S90_CONTROL_UNIT_MONITOR = APOLLO_PROTOCOL | AP_S90_CONTROL_UNIT_MONITOR;
        public const int K_AP_XP_ZONE_MONITOR_UNIT = APOLLO_PROTOCOL | AP_XP_ZONE_MONITOR_UNIT;
        public const int K_AP_XP_RADIO_SENSOR = APOLLO_PROTOCOL | AP_XP_RADIO_SENSOR;
        public const int K_AP_XP_SWITCH_MONITOR = APOLLO_PROTOCOL | AP_XP_SWITCH_MONITOR;
        public const int K_AP_XP_SWITCH_MONITOR_PLUS = APOLLO_PROTOCOL | AP_XP_SWITCH_MONITOR_PLUS;
        public const int K_AP_XP_MINI_SWITCH_MONITOR = APOLLO_PROTOCOL | AP_XP_MINI_SWITCH_MONITOR;
        public const int K_AP_S90_PHOTOELECTRIC = APOLLO_PROTOCOL | AP_S90_PHOTOELECTRIC;
        public const int K_AP_XP_PHOTOELECTRIC = APOLLO_PROTOCOL | AP_XP_PHOTOELECTRIC;
        public const int K_AP_XP_BEAM = APOLLO_PROTOCOL | AP_XP_BEAM;
        public const int K_AP_XP_INTELEGENT_BEAM = APOLLO_PROTOCOL | AP_XP_INTELEGENT_BEAM;
        public const int K_AP_XP_FLAME_DETECTOR = APOLLO_PROTOCOL | AP_XP_FLAME_DETECTOR;
        public const int K_AP_XP_MULTI_PHOTO = APOLLO_PROTOCOL | AP_XP_MULTI_PHOTO;
        public const int K_AP_D_PHOTOELECTRIC = APOLLO_PROTOCOL | AP_D_PHOTOELECTRIC;
        public const int K_AP_D_DUAL_SENSOR = APOLLO_PROTOCOL | AP_D_DUAL_SENSOR;
        public const int K_AP_S90_HEAT_SENSOR = APOLLO_PROTOCOL | AP_S90_HEAT_SENSOR;
        public const int K_AP_XP_HEAT_SENSOR = APOLLO_PROTOCOL | AP_XP_HEAT_SENSOR;
        public const int K_AP_XP_HIGH_TEMP_SENSOR = APOLLO_PROTOCOL | AP_XP_HIGH_TEMP_SENSOR;
        public const int K_AP_D_HEAT_SENSOR = APOLLO_PROTOCOL | AP_D_HEAT_SENSOR;
        public const int K_AP_D_GASEOUS_FIRE_SENSOR = APOLLO_PROTOCOL | AP_D_GASEOUS_FIRE_SENSOR;
        public const int K_AP_D_CO_HEAT_FIRE_SENSOR = APOLLO_PROTOCOL | AP_D_CO_HEAT_FIRE_SENSOR;
        public const int K_AP_S90_CALL_POINT = APOLLO_PROTOCOL | AP_S90_CALL_POINT;
        public const int K_AP_S90_CALL_POINT_MONITOR = APOLLO_PROTOCOL | AP_S90_CALL_POINT_MONITOR;
        public const int K_AP_XP_CALL_POINT = APOLLO_PROTOCOL | AP_XP_CALL_POINT;
        public const int K_AP_D_CALL_POINT = APOLLO_PROTOCOL | AP_D_CALL_POINT;
        public const int K_AP_XP_MINI_SWITCH_INTERRUPT = APOLLO_PROTOCOL | AP_XP_MINI_SWITCH_INTERRUPT;
        public const int K_AP_XP_3CH_INPUT_OUTPUT_UNIT = APOLLO_PROTOCOL | AP_XP_3CH_INPUT_OUTPUT_UNIT;
        public const int K_AP_D_SBB_BASE_SOUNDER = APOLLO_PROTOCOL | AP_D_SBB_BASE_SOUNDER;
        public const int K_AP_XP_CLUSTER_ZONE_MONITOR = APOLLO_PROTOCOL | AP_XP_CLUSTER_ZONE_MONITOR;
        public const int K_AP_XP_ANCILLARY_BASE_SOUNDER = APOLLO_PROTOCOL | 0x08;  // Specific value
        public const int K_AP_XP_CH_INPUT_OUTPUT_UNIT = APOLLO_PROTOCOL | 0xfe;  // Specific value
    }

}

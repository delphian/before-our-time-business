using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals.Telnet.Ansi
{
    public static class AnsiColors
    {
        // Regular
        public static string black = "\u001B[0;30m";
        public static string red = "\u001B[0;31m";
        public static string green = "\u001B[0;32m";
        public static string yellow = "\u001B[0;33m";
        public static string blue = "\u001B[0;34m";
        public static string purple = "\u001B[0;35m";
        public static string cyan = "\u001B[0;36m";
        public static string white = "\u001B[0;37m";
        // Bold
        public static string blackB = "\u001B[1;30m";
        public static string redB = "\u001B[1;31m";
        public static string greenB = "\u001B[1;32m";
        public static string yellowB = "\u001B[1;33m";
        public static string blueB = "\u001B[1;34m";
        public static string purpleB = "\u001B[1;35m";
        public static string cyanB = "\u001B[1;36m";
        public static string whiteB = "\u001B[1;37m";
        // Underline
        public static string blackU = "\u001B[4;30m";
        public static string redU = "\u001B[4;31m";
        public static string greenU = "\u001B[4;32m";
        public static string yellowU = "\u001B[4;33m";
        public static string blueU = "\u001B[4;34m";
        public static string purpleU = "\u001B[4;35m";
        public static string cyanU = "\u001B[4;36m";
        public static string whiteU = "\u001B[4;37m";
        // High intensity
        public static string blackHi = "\u001B[0;90m";
        public static string redHi = "\u001B[0;91m";
        public static string greenHi = "\u001B[0;92m";
        public static string yellowHi = "\u001B[0;93m";
        public static string blueHi = "\u001B[0;94m";
        public static string purpleHi = "\u001B[0;95m";
        public static string cyanHi = "\u001B[0;96m";
        public static string whiteHi = "\u001B[0;97m";
        // Bold high intensity
        public static string blackBHi = "\u001B[1;90m";
        public static string redBHi = "\u001B[1;91m";
        public static string greenBHi = "\u001B[1;92m";
        public static string yellowBHi = "\u001B[1;93m";
        public static string blueBHi = "\u001B[1;94m";
        public static string purpleBHi = "\u001B[1;95m";
        public static string cyanBHi = "\u001B[1;96m";
        public static string whiteBHi = "\u001B[1;97m";
        // Misc
        public static string reset = "\u001B[0m";
    }
}

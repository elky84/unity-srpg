using Logic.Types;
using System;
using System.Collections.Generic;

namespace Logic.Util
{
    // Logger를 연동해도, 유니티에서 로직을 사용할 수 있기 때문에, DI로 주입 받아서 로깅하도록 수정해야 함. 그 전에 임시로 컬러링을 위한 작업.
    public static class LogUtil
    {
        public delegate void LogDelegate(String str, params object[] args);

        public static Dictionary<LogLevel, LogDelegate> LogDelegates { get; set; } = new Dictionary<LogLevel, LogDelegate>();

        public static void WriteError(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Error, ConsoleColor.Black, ConsoleColor.Red, format, args);
        }
        public static void WriteDebug(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Green, format, args);
        }

        public static void WriteWarn(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Warn, ConsoleColor.Black, ConsoleColor.DarkRed, format, args);
        }

        public static void WriteTrace(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Trace, ConsoleColor.Black, ConsoleColor.Green, format, args);
        }

        public static void WriteInfo(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Info, ConsoleColor.Black, ConsoleColor.White, format, args);
        }

        public static void WriteGameStartEnd(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.DarkGray, ConsoleColor.Magenta, format, args);
        }

        public static void WriteDead(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.DarkRed, format, args);
        }

        public static void WriteDamage(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Red, format, args);
        }

        public static void WritePlayerTurn(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Blue, format, args);
        }

        public static void WriteSkillEffect(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Cyan, format, args);
        }

        public static void WriteBuffEffect(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Green, format, args);
        }

        public static void WriteNormalAttack(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Magenta, format, args);
        }

        public static void WriteComboAttack(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.DarkMagenta, format, args);
        }

        public static void WriteHpChange(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.Gray, format, args);
        }

        public static void WriteChargingToken(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.Black, ConsoleColor.DarkGray, format, args);
        }

        public static void WriteActiveSkill(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.DarkGray, ConsoleColor.Yellow, format, args);
        }


        public static void WriteChargeSkill(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.DarkGray, ConsoleColor.DarkYellow, format, args);
        }


        public static void WriteBadStatus(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.DarkGray, ConsoleColor.DarkRed, format, args);
        }


        public static void WriteGuard(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Debug, ConsoleColor.DarkGray, ConsoleColor.DarkGreen, format, args);
        }


        public static void WriteImportant(string format, params object[] args)
        {
            WriteWithColor(LogLevel.Error, ConsoleColor.Black, ConsoleColor.Yellow, format, args);
        }

        public static void WriteWithColor(LogLevel logLevel, ConsoleColor bgColor, ConsoleColor fgColor, string format, params object[] args)
        {
            if (LogDelegates.TryGetValue(logLevel, out LogDelegate logDelegate))
            {
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;

                Console.WriteLine(format, args);

                Console.ResetColor();
            }
        }
    }
}

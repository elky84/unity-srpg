using System;
using System.Collections.Generic;
using System.Linq;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using Logic.Entity;

namespace Logic.Util
{
    public static class DslUtil
    {
        public static KeyValuePair<string, List<string>> Parse(this string str)
        {
            var trimmed = Regex.Replace(str, @"\s+", "");
            var splitted = trimmed.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length == 0)
            {
                return new KeyValuePair<string, List<string>>(null, null);
            }
            else if (splitted.Length < 2)
            {
                return new KeyValuePair<string, List<string>>(splitted[0], null);
            }
            else
            {
                return new KeyValuePair<string, List<string>>(splitted[0], splitted[1].Split(',').OfType<string>().ToList());
            }
        }

        public static Dictionary<string, string> ParseYaml(this string str)
        {
            var input = new StringReader(str);
            var deserializer = new DeserializerBuilder().Build();
            var parser = new Parser(input);
            parser.Expect<StreamStart>();

            while (parser.Accept<DocumentStart>())
            {
                return deserializer.Deserialize<Dictionary<string, string>>(parser);
            }

            return null;
        }

        public static Skill ParseYamlSkill(this string str)
        {
            try
            {
                var dic = str.ParseYaml();
                if (dic == null)
                    return null;

                Skill skill = new Skill();
                skill.Setup(dic);
                return skill;
            }
            catch (YamlDotNet.Core.SyntaxErrorException exception)
            {
                Console.WriteLine("ParseYamlSkill failed. str: {0}, exception: {1}", str, exception);
                throw exception;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Humanizer
{
    public static class StringHumanizeExtensions
    {
        static readonly Func<string, string> FromUnderscoreDashSeparatedWords = methodName => string.Join(" ", methodName.Split(new[] { '_', '-' }));

        static string FromPascalCase(string name)
        {
            var resultBuilder = new StringBuilder();
            var wordBuilder = new StringBuilder();

            Func<StringBuilder, Func<char, bool>, bool>
                lastCharOf = (builder, charPredicate) =>
                    builder.Length > 0 && charPredicate(builder[builder.Length - 1]);
            Func<Func<char, bool>, bool>
                lastCharOfCurrentWord = isOfCharClass =>
                    lastCharOf(wordBuilder, isOfCharClass);
            Action appendSpaceToResult = () =>
            {
                if (lastCharOf(resultBuilder, _ => _ != ' '))
                {
                    resultBuilder.Append(' ');
                }
            };

            foreach (var currentChar in name)
            {
                if (lastCharOfCurrentWord(char.IsLower)
                    && (char.IsUpper(currentChar) || char.IsDigit(currentChar)))
                {
                    appendSpaceToResult();
                    resultBuilder.Append(wordBuilder);
                    // new word
                    wordBuilder.Clear();
                }
                else if (lastCharOfCurrentWord(char.IsUpper)
                    && char.IsLower(currentChar))
                {
                    appendSpaceToResult();
                    resultBuilder.Append(wordBuilder.ToString(0, wordBuilder.Length - 1));
                    // new word
                    var firstCharOfNewWord = wordBuilder[wordBuilder.Length - 1];
                    wordBuilder.Clear();
                    wordBuilder
                        .Append(firstCharOfNewWord);
                }
                wordBuilder.Append(currentChar);
            }
            appendSpaceToResult();
            resultBuilder.Append(wordBuilder);

            var result = resultBuilder[0] +
                resultBuilder.ToString(1, resultBuilder.Length - 1).ToLower();
            return result.Replace(" i ", " I "); // I is an exception
        }

        /// <summary>
        /// Humanizes the input string; e.g. Underscored_input_String_is_turned_INTO_sentence -> 'Underscored input String is turned INTO sentence'
        /// </summary>
        /// <param name="input">The string to be humanized</param>
        /// <returns></returns>
        public static string Humanize(this string input)
        {
            // if input is all capitals (e.g. an acronym) then return it without change
            if (!input.Any(Char.IsLower))
                return input;

            if (input.Contains('_') || input.Contains('-'))
                return FromUnderscoreDashSeparatedWords(input);

            return FromPascalCase(input);
        }

        /// <summary>
        /// Humanized the input string based on the provided casing
        /// </summary>
        /// <param name="input">The string to be humanized</param>
        /// <param name="casing">The desired casing for the output</param>
        /// <returns></returns>
        public static string Humanize(this string input, LetterCasing casing)
        {
            var humanizedString = input.Humanize();

            return ApplyCase(humanizedString, casing);
        }

        /// <summary>
        /// Changes the casing of the provided input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="casing"></param>
        /// <returns></returns>
        public static string ApplyCase(this string input, LetterCasing casing)
        {
            switch (casing)
            {
                case LetterCasing.Title:
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);

                case LetterCasing.LowerCase:
                    return input.ToLower();

                case LetterCasing.AllCaps:
                    return input.ToUpper();

                case LetterCasing.Sentence:
                    if (input.Length >= 1)
                        return string.Concat(input.Substring(0, 1).ToUpper(), input.Substring(1));

                    return input.ToUpper();

                default:
                    throw new ArgumentOutOfRangeException("casing");
            }
        }
    }
}

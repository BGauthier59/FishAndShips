using System.Collections.Generic;
using System.Text;

public static class StringUtils 
{
    private static readonly Dictionary<int, char> NumberToLetter = new Dictionary<int, char>() {
        { 1, 'A' }, { 2, 'B' }, { 3, 'C' }, { 4, 'D' }, { 5, 'E' }, { 6, 'F' }, { 7, 'G' }, { 8, 'H' }, { 9, 'I' },
        { 10, 'J' }, { 11, 'K' }, { 12, 'L' }, { 13, 'M' }, { 14, 'N' }, { 15, 'O' }, { 16, 'P' }, { 17, 'Q' }, { 18, 'R' },
        { 19, 'S' }, { 20, 'T' }, { 21, 'U' }, { 22, 'V' }, { 23, 'W' }, { 24, 'X' }, { 25, 'Y' }, { 0, 'Z' }
    };
    
    public static string NumberToLetterIP(string inputString)
    {
        StringBuilder output = new StringBuilder();

        string[] segments = inputString.Split('.');
        foreach (string segment in segments)
        {
            if (segment.Length == 3)
            {
                int firstTwoDigits = int.Parse(segment.Substring(0, 2));
                int thirdDigit = int.Parse(segment[2].ToString());

                output.Append(NumberToLetter[firstTwoDigits].ToString());
                output.Append(NumberToLetter[thirdDigit].ToString());
                output.Append(".");
            }
            else if (segment.Length == 2)
            {
                int firstTwoDigits = int.Parse(segment.Substring(0, 2));
                if (firstTwoDigits > 25)
                {
                    int firstDigit = int.Parse(segment.Substring(0, 1));
                    int twoDigit = int.Parse(segment.Substring(1, 1));
                    output.Append(NumberToLetter[firstDigit].ToString());
                    output.Append(NumberToLetter[twoDigit].ToString());
                }
                else
                {
                    output.Append(NumberToLetter[firstTwoDigits].ToString());
                }
                output.Append(".");
            }
            else if (segment.Length == 1)
            {
                int firstDigit = int.Parse(segment.Substring(0, 1));
                output.Append(NumberToLetter[firstDigit].ToString());
                output.Append(".");
            }
        }
    
        // remove the last dot if it exists
        if (output.Length > 0 && output[output.Length - 1] == '.')
        {
            output.Remove(output.Length - 1, 1);
        }
    
        return output.ToString();
    }

    
    //private static readonly Dictionary<char, int> LetterToNumber = new Dictionary<char, int>() {
    //    { 'A', 1 }, { 'B', 2 }, { 'C', 3 }, { 'D', 4 }, { 'E', 5 }, { 'F', 6 }, { 'G', 7 }, { 'H', 8 }, { 'I', 9 },
    //    { 'J', 10 }, { 'K', 11 }, { 'L', 12 }, { 'M', 13 }, { 'N', 14 }, { 'O', 15 }, { 'P', 16 }, { 'Q', 17 }, { 'R', 18 },
    //    { 'S', 19 }, { 'T', 20 }, { 'U', 21 }, { 'V', 22 }, { 'W', 23 }, { 'X', 24 }, { 'Y', 25 }, { 'Z', 0 }
    //};

    //public static string LetterToNumberIP(string inputString)
    //{
    //    StringBuilder output = new StringBuilder();
    //
    //    string[] segments = inputString.Split('.');
    //    foreach (string segment in segments)
    //    {
    //        foreach (char c in segment)
    //        {
    //            int digit;
    //            if (LetterToNumber.TryGetValue(c, out digit))
    //            {
    //                output.Append(digit.ToString());
    //            }
    //        }
    //        output.Append(".");
    //    }
    //    return output.ToString().TrimEnd('.');
    //}
    
    private static readonly Dictionary<char, int> LetterToNumber = GetSwizzledDictionary(NumberToLetter);

    private static Dictionary<char, int> GetSwizzledDictionary(Dictionary<int, char> originalDictionary)
    {
        Dictionary<char, int> swizzledDictionary = new Dictionary<char, int>();
        foreach (KeyValuePair<int, char> pair in originalDictionary)
        {
            swizzledDictionary.Add(pair.Value, pair.Key);
        }
        return swizzledDictionary;
    }

    public static string LetterToNumberIP(string inputString)
    {
        StringBuilder output = new StringBuilder();
        if (inputString == null) return "0";

        string[] segments = inputString.Split('.');
        foreach (string segment in segments)
        {
            foreach (char c in segment)
            {
                output.Append(LetterToNumber[c].ToString());
            }
            output.Append(".");
        }

        return output.ToString().TrimEnd('.');
    }
}

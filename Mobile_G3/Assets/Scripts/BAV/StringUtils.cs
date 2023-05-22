using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public static class StringUtils
{
    /* private static readonly Dictionary<int, char> NumberToLetter = new Dictionary<int, char>() {
         { 1, 'A' }, { 2, 'B' }, { 3, 'C' }, { 4, 'D' }, { 5, 'E' }, { 6, 'F' }, { 7, 'G' }, { 8, 'H' }, { 9, 'I' },
         { 10, 'J' }, { 11, 'K' }, { 12, 'L' }, { 13, 'M' }, { 14, 'N' }, { 15, 'O' }, { 16, 'P' }, { 17, 'Q' }, { 18, 'R' },
         { 19, 'S' }, { 20, 'T' }, { 21, 'U' }, { 22, 'V' }, { 23, 'W' }, { 24, 'X' }, { 25, 'Y' }, { 0, 'Z' }
     };*/

    private static readonly Dictionary<char, char> NumberToLetter = new Dictionary<char, char>()
    {
        { '1', 'g' }, { '2', 'h' }, { '3', 'i' }, { '4', 'j' }, { '5', 'k' }, { '6', 'l' }, { '7', 'm' }, { '8', 'n' }, { '9', 'o' },{'0','p'}
    };

    public static string NumberToLetterIP(string inputString)
    {
        StringBuilder output = new StringBuilder();

        IPAddress ipAddress = IPAddress.Parse(inputString);
        byte[] ipAddressBytes = ipAddress.GetAddressBytes();
        string hexIpAddress = BitConverter.ToString(ipAddressBytes).Replace("-", ".").ToLower();


        string[] segments = hexIpAddress.Split('.');

        char letterToAdd;
        foreach (string segment in segments)
        {
            foreach (char letter in segment)
            {
                if (NumberToLetter.ContainsKey(letter))
                {
                    letterToAdd = NumberToLetter[letter];
                    segment.Replace(letter, letterToAdd);
                }
                else
                {
                    letterToAdd = letter;
                }
                output.Append(letterToAdd);
            }
        }
        return output.ToString().ToLower();
    }
    
    private static readonly Dictionary<char, char> LetterToNumber = GetSwizzledDictionary(NumberToLetter);

    private static Dictionary<char, char> GetSwizzledDictionary(Dictionary<char, char> originalDictionary)
    {
        Dictionary<char, char> swizzledDictionary = new Dictionary<char, char>();
        foreach (KeyValuePair<char, char> pair in originalDictionary)
        {
            swizzledDictionary.Add(pair.Value, pair.Key);
        }
        return swizzledDictionary;
    }
    
    public static string LetterToNumberIP(string inputString)
    {
        StringBuilder outputString = new StringBuilder();
        string dot =".";
        for (int i = 0; i < 8; i++)
        {
            if (LetterToNumber.ContainsKey(inputString[i]))
            {
                outputString.Append(LetterToNumber[inputString[i]]);
            }
            else
            {
                outputString.Append(inputString[i]);
            }

            if (i == 1 || i == 3 || i == 5)
            {
                outputString.Append('.');
            }
        }
        string output = outputString.ToString().ToUpper();
        string[] segments = output.Split('.');
        StringBuilder finalOutput = new StringBuilder();
        foreach (string segment in segments)
        {
            int intValue = Convert.ToInt32(segment, 16);
            finalOutput.Append(intValue.ToString());
            finalOutput.Append('.');
        }

        return finalOutput.ToString().TrimEnd('.');

        /*
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

        return output.ToString().TrimEnd('.');*/
    }
    
    
}

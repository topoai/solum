using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class StringExtensions
{
    public static string format(this string input, params string[] args)
    {
        return string.Format(input, args);
    }
    public static string format(this string input, params object[] args)
    {
        return string.Format(input, args);
    }
    public static Stream toStream(this string input, Encoding encoding)
    {
        var stream = new MemoryStream();

        using (var writer = new StreamWriter(stream, encoding, 4096, leaveOpen: true))
            writer.Write(input);

        stream.Position = 0;

        return stream;
    }
    public static Stream toStream(this string input)
    {
        return toStream(input, Encoding.Default);
    }
    public class LineDifference
    {
        public LineDifference(string compareTo, string compareWith)
        {
            this.CompareTo = compareTo;
            this.CompareWith = compareWith;
        }

        public string CompareWith { get; private set; }
        public string CompareTo { get; private set; }
    }
    public static List<LineDifference> linesDiff(this string compareWith, string compareTo)
    {
        // ** Split the strings by the new-line character
        var compareWithLines = compareWith.Split('\n');
        var compareToLines = compareTo.Split('\n');

        // ** Create a list that will contain the lines differences
        List<LineDifference> linesThatDiff = new List<LineDifference>();
        
        for (var index = 0; index < compareWithLines.Length; index++)
        {
            // ** Check if output still has a line to compare
            if (index >= compareToLines.Length)
            {
                linesThatDiff.Add(new LineDifference(compareWithLines[index], string.Empty));
                continue;
            }            
            
            // ** Compare the two lines
            if (compareWithLines[index] != compareToLines[index])
                linesThatDiff.Add(new LineDifference(compareWithLines[index], compareToLines[index]));
        }

        return linesThatDiff;
    }
}
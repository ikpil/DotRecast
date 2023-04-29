using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class Program
{
  private static List<FileLocation> CreateLocations(string path)
  {
    List<string> list = ((IEnumerable<string>) File.ReadAllLines(path)).Where<string>((Func<string, bool>) (line => line.Contains("error CS1061: "))).ToList<string>();
    List<FileLocation> locations = new List<FileLocation>();
    foreach (string input in list)
    {
      string pattern = "([A-Za-z]:\\\\[^\\\\]+\\\\[^\\\\]+(?:\\\\[^\\\\]+)*\\\\[^\\\\]+\\.cs)\\((\\d+),(\\d+)\\)";
      Match match = Regex.Match(input, pattern);
      if (match.Success)
      {
        string str1 = match.Groups[1].Value;
        string s1 = match.Groups[2].Value;
        string s2 = match.Groups[3].Value;
        string str2 = "";
        if (input.Contains("'x'에 대한"))
          str2 = "x";
        if (input.Contains("'y'에 대한"))
          str2 = "y";
        if (input.Contains("'z'에 대한"))
          str2 = "z";
        FileLocation fileLocation = new FileLocation()
        {
          Path = str1,
          Line = int.Parse(s1),
          Column = int.Parse(s2),
          Letter = str2
        };
        locations.Add(fileLocation);
      }
    }
    return locations;
  }

  private static void Change(FileLocation location)
  {
    string[] contents = File.ReadAllLines(location.Path);
    List<string> list = ((IEnumerable<char>) contents[location.Line - 1].ToCharArray()).Select<char, string>((Func<char, string>) (x => x.ToString() ?? "")).ToList<string>();
    list[location.Column - 2] = "[";
    if (location.Letter == "x")
      list[location.Column - 1] = "0]";
    else if (location.Letter == "y")
      list[location.Column - 1] = "1]";
    else if (location.Letter == "z")
      list[location.Column - 1] = "2]";
    string str = string.Join("", (IEnumerable<string>) list);
    contents[location.Line - 1] = str;
    //File.WriteAllLines(location.Path, contents);
  }

  public static void Main(string[] args)
  {
    var locations = Program.CreateLocations("../../../../../error.log");
    foreach (FileLocation location in locations)
      Program.Change(location);
  }
}
    
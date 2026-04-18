using System.Text;
using System.Text.RegularExpressions;

namespace Mp3Extractor.Utilities;

internal static class TitleService
{
    internal static async Task<string?> GetTitleAsync(string url)
    {
        using var http = new HttpClient();

        // Some sites (like YouTube) require a User-Agent
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        var html = await http.GetStringAsync(url);

        var match = Regex.Match(html, @"<title>\s*(.+?)\s*</title>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (!match.Success)
            return null;

        var title = match.Groups[1].Value;

        // YouTube titles often end with " - YouTube"
        title = title.Replace(" - YouTube", "").Trim();

        var decoded = System.Net.WebUtility.HtmlDecode(title);

        var safeTitle = string.Concat(decoded!.Split(Path.GetInvalidFileNameChars()));

        var latinTitle = CyrillicToLatin(safeTitle);

        return latinTitle;
    }

    private static string CyrillicToLatin(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var map = new Dictionary<char, string>
        {
            // Uppercase
            ['А'] = "A",
            ['Б'] = "B",
            ['В'] = "V",
            ['Г'] = "G",
            ['Д'] = "D",
            ['Е'] = "E",
            ['Ж'] = "Zh",
            ['З'] = "Z",
            ['И'] = "I",
            ['Й'] = "Y",
            ['К'] = "K",
            ['Л'] = "L",
            ['М'] = "M",
            ['Н'] = "N",
            ['О'] = "O",
            ['П'] = "P",
            ['Р'] = "R",
            ['С'] = "S",
            ['Т'] = "T",
            ['У'] = "U",
            ['Ф'] = "F",
            ['Х'] = "H",
            ['Ц'] = "Ts",
            ['Ч'] = "Ch",
            ['Ш'] = "Sh",
            ['Щ'] = "Sht",
            ['Ъ'] = "A",
            ['Ь'] = "",
            ['Ю'] = "Yu",
            ['Я'] = "Ya",

            // Lowercase
            ['а'] = "a",
            ['б'] = "b",
            ['в'] = "v",
            ['г'] = "g",
            ['д'] = "d",
            ['е'] = "e",
            ['ж'] = "zh",
            ['з'] = "z",
            ['и'] = "i",
            ['й'] = "y",
            ['к'] = "k",
            ['л'] = "l",
            ['м'] = "m",
            ['н'] = "n",
            ['о'] = "o",
            ['п'] = "p",
            ['р'] = "r",
            ['с'] = "s",
            ['т'] = "t",
            ['у'] = "u",
            ['ф'] = "f",
            ['х'] = "h",
            ['ц'] = "ts",
            ['ч'] = "ch",
            ['ш'] = "sh",
            ['щ'] = "sht",
            ['ъ'] = "a",
            ['ь'] = "",
            ['ю'] = "yu",
            ['я'] = "ya"
        };

        var result = new StringBuilder();

        foreach (var ch in text)
        {
            if (map.TryGetValue(ch, out var latin))
                result.Append(latin);
            else
                result.Append(ch); // keep symbols, numbers, spaces
        }

        return result.ToString();
    }
}

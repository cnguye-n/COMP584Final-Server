using System.Text;

namespace COMP584Server.Data.Seed
{
    public static class CsvMini
    {
        public static List<Dictionary<string, string>> Read(string path)
        {
            var lines = File.ReadAllLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (lines.Count < 2) return new();

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var rows = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Count; i++)
            {
                var cols = lines[i].Split(',').Select(c => c.Trim()).ToArray();
                if (cols.Length != headers.Length) continue;

                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < headers.Length; c++)
                    dict[headers[c]] = cols[c];

                rows.Add(dict);
            }

            return rows;
        }
    }
}

using System.Text.Json;

namespace BeatblockLevelMaker
{
    public class LevelJsonHelper
    {
        private JsonElement root;

        public List<JsonElement> GetEvents(string levelPath)
        {
            string levelJsonPath = $@"{levelPath}\level.json";
            string jsonText = File.ReadAllText(levelJsonPath);

            JsonDocument doc = JsonDocument.Parse(jsonText);
            JsonElement root = doc.RootElement;

            this.root = root;

            return root.GetProperty("events").EnumerateArray().ToList();
        }
        public void AppendColorEvent(string levelPath, JsonSerializerOptions options)
        {
            try
            {
                var events = GetEvents(levelPath);

                bool hasColorEvent = events.Any(e => e.TryGetProperty("type", out var t) && t.GetString() == "setColor");
                int playEventInd = events.FindIndex(e => e.TryGetProperty("type", out var t) && t.GetString() == "play");

                Random rand = new();

                int r = rand.Next(10, 250);
                int g = rand.Next(10, 250);
                int b = rand.Next(10, 250);

                int darkr = Math.Clamp(r - rand.Next(20, 100), 0, 255);
                int darkg = Math.Clamp(g - rand.Next(20, 100), 0, 255);
                int darkb = Math.Clamp(b - rand.Next(20, 100), 0, 255);

                var color1 = JsonSerializer.Deserialize<JsonElement>($$"""
                    {"angle":0,"r":{{r}},"g":{{g}},"b":{{b}},"color":0,"order":0,"time":-1,"type":"setColor"}
                    """);

                var color2 = JsonSerializer.Deserialize<JsonElement>($$"""
                    {"angle":0,"r":{{darkr}},"g":{{darkg}},"b":{{darkb}},"color":1,"order":0,"time":-1,"type":"setColor"}
                    """);

                if (hasColorEvent)
                {
                    var colorEvent1 = events.First(e => e.TryGetProperty("type", out var t) && t.GetString() == "setColor" && e.GetProperty("color").GetInt32() == 0);
                    var colorEvent2 = events.First(e => e.TryGetProperty("type", out var t) && t.GetString() == "setColor" && e.GetProperty("color").GetInt32() == 1);
                    events.Remove(colorEvent1);
                    events.Remove(colorEvent2);
                }

                events.Insert(playEventInd + 1, color1);
                events.Insert(playEventInd + 2, color2);

                var newRoot = new Dictionary<string, object>();

                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name == "events") newRoot["events"] = events;
                    else newRoot[prop.Name] = JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
                }

                string output = JsonSerializer.Serialize(newRoot, options);
                File.WriteAllText($@"{levelPath}\level.json", output);
            }
            catch
            {
                return;
            }
        }
    }
}
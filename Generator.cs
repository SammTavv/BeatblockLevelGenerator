using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatblockLevelMaker
{
    public class Generator
    {
        private static LevelJsonHelper jsonHelper = new();
        private static bool canPlaceFirst;
        private static bool canCloseMenu;
        private static string midiPath;
        private static string songPath;
        private static string levelPath;
        private static bool usingMod;
        private static bool showMenu = true;
        private static bool appendLevelData;
        private static bool canGenerate = true;
        private static readonly string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.txt");

        private static JsonSerializerOptions options = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static void Intro(int page = 0)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Beatblock Level Generator\n");
            Console.ResetColor();

            if (page == 1)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Beatblock Level Generator\n");

                Console.ForegroundColor = ConsoleColor.Green;
                if (canGenerate)
                {
                    Console.WriteLine("Notes generated.");
                }
                else
                {
                    Console.WriteLine($"Level loaded: Custom Levels/{levelPath.Split('\\').Last()}");
                    canGenerate = true;
                }
                Console.ResetColor();

                Console.WriteLine("" +
                    "Type \"r\" to regenerate notes." +
                    "\nType \"c\" to change the selected level." +
                    "\nType \"s\" to open generator settings." +
                    "\nType \"g\" to reset generator settings." +
                    "\n\nNote: to view changes reload the Custom Levels page.");
            }

            if (page == 2)
            {
                // -- MIDI --
                Intro();
                Console.WriteLine("Insert the midi path (.mid):");

                string midi = Console.ReadLine().Trim('"');

                midiPath = Path.GetFullPath(midi);

                File.Copy(midiPath, $@"{levelPath}\mus.mid", true);
            }

            if (page == 3)
            {
                // -- SONG --
                Intro();
                Console.WriteLine("Insert the song path (.ogg):");

                string song = Console.ReadLine().Trim('"');

                songPath = Path.GetFullPath(song);

                File.Copy(songPath, $@"{levelPath}\mus.ogg", true);

                // -- BPM --
                Intro();
                Console.WriteLine("Insert the song BPM:");

                string bpmInput = Console.ReadLine();

                float bpm = float.Parse(bpmInput);

                File.AppendAllText($@"{levelPath}\level.json", $"" +
                    $"{{\"events\":[{{\"angle\":0,\"bpm\":{bpm},\"file\":\"mus.ogg\",\"time\":0,\"type\":\"play\",\"volume\":1}}\r\n,");
            }
        }
        private static void BeginMenu()
        {
            usingMod = false;
            canCloseMenu = false;
            appendLevelData = true;
            string baseDir = AppContext.BaseDirectory;

            if (!File.Exists(settingsPath)) WriteSettings();

            if (File.Exists("path.txt"))
            {
                string path = File.ReadAllText("path.txt").Trim('"');

                if (Directory.Exists(path))
                {
                    levelPath = path;
                    midiPath = Path.GetFullPath($@"{levelPath}\mus.mid");

                    canGenerate = false;
                    canCloseMenu = true;
                    showMenu = false;
                    return;
                }
            }

            while (!canCloseMenu)
            {
                try
                {
                    // -- NAME --
                    Intro();
                    Console.WriteLine("Insert the level name:");

                    string name = Console.ReadLine();

                    levelPath = Path.GetFullPath(Path.Combine(baseDir,
                        $@"C:\Users\{Environment.UserName}\AppData\Roaming\beatblock\Custom Levels", name));

                    if (Directory.Exists(levelPath))
                    {
                        Intro();
                        Console.WriteLine("Replace existing level (true/false):");

                        bool replace = bool.Parse(Console.ReadLine());

                        if (!replace)
                        {
                            midiPath = Path.GetFullPath($@"{levelPath}\mus.mid");
                            File.WriteAllText("path.txt", levelPath);

                            canGenerate = false;
                            canCloseMenu = true;
                            showMenu = false;
                            return;
                        }
                        else
                        {
                            Directory.Delete(levelPath, true);
                        }
                    }

                    Directory.CreateDirectory(levelPath);

                    File.WriteAllText($@"{levelPath}\chart-var.json", "");
                    File.WriteAllText($@"{levelPath}\manifest.json", $"" +
                        $"{{\"defaultVariant\":\"var\",\"metadata\":{{\"artist\":\"Artist\",\"artistLink\":\"\",\"bg\":false,\"charter\":\"Charter\",\"description\":\"\",\"difficulty\":0,\"lightWarning\":false,\"loopPointsEnable\":false,\"lyricsWarning\":false,\"songName\":\"{name}\"}}\r\n,\"properties\":{{\"formatversion\":19}}\r\n,\"variants\":[{{\"charter\":\"\",\"difficulty\":0,\"display\":\"var\",\"hidden\":false,\"name\":\"var\",\"slot\":0}}\r\n]}}");
                    File.WriteAllText($@"{levelPath}\level.json", "");

                    ProcessStartInfo psi = new()
                    {
                        FileName = levelPath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);

                    // -- MIDI --
                    Intro(2);

                    // -- SONG --
                    Intro(3);

                    // -- USING BEATTOOLS --
                    /*Intro();
                    Console.WriteLine("Is the \"BeatTools\" mod enabled (true/false):");

                    string modInput = Console.ReadLine();

                    usingMod = bool.Parse(modInput);*/

                    // -- END MENU --
                    File.WriteAllText("path.txt", levelPath);
                    canCloseMenu = true;
                    showMenu = false;
                }
                catch
                {
                    Intro();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error parsing values.\n");
                    Console.ResetColor();
                }
            }
        }
        public static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            if (showMenu) BeginMenu();

            if (!File.Exists(midiPath))
            {
                Intro();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("MIDI path unavailable.\nRestart the program to select a new level.");
                Console.ResetColor();
                File.Delete("path.txt");
                Console.ReadLine();

                return;
            }

            canPlaceFirst = true;
            lastAngle = null;

            MidiFile midi = MidiFile.Read($@"{levelPath}\mus.mid");
            TempoMap tempoMap = midi.GetTempoMap();

            var midiNotes = midi.GetNotes().OrderBy(n => n.Time).ToList();

            Random rand = new();
            List<NoteEvent> objects = [];

            float skiptime = 0f;

            for (int i = 0; i < midiNotes.Count; i++)
            {
                float currTime = (float)GetBeatTime(midiNotes[i], tempoMap);

                if (currTime <= skiptime && !canPlaceFirst) continue;

                float nextTime = currTime + 1f;

                for (int j = i + 1; j < midiNotes.Count; j++)
                {
                    float t = (float)GetBeatTime(midiNotes[j], tempoMap);

                    if (t > currTime)
                    {
                        nextTime = t;
                        break;
                    }
                }

                var note = new NoteEvent
                {
                    time = currTime,
                    angle = PickAngle(rand),
                    type = PickNoteType(rand, midiNotes[i].Channel)
                };

                switch (note.type)
                {
                    case "hold":
                    case "mineHold":
                        note.duration = nextTime - currTime;
                        skiptime = nextTime;
                        break;

                    case "bounce":
                        {
                            float window = nextTime - currTime;
                            int bounces = (int)GetSetting("max bounces");
                            float delay = window / bounces;
                            note.bounces = bounces;
                            note.delay = delay;
                            note.tap = CanTap(rand);
                            note.rotation = 0;
                            skiptime = currTime + (bounces * delay);
                            break;
                        }

                    default:
                        skiptime = currTime;
                        break;
                }

                SetData(note, rand);
                objects.Add(note);
                canPlaceFirst = false;
            }

            if (canGenerate)
            {
                string json = JsonSerializer.Serialize(objects, options);
                File.WriteAllText($@"{levelPath}\chart-var.json", json);

                if (GetSetting("random colors") != 0) jsonHelper.AppendColorEvent(levelPath, options);

                if (appendLevelData)
                {
                    File.AppendAllText($@"{levelPath}\level.json", $"" +
                    $"{{\"angle\":0,\"time\":{objects.Last().time + 3f},\"type\":\"showResults\"}}\r\n]");

                    if (usingMod)
                    {
                        File.AppendAllText($@"{levelPath}\level.json", $"" +
                            $",\"properties\":{{\"beattools\":{{\"eventGroups\":{{\"all\":{{\"events\":{{\"advancetextdeco\":true,\"aft\":true,\"block\":true,\"bookmark\":true,\"bounce\":true,\"deco\":true,\"ease\":true,\"easeSequence\":true,\"extraTap\":true,\"forcePlayerSprite\":true,\"hold\":true,\"hom\":true,\"inverse\":true,\"mine\":true,\"mineHold\":true,\"noise\":true,\"outline\":true,\"paddles\":true,\"play\":true,\"playSound\":true,\"retime\":true,\"setBPM\":true,\"setBgColor\":true,\"setBoolean\":true,\"setBounceHeight\":true,\"setColor\":true,\"showResults\":true,\"side\":true,\"songNameOverride\":true,\"tag\":true,\"textdeco\":true,\"toggleParticles\":true}}\r\n,\"index\":0,\"name\":\"all\",\"visibility\":\"show\"}}\r\n,\"bookmarks\":{{\"events\":{{\"bookmark\":true}}\r\n,\"index\":3,\"name\":\"bookmarks\",\"visibility\":\" - \"}}\r\n,\"color\":{{\"events\":{{\"hom\":true,\"noise\":true,\"outline\":true,\"setBgColor\":true,\"setColor\":true}}\r\n,\"index\":2,\"name\":\"color\",\"visibility\":\" - \"}}\r\n,\"deco\":{{\"events\":{{\"advancetextdeco\":true,\"deco\":true,\"textdeco\":true}}\r\n,\"index\":3,\"name\":\"deco\",\"visibility\":\" - \"}}\r\n,\"eases\":{{\"events\":{{\"ease\":true,\"easeSequence\":true,\"setBoolean\":true}}\r\n,\"index\":3,\"name\":\"eases\",\"visibility\":\" - \"}}\r\n,\"gameplay\":{{\"events\":{{\"block\":true,\"bounce\":true,\"extraTap\":true,\"hold\":true,\"inverse\":true,\"mine\":true,\"mineHold\":true,\"paddles\":true,\"setBounceHeight\":true,\"side\":true}}\r\n,\"index\":1,\"name\":\"gameplay\",\"visibility\":\" - \"}}\r\n,\"song\":{{\"events\":{{\"play\":true,\"playSound\":true,\"retime\":true,\"setBPM\":true,\"showResults\":true}}\r\n,\"index\":1,\"name\":\"song\",\"visibility\":\" - \"}}\r\n,\"tags\":{{\"events\":{{\"tag\":true}}\r\n,\"index\":3,\"name\":\"tags\",\"visibility\":\" - \"}}\r\n,\"visuals\":{{\"events\":{{\"advancetextdeco\":true,\"aft\":true,\"deco\":true,\"ease\":true,\"easeSequence\":true,\"forcePlayerSprite\":true,\"hom\":true,\"noise\":true,\"outline\":true,\"setBgColor\":true,\"setBoolean\":true,\"setColor\":true,\"songNameOverride\":true,\"textdeco\":true,\"toggleParticles\":true}}\r\n,\"index\":1,\"name\":\"visuals\",\"visibility\":\" - \"}}\r\n}}\r\n}}\r\n");
                    }

                    File.AppendAllText($@"{levelPath}\level.json", $"" +
                        $",\"properties\":{{\"formatversion\":19,\"offset\":8,\"speed\":70}}\r\n}}\r\n\r\n");
                }
            }
            appendLevelData = false;

            Intro(1);

            while (true)
            {
                string inp = Console.ReadLine();

                if (inp == "r")
                {
                    canGenerate = true;
                    Main();
                    return;
                }

                if (inp == "c")
                {
                    showMenu = true;
                    File.Delete("path.txt");
                    Main();
                    return;
                }

                if (inp == "s")
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = settingsPath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);

                    canGenerate = false;
                    Intro(1);
                }

                if (inp == "g")
                {
                    WriteSettings();

                    canGenerate = false;
                    Intro(1);
                }

                /*if (inp == "cm")
                {
                    canGenerate = false;
                    Intro(2);
                }

                if (inp == "cs")
                {
                    canGenerate = false;
                    Intro(3);
                }*/
            }
        }

        private static double GetBeatTime(Note note, TempoMap tempoMap)
        {
            var ticksPerBeat = ((TicksPerQuarterNoteTimeDivision)
                tempoMap.TimeDivision).TicksPerQuarterNote;

            return (double)note.Time / ticksPerBeat;
        }
        private static string PickNoteType(Random rand, int channel)
        {
            if (GetSetting("use midi channels") != 1)
            {
                (string, string)[] noteTypes =
                [
                    ("extraTap", "extra tap chance"),
                ("block", "block chance"),
                ("bounce", "bounce chance"),
                ("hold", "hold chance"),
                ("mineHold", "mine hold chance"),
                ("mine", "mine chance"),
                ("side", "side chance"),
                ("inverse", "inverse chance")
                ];

                List<string> possibleTypes = [];

                foreach (var (type, setting) in noteTypes)
                {
                    float chance = GetSetting(setting);
                    if (rand.Next(0, 100) < chance)
                    {
                        possibleTypes.Add(type);
                    }
                }

                if (possibleTypes.Count == 0) return "";

                return possibleTypes[rand.Next(possibleTypes.Count)];
            }
            else
            {
                return channel switch
                {
                    0 => "block",
                    1 => "bounce",
                    2 => "hold",
                    3 => "mineHold",
                    4 => "mine",
                    5 => "side",
                    6 => "inverse",
                    7 => "extraTap",
                    _ => "block"
                };
            }
        }

        private static int PickNeg(Random rand)
        {
            if (rand.Next(2, 5) > 3)
                return 1;
            else
                return -1;
        }

        private static bool CanTap(Random rand) => rand.NextDouble() < GetSetting("has tap chance");

        private static float? lastAngle = null;
        private static float PickAngle(Random rand)
        {
            float spread = GetSetting("max angle spread");

            if (spread <= 0 || lastAngle == null)
            {
                float angle = (float)(rand.NextDouble() * 360f);
                lastAngle = angle;
                return angle;
            }

            spread = Math.Clamp(spread, 0f, 360f);

            float min = lastAngle.Value - spread;
            float max = lastAngle.Value + spread;

            float newAngle = (float)(min + rand.NextDouble() * (max - min));

            newAngle = (newAngle % 360 + 360) % 360;

            lastAngle = newAngle;
            return newAngle;
        }

        private static float holdLeniency;
        private static void SetData(NoteEvent note, Random rand)
        {
            holdLeniency = GetSetting("hold angle difficulty");

            float baseAngle = note.angle;
            float diff;

            switch (note.type)
            {
                case "hold":
                    note.startTap = CanTap(rand);
                    note.endTap = CanTap(rand);

                    //note.angle = (float)(rand.NextDouble() * 360);
                    diff = note.duration.Value / 0.5f * holdLeniency;
                    note.angle2 = baseAngle + diff * PickNeg(rand);
                    break;

                case "mineHold":
                    //note.angle = (float)(rand.NextDouble() * 360);
                    diff = note.duration.Value / 0.5f * holdLeniency;
                    note.angle2 = baseAngle + diff * PickNeg(rand);
                    break;

                case "block":
                case "inverse":
                case "side":
                    note.tap = CanTap(rand);
                    break;
            }
        }

        private static void WriteSettings()
        {
            File.WriteAllText(settingsPath, "" +
                "extra tap chance = 100" +
                "\nblock chance = 100" +
                "\nbounce chance = 100" +
                "\nhold chance = 100" +
                "\nmine hold chance = 100" +
                "\nmine chance = 100" +
                "\nside chance = 100" +
                "\ninverse chance = 100" +
                "\nhas tap chance = 0.4" +
                "\nhold angle difficulty = 70" +
                "\nmax bounces = 1" +
                "\nmax angle spread = 0" +
                "\nrandom colors = 1" +
                "\nuse midi channels = 0");
        }

        private static float GetSetting(string name)
        {
            foreach (string line in File.ReadAllLines(settingsPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('=', StringSplitOptions.TrimEntries);
                if (parts.Length != 2) continue;

                if (string.Equals(parts[0], name, StringComparison.OrdinalIgnoreCase)) return float.Parse(parts[1], CultureInfo.InvariantCulture);
            }

            return 0.1f;
        }
    }
}
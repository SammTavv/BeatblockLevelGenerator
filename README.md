# Beatblock Level Generator
A console application that automatically generates custom levels for Beatblock.

### Setup
Install the latest release and run `BeatblockLevelGenerator.exe`.\
You will be prompted to write the level name, this will generate a new folder in `AppData/Roaming/beatblock/Custom Levels` if it doesn't already exist.\
After that, you'll have to include a MIDI (.mid) file path and a song (.ogg) file path, as well as the song's BPM. The program will copy the files to the level folder.\
After it has successfully generated notes, refresh the Custom Levels page to view changes.\
(To clarify: **MIDI notes** are the notes placed in the midi file while **notes** are the entities in the Beatblock level)

### Functionality
The program will place [notes](https://docs.google.com/document/d/1myd85jpTYQCtE1Mlu6fJEm3fJEIN_-7Q1TFcB1OcrT4/edit?pli=1&tab=t.0#heading=h.o5fxa6zbzwv3)
based on MIDI notes, randomly choosing an angle.\
Some notes will have specific properties based on MIDI notes, for example, **Bounce**s, **Hold**s and **Minehold**s will end on the next note.\
MIDI tempo and swing changes won't affect note placements.

### Generator settings
You can change the level generation settings by typing `s` in the console. Here are the available settings:
- extra tap chance: the chance of an **Extra Tap** being placed (0-100)
- block chance: the chance of a **Block** being placed (0-100)
- bounce chance: the chance of a **Bounce** being placed (0-100)
- hold chance: the chance of a **Hold** being placed (0-100)
- mine hold chance: the chance of a **Minehold** being placed (0-100)
- mine chance: the chance of a **Mine** being placed (0-100)
- side chance: the chance of a **Side** being placed (0-100)
- inverse chance: the chance of an **Inverse Block** being placed (0-100)
- has tap chance: the chance of any note to require a press (0-1)
- hold angle difficulty: the maximum rotation of a 0.5 beats **Hold**
- max bounces: how many times **Bounce** notes have to bounce before the next note
- max angle spread: the maximum angle between every note (0 for no angle spread)
- random colors: whether to randomly generate the level colors (0 for false, 1 for true)
- use midi channels: places different notes based on midi channels (0 for false, 1 for true)

## MIDI channels
These are what every MIDI channel represents if the `use midi channels` setting is active:
- 0: **Block**
- 1: **Bounce**
- 2: **Hold**
- 3: **Minehold**
- 4: **Mine**
- 5: **Side**
- 6: **Inverse**
- 7: **Extra Tap**
If your DAW starts at 1, just add 1 to your desired channel (for example MIDI channel 3 is the **Hold** note)

### Extra
In the release i included a demo level made with Beatblock Level Generator using the default settings. You can copy the midi and song files to test how it works!\
Actually nevermind i forgot to update the demo so i removed it entirely lmao\
Have fun !!!!!

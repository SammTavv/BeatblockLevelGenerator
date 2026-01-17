using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatblockLevelMaker
{
    public class NoteEvent
    {
        public float time { get; set; }
        public string type { get; set; }
        public float angle { get; set; }
        public float? angle2 { get; set; }
        public float? duration { get; set; }
        public int? bounces { get; set; }
        public float? delay { get; set; }
        public float? rotation { get; set; }
        public bool? tap { get; set; }
        public bool? startTap { get; set; }
        public bool? endTap { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace CivSim
{
    public class CivEvent
    {
        public Stat Stat { get; set; }
        public int Effect { get; set; }
        public string FlavourText { get; set; }
        public DateTime Expiry { get; set; }

        // Makes JSON deserializer happy
        public CivEvent() { }

        public CivEvent(CivEvent from)
        {
            Stat = from.Stat;
            Effect = from.Effect;
            FlavourText = from.FlavourText;
            Expiry = DateTime.Now.AddDays(7);
        }

        public static Dictionary<string, CivEvent> ShopEvents = new Dictionary<string, CivEvent>() {
            { "evt_offence1", new CivEvent(){ Stat = Stat.Offence, Effect = -1, FlavourText = "Sensitive information leaked" } },
            { "evt_offence2", new CivEvent(){ Stat = Stat.Offence, Effect = -2, FlavourText = "Outdated military technology" } },
            { "evt_offence3", new CivEvent(){ Stat = Stat.Offence, Effect = -4, FlavourText = "Threat of nuclear warfare" } },

            { "evt_defence1", new CivEvent(){ Stat = Stat.Defence, Effect = -1, FlavourText = "Whistleblowing" } },
            { "evt_defence2", new CivEvent(){ Stat = Stat.Defence, Effect = -2, FlavourText = "Foreign espionage" } },
            { "evt_defence3", new CivEvent(){ Stat = Stat.Defence, Effect = -4, FlavourText ="Cyberwarfare" } },

            { "evt_research1", new CivEvent(){ Stat = Stat.Research, Effect = -1, FlavourText = "Lack of public interest" } },
            { "evt_research2", new CivEvent(){ Stat = Stat.Research, Effect = -2, FlavourText = "Corporate greed" } },
            { "evt_research3", new CivEvent(){ Stat = Stat.Research, Effect = -4, FlavourText = "Political deadlock" } },

            { "evt_education1", new CivEvent(){ Stat = Stat.Education, Effect = -1, FlavourText = "Teacher strike" } },
            { "evt_education2", new CivEvent(){ Stat = Stat.Education, Effect = -2, FlavourText = "Disinformation campaign" } },
            { "evt_education3", new CivEvent(){ Stat = Stat.Education, Effect = -4, FlavourText = "Student mental health crisis" } },

            { "evt_healthcare1", new CivEvent(){ Stat = Stat.Healthcare, Effect = -1, FlavourText = "Flu outbreak" } },
            { "evt_healthcare2", new CivEvent(){ Stat = Stat.Healthcare, Effect = -2, FlavourText = "Medical equipment shortage" } },
            { "evt_healthcare3", new CivEvent(){ Stat = Stat.Healthcare, Effect = -4, FlavourText = "Epidemic" } },

            { "evt_civilian1", new CivEvent(){ Stat = Stat.Civilian, Effect = -1, FlavourText = "iris please add this" } },
            { "evt_civilian2", new CivEvent(){ Stat = Stat.Civilian, Effect = -2, FlavourText = "iris please add this" } },
            { "evt_civilian3", new CivEvent(){ Stat = Stat.Civilian, Effect = -4, FlavourText = "iris please add this" } },

            { "evt_morale1", new CivEvent(){ Stat = Stat.Morale, Effect = -1, FlavourText = "iris please add this" } },
            { "evt_morale2", new CivEvent(){ Stat = Stat.Morale, Effect = -2, FlavourText = "iris please add this" } },
            { "evt_morale3", new CivEvent(){ Stat = Stat.Morale, Effect = -4, FlavourText = "iris please add this" } },
        };
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Audio
{
    class AudioLibrary
    {
        #region Characters
        public class SpeechCharacter
        {
            public string Name = "";
            public string Ident = "";
            public int ID = 0;
            public List<SpeechMessage> Messages = null;
        }

        public class SpeechMessage
        {
            public string ID = "";
            public List<SpeechFile> Files = null;
        }

        public class SpeechFile
        {
            public string File = "";
            public string Caption = "";
        }
        #endregion

        #region Effects
        public static List<EffectFile> EffectsLibrary = new List<EffectFile>()
        {
            new EffectFile()
            {
                File = "KACHING.mp3",
            },
            new EffectFile()
            {
                File = "THROAT.mp3",
            },
            new EffectFile()
            {
                File = "SHUTTER.mp3",
            },
            new EffectFile()
            {
                File = "TEST.mp3",
            },
        };

        public class EffectFile
        {
            public string File = "";
        }
        #endregion
        
    }
}

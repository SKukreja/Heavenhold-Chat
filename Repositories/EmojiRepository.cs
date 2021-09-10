using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenholdBot.Repositories
{
    public class EmojiRepository
    {
        private Dictionary<string, string> emojiList;
        private Dictionary<string, uint> colorRoleList;

        public EmojiRepository()
        {
            //Static IDs associated to each emoji on the server
            emojiList = new Dictionary<string, string>() {
                    { "Basic", "<:basic:783385194869686324>" },
                    { "Earth", "<:earth:783385243231060009>" },
                    { "Dark", "<:dark:783385232070017034>" },
                    { "Light", "<:light:783385268374732850>" },
                    { "Water", "<:water:783385918106107925>" },
                    { "Fire", "<:fire:783385255184039956>" },
                    { "Ranged", "<:ranged:783385281708556328>" },
                    { "Warrior", "<:warrior:783385928856895539>" },
                    { "Tanker", "<:tank:783385951908397066>" },
                    { "Support", "<:support:783385976402739301>" },
                    { "All", "<:all:783385308271476767>" },
                    { "Airborne", "<:airborne:783385321038807040>" },
                    { "Injured", "<:injured:783385374033969206>" },
                    { "Downed", "<:downed:783385358741274644>" },
                    { "Crowns", "<:crowns:802779385757171753>" },
                    { "None", "N/A" }
                };
            colorRoleList = new Dictionary<string, uint>() {
                    { "Basic", 0x707070 },
                    { "Earth", 0xf5b800 },
                    { "Dark", 0x220036 },
                    { "Light", 0xcb3636 },
                    { "Water", 0x2d84bc },
                    { "Fire", 0x8f4f1a },
            };
        }

        public uint GetRoleColorCode(string roleName)
        {
            return colorRoleList[roleName];
        }

        public string GetEmojiCode(string emojiName)
        {
            return emojiList[emojiName];
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    public class UserGroup
    {
        [JsonProperty("idgroup")]
        public Guid IdGroup { get; set; }
        [JsonProperty("iduser")]
        public Guid IdUser { get; set; }
        [JsonProperty("pending")]
        public bool Pending { get; set; }
        [JsonProperty("withsessiondata")]
        public bool WithSessionData { get; set; } // zit niet in de database, is voor een CASE bij GetAllFriendsFromUserdata


    }
}

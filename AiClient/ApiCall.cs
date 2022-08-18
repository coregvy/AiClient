using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AiClient
{
    public class ApiCall
    {
        private const string URL_ROOM_INFO = "https://www.tomiko.cf/red/api/con4/room";

        internal static Task<List<RoomInfo>> GetAllRoom()
        {
            return Task.Run(async () =>
            {
                var client = new HttpClient();
                var res = await client.GetAsync(URL_ROOM_INFO);
                var body = await res.Content.ReadAsStreamAsync();

                return JSON.Parse<List<RoomInfo>>(body);
            });
        }
    }

    [DataContract]
    public class RoomInfo
    {
        public static IEnumerable<RoomInfo> ReloadRoom { get
            {
                return new RoomInfo[] { new RoomInfo { Label = " - 取得中", Using = -1 } };
            }
        }
        private static string UsingLabel(int u) => u == 0 ? "利用可能" : (u == 1 ? "利用中" : (u==-1?"":"エラー"));
        [DataMember(Name = "path")]
        public string Path { get; set; }
        [DataMember(Name = "using")]
        public int Using { get; set; }
        [DataMember(Name = "label")]
        public string Label { get; set; }
        [DataMember(Name = "user0_name")]
        public string User0Name { get; set; }
        [DataMember(Name = "user1_name")]
        public string User1Name { get; set; }
        public override string ToString()
        {
            // combobox label
            return $"{Label} - {UsingLabel(Using)}";
        }
    }
}

using YukoBot.Enums;

namespace YukoBot.Models.Web.Requests
{
    public class ExecuteScriptRequest : Request<ExecuteScriptRequest>
    {
        public ulong ChannelId { get; set; }
        public ScriptMode Mode { get; set; }
        public ulong MessageId { get; set; }
        public int Count { get; set; }
        public bool HasNext { get; set; }
    }
}

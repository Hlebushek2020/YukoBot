namespace YukoClient.Models.Web.Requests
{
    public class ExecuteScriptRequest : Request
    {
        public ulong ChannelId { get; set; }
        public Enums.ScriptMode Mode { get; set; }
        public ulong MessageId { get; set; }
        public int Count { get; set; }
        public bool HasNext { get; set; }
    }
}

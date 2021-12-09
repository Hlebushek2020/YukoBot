namespace YukoClient.Models
{
    public class Script
    {
        #region Propirties
        public Channel Channel { get; set; }

        public ScriptMode Mode { get; set; }

        public ulong MessageId { get; set; }

        public int Count { get; set; }
        #endregion
    }
}

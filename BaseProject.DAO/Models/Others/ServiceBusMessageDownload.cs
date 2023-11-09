namespace BaseProject.DAO.Models.Others
{
    public class ServiceBusMessageDownload<T>
    {
        public int IdDownload { get; set; }
        public T Payload { get; set; }

        public ServiceBusMessageDownload() { }

        public ServiceBusMessageDownload(int idDownload, T payload)
        {
            IdDownload = idDownload;
            Payload = payload;
        }
    }
}

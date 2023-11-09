namespace BaseProject.DAO.Models.Others
{
    public class ServiceBusMessageUpload<T>
    {
        public int IdUpload { get; set; }
        public T Payload { get; set; }

        public ServiceBusMessageUpload() { }

        public ServiceBusMessageUpload(int idUpload, T payload)
        {
            IdUpload = idUpload;
            Payload = payload;
        }
    }
}

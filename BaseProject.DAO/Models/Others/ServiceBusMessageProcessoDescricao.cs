namespace BaseProject.DAO.Models.Others
{
    public class ServiceBusMessageProcessoDescricao<T>
    {
        public int IdProcessoDescricao { get; set; }
        public T Payload { get; set; }

        public ServiceBusMessageProcessoDescricao() { }

        public ServiceBusMessageProcessoDescricao(int idProcessoDescricao, T payload)
        {
            IdProcessoDescricao = idProcessoDescricao;
            Payload = payload;
        }
    }

}

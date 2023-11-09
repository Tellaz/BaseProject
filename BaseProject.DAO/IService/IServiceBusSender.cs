
namespace BaseProject.DAO.IService
{
	public interface IServiceBusSender
    {
        Task<bool> SendMessage<T>(byte queue, T message);
    }
}
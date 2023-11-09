using BaseProject.DAO.Models.API.OpenAI;
using BaseProject.DAO.Models;

namespace BaseProject.DAO.IService
{
	public interface IServiceOpenAI
    {
        Task<LogOpenAI> Chat(ChatRequest chatRequest);
    }
}
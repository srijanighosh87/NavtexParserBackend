

namespace NavtexPositionParser.Base
{
    public interface IBaseManager<Tin, Tout> where Tin : ICommand where Tout : IDto
    {
        Task<Tout> ProcessAsync(Tin input);
    }

}

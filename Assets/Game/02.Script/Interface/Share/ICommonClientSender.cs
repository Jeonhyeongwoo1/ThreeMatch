using Cysharp.Threading.Tasks;
using ThreeMatch.Shared;

namespace ThreeMatch.Interface
{
    public interface ICommonClientSender : IClientSender
    {
        public UniTask<CommonResponse> GetConstDataRequest();
    }
}

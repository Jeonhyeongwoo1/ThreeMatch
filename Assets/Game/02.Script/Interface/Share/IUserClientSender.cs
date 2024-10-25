using Cysharp.Threading.Tasks;
using ThreeMatch.Shared;

namespace ThreeMatch.Interface
{
    public interface IUserClientSender : IClientSender
    {
        UniTask<UserResponse> SelectStageRequest();
        UniTask<UserResponse> LoadUserDataRequest(UserRequest request);
        UniTask<UserResponse> ChargedHeartRequest();
        UniTask<UserResponse> BuyHeartRequest();
    }
}
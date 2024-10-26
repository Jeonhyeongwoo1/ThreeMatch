using Cysharp.Threading.Tasks;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Shared;

namespace ThreeMatch.Interface
{
    public interface IUserClientSender : IClientSender
    {
        UniTask<UserResponse> LoadUserDataRequest(UserRequest request);
        UniTask<UserResponse> ChargedHeartRequest();
        UniTask<UserResponse> BuyHeartRequest();
        UniTask<DailyRewardHistoryResponse> GetDailyRewardRequest(int itemId);
    }
}
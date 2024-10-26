using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Shared;

namespace ThreeMatch.Interface
{
    public interface IStageClientSender : IClientSender
    {
        UniTask<StageResponse> LoadStageDataRequest();
        UniTask<StageResponse> StageClearOrFailRequest(int stageLevel, int starCount, bool isClear, List<InGameItemData> inGameItemDataList);
        UniTask<UserResponse> RemoveHeartRequest();
    }
}
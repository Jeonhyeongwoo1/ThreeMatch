using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.Firebase.Data;
using ThreeMatch.Shared;

namespace ThreeMatch.Interface
{
    public interface IStageClientSender : IClientSender
    {
        UniTask<StageResponse> LoadStageDataRequest();
        UniTask<StageResponse> StageClearRequest(int stageLevel, int starCount, List<InGameItemData> inGameItemDataList);
        UniTask<Response> StageFailedRequest(List<InGameItemData> inGameItemDataList);
    }
}
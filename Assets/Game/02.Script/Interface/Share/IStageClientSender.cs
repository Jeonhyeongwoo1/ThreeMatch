using Cysharp.Threading.Tasks;
using ThreeMatch.Shared;

namespace ThreeMatch.Interface
{
    public interface IStageClientSender : IClientSender
    {
        UniTask<StageResponse> LoadStageDataRequest();
        UniTask<StageResponse> UpdateStageLevelRequest(int stageLevel, int starCount);
    }
}
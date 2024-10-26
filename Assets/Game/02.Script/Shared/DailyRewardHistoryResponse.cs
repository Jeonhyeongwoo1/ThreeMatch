using ThreeMatch.Firebase.Data;

namespace ThreeMatch.Shared
{
    public class DailyRewardHistoryResponse : Response
    {
        public DailyRewardHistoryData dailyRewardHistoryData;
        public long userMoney;
    }
}
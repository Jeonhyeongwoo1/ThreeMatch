using System.Collections.Generic;
using ThreeMatch.Firebase.Data;

namespace ThreeMatch.Shared
{
    public class CommonResponse : Response
    {
        public FBCommonData fbCommonData;
        public AchievementCommonData achievementCommonData;
    }
}
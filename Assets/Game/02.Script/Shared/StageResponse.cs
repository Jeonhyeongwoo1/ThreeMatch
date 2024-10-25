using System.Collections.Generic;
using ThreeMatch.Firebase.Data;

namespace ThreeMatch.Shared
{
    public class StageResponse : Response
    {
        public List<StageLevelData> stageLevelDataList;
    }
}
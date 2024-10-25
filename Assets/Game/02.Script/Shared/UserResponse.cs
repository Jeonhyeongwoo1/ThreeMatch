using System.Collections.Generic;
using ThreeMatch.Firebase.Data;

namespace ThreeMatch.Shared
{
    public class UserResponse : Response
    {
        public UserData userData;
        public List<InGameItemData> inGameItemDataList;
    }
}
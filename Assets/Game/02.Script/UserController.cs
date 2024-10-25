using ThreeMatch.Core;
using ThreeMatch.OutGame.Data;

namespace ThreeMatch
{
    public class UserController
    {
        public static UserController I
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserController();
                }

                return _instance;
            }
        }
        
        private static UserController _instance;

        private UserModel _userModel;
        
        UserController()
        {
            _userModel = ModelFactory.CreateOrGet<UserModel>();
        }
    }
}
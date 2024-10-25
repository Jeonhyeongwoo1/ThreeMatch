using ThreeMatch.Firebase;

namespace ThreeMatch.Server
{
    public abstract class ServerRequestHandler
    {
        protected FirebaseController _firebaseController;

        protected ServerRequestHandler(FirebaseController firebaseController)
        {
            _firebaseController = firebaseController;
        }
    }
}
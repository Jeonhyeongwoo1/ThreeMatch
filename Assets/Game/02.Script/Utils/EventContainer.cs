using ThreeMatch.Manager;
using ThreeMatch.OutGame.Presenter;
using ThreeMatch.Shared.Presenter;

public static class EventContainer
{
    public static void SilentRegisterEvent()
    {
        EventManager.Register(nameof(HeartShopPresenter.OpenHeartShopPopup));
        EventManager.Register(nameof(TutorialPresenter.OpenPopup));
    }
}
using ThreeMatch.Common.Presenter;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Presenter;

public static class EventContainer
{
    public static void SilentRegisterEvent()
    {
        EventManager.Register(nameof(HeartShopPresenter.OpenHeartShopPopup));
        EventManager.Register(nameof(TutorialPresenter.OpenPopup));
    }
}
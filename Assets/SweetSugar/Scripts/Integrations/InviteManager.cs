// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using UnityEngine;
#if USE_GETSOCIAL_UI
using GetSocialSdk.Ui;
using GetSocialSdk.Core;
using GetSocialSdk;
using Facebook.Unity;
#endif

namespace SweetSugar.Scripts.Integrations
{
    public class InviteManager : MonoBehaviour
    {
        #if USE_GETSOCIAL_UI

    private string url;
    public Texture2D image; // works only on uncompressed, non-HDR texture formats. You must enable the texture's Read/Write Enabled flag in the Texture Import Settings.

    private void Start()
    {
        GetSocial.RegisterInviteChannelPlugin(InviteChannelIds.Facebook, new FacebookSharePlugin());
    }
    public void Invite()
    {
        if (GetSocial.IsInitialized)
        {
            var linkParams = new LinkParams();
            linkParams.Add("invite", "invite_value");
            var inviteContent = InviteContent.CreateBuilder()
            .WithMediaAttachment(MediaAttachment.Image(image))
            .Build();
            GetSocialUi.CreateInvitesView().SetCustomInviteContent(inviteContent).SetInviteCallbacks(
                    onComplete: (channelId) => { Debug.Log("Invitation was sent via " + channelId); InitScript.Instance.ShowReward(); },
                    onCancel: (channelId) => Debug.Log("Invitation via " + channelId + " was cancelled"),
                    onFailure: (channelId, error) => Debug.LogError("Invitation via" + channelId + "failed, error: " + error.Message)
                ).Show();
        }
    }

    public void Share()
    {
        StartCoroutine(ShareDelay());

    }

    IEnumerator ShareDelay()
    {
        yield return new WaitForEndOfFrame();
        Texture2D captureScreenshotAsTexture = ScreenCapture.CaptureScreenshotAsTexture();
        if (GetSocial.IsInitialized)
        {
            var inviteContent = InviteContent.CreateBuilder()
                .WithText("Try to beat me in this game [APP_INVITE_URL]") // NOTE: if you customize the text [APP_INVITE_URL] placeholder have to be used
                .WithMediaAttachment(MediaAttachment.Image(captureScreenshotAsTexture))
                .Build();
            bool wasShown = GetSocialUi.CreateInvitesView()
                .SetCustomInviteContent(inviteContent)
                .SetInviteCallbacks(
                    onComplete: (channelId) => Debug.Log("Invitation was sent via " + channelId),
                    onCancel: (channelId) => Debug.Log("Invitation via " + channelId + " was cancelled"),
                    onFailure: (channelId, error) => Debug.LogError("Invitation via" + channelId + "failed, error: " + error.Message)
                )
                .Show();
        }
    }
}

    public class FacebookSharePlugin : InviteChannelPlugin
    {
        #region IInvitePlugin implementation

        public bool IsAvailableForDevice(InviteChannel inviteChannel)
        {
            return true;
        }

        public void PresentChannelInterface(InviteChannel inviteChannel, InvitePackage invitePackage,
                                             Action onComplete, Action onCancel, Action<GetSocialError> onFailure)
        {
            GetSocialDebugLogger.D(string.Format("FacebookSharePlugin.PresentChannelInterface(), inviteChannel: {0}, invite package: {1}",
                    inviteChannel, invitePackage));

            // GetSocialUi needs to be closed while Facebook activity is opened
            // because othewise it cannot deliever the result back to the app
            GetSocialUi.CloseView(true);
            SendInvite(invitePackage.ReferralDataUrl, onComplete, onCancel, onFailure);
        }

        #endregion

        static void SendInvite(string referralDataUrl,
                               Action completeCallback,
                               Action cancelCallback,
                               Action<GetSocialError> errorCallback)
        {
            GetSocialDebugLogger.D("Sharing link on Facebook : " + referralDataUrl);
            FB.Mobile.ShareDialogMode = ShareDialogMode.WEB;
            FB.ShareLink(new Uri(referralDataUrl), callback: result =>
            {

            // reopen GetSocialUi
            // because othewise it cannot deliever the result back to the app
            GetSocialUi.RestoreView();
            GetSocialDebugLogger.D("Sharing link finished: " + result);
                if (result.Cancelled)
                {
                    cancelCallback();
                    return;
                }
                if (!string.IsNullOrEmpty(result.Error))
                {
                    var errorMsg = "Failed to share link: " + result.Error;
                    Debug.LogError(errorMsg);
                    errorCallback(new GetSocialError(errorMsg));
                    return;
                }

                completeCallback();
            });
        }
        #endif
    }
}


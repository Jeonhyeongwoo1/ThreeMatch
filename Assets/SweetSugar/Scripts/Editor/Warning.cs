﻿// // ©2015 - 2024 Candy Smith
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

using System.IO;
using UnityEditor;

namespace SweetSugar.Scripts.Editor
{
    [InitializeOnLoad]
    public class Warning
    {
        static Warning()
        {
            EditorApplication.update += RunOnce;
        }

        static void RunOnce()
        {
            //        Debug.Log(EditorSceneManager.GetSceneManagerSetup()[0].path);
            //        if (EditorSceneManager.GetSceneManagerSetup()[0].path != "Assets/WoodlandBubble/Scenes/game.unity")
            //            EditorSceneManager.OpenScene("Assets/WoodlandBubble/Scenes/game.unity");
            if (Directory.Exists("Assets/PlayServicesResolver"))
            {
                //if (!imported)
                //{

                //    AssetDatabase.ImportAsset("Assets/PlayServicesResolver");
                //#if GOOGLE_MOBILE_ADS
                //            GooglePlayServices.PlayServicesResolver.MenuResolve();
                //            EditorPrefs.SetBool("notfirsttime", true);
                //#endif

                //            Debug.Log("assets reimorted");
                //            //}
            }

            EditorApplication.update -= RunOnce;
        }
    }
}
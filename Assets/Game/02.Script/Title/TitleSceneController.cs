using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.Title.Controller
{
    public class TitleSceneController : MonoBehaviour
    {

        public void MoveToMemu()
        {
            StartCoroutine(Wait(2, () => SceneManager.LoadScene("MenuScene")));
        }

        private IEnumerator Wait(float wait, Action action)
        {
            yield return new WaitForSeconds(wait);
            action?.Invoke();
        }
    }
}
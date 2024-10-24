  using System;
  using System.Collections.Generic;
  using ThreeMatch.InGame.Interface;
  using UniRx;
  using UnityEngine;

  namespace ThreeMatch.InGame.Model
  {
      public class InGameMenuButtonModel : IModel
      {
          [Serializable]
          public struct MenuButtonData
          {
              public InGameMenuPopupButtonType inGameMenuPopupButtonType;
              public Action callback;
          }

          private ReactiveProperty<List<MenuButtonData>> _menuButtonDataList = new();

          public InGameMenuButtonModel()
          {
              Initialize();
          }

          private void Initialize()
          {
              _menuButtonDataList.Value ??= new List<MenuButtonData>();
              int length = Enum.GetValues(typeof(InGameMenuPopupButtonType)).Length;
              for (int i = 0; i < length; i++)
              {
                  MenuButtonData data = new MenuButtonData();
                  data.inGameMenuPopupButtonType = (InGameMenuPopupButtonType)i;
                  // data.callback = null;
                  _menuButtonDataList.Value.Add(data);
              }
          }

          public Action GetCallback(InGameMenuPopupButtonType inGameMenuPopupButtonType)
          {
              if (!_menuButtonDataList.HasValue)
              {
                  Debug.LogError("menu button data list is null");
                  return null;
              }

              var data = _menuButtonDataList.Value;
              if (data == null)
              {
                  Debug.LogError("menu button data list is null");
                  return null;
              }

              return data.Find(v => v.inGameMenuPopupButtonType == inGameMenuPopupButtonType).callback;
          }

          public void AddCallback(InGameMenuPopupButtonType inGameMenuPopupButtonType, Action callback)
          {
              if (!_menuButtonDataList.HasValue)
              {
                  _menuButtonDataList = new ReactiveProperty<List<MenuButtonData>>();
                  MenuButtonData buttonData = new MenuButtonData();
                  buttonData.inGameMenuPopupButtonType = inGameMenuPopupButtonType;
                  buttonData.callback = callback;
                  _menuButtonDataList.Value.Add(buttonData);
              }
              else
              {
                  if (_menuButtonDataList.Value == null)
                  {
                      _menuButtonDataList.Value = new List<MenuButtonData>();
                      Initialize();
                  }
                  // _menuButtonDataList.Value ??= new List<MenuButtonData>();
                  for (var i = 0; i < _menuButtonDataList.Value.Count; i++)
                  {
                      MenuButtonData buttonData = _menuButtonDataList.Value[i];
                      if (buttonData.inGameMenuPopupButtonType == inGameMenuPopupButtonType)
                      {
                          buttonData.callback = callback;
                          _menuButtonDataList.Value[i] = buttonData;
                      }
                  }
              }
          }
      }
  }
using System.Collections.Generic;
using System.Linq;
using ThreeMatch.Core;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using UniRx;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Mission
    {
        private MissionModel _missionModel;
        private MissionView _missionView;
        
        public Mission(List<MissionInfoData> missionDataList)
        {
             _missionModel = ModelFactory.CreateOrGet<MissionModel>();
             var missionInfoDataList = new List<MissionInfoData>();
             foreach (MissionInfoData missionInfoData in missionDataList)
             {
                 MissionInfoData data = new MissionInfoData();
                 data.removeCount = missionInfoData.removeCount;
                 data.missionType = missionInfoData.missionType;
                 missionInfoDataList.Add(data);
             }

             _missionModel.missionDataList = new ReactiveProperty<List<MissionInfoData>>(missionInfoDataList);
             _missionView = UIManager.Instance.GetView<MissionView>();

             var missionViewDataList = missionDataList.Select(missionData => new MissionView.Data
                 { missionType = missionData.missionType, count = missionData.removeCount }).ToList();
             _missionView.Initialize(missionViewDataList);
        }

        public (bool, Vector3) TryClearMission(MissionType missionType, int removeCount)
        {
            if (!IsContainMission(missionType))
            {
                return (false, Vector3.zero);
            }

            MissionInfoData missionInfoData = _missionModel.missionDataList.Value.Find(v => v.missionType == missionType);
            if (IsAlreadyClearMission(missionInfoData))
            {
                return (true, Vector3.zero);
            }

            missionInfoData.removeCount -= removeCount;
            Vector3 missionElementPosition =
                _missionView.GetMissionElementPositionAndUpdateUI(missionInfoData.missionType,
                    missionInfoData.removeCount.ToString());
            return (true, missionElementPosition);
        }

        public bool IsAllSuccessMission()
        {
            foreach (MissionInfoData data in _missionModel.missionDataList.Value)
            {
                if (data.removeCount > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsAlreadyClearMission(MissionInfoData missionInfoData)
        {
            return missionInfoData.removeCount == 0;
        }

        public bool IsContainMission(MissionType missionType)
        {
            if (_missionModel.missionDataList.Value == null)
            {
                return false;
            }

            return _missionModel.missionDataList.Value.Exists(v => v.missionType == missionType);
        }
    }
}
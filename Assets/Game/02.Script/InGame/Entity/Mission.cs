using System.Collections.Generic;
using System.Linq;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;

namespace ThreeMatch.InGame.Entity
{
    public class Mission
    {
        private MissionModel _missionModel;
        private MissionView _missionView;
        
        public Mission(List<MissionData> missionDataList)
        {
             _missionModel = ModelFactory.CreateOrGet<MissionModel>();
             _missionModel.missionDataList.Value = missionDataList;
             _missionView = UIManager.Instance.CreateOrGet<MissionView>();

             var missionViewDataList = missionDataList.Select(missionData => new MissionView.Data
                 { missionType = missionData.missionType, count = missionData.removeCount }).ToList();
             _missionView.Initialize(missionViewDataList);
        }

        public bool TryClearMission(MissionType missionType, int removeCount)
        {
            if (!IsContainMission(missionType))
            {
                return false;
            }

            MissionData missionData = _missionModel.missionDataList.Value.Find(v => v.missionType == missionType);
            if (IsClearMission(missionData))
            {
                return true;
            }

            missionData.removeCount -= removeCount;
            _missionView.UpdateUI(missionData.missionType, missionData.removeCount.ToString());
            return true;
        }

        public bool IsAllSuccessMission()
        {
            foreach (MissionData data in _missionModel.missionDataList.Value)
            {
                if (data.removeCount > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsClearMission(MissionData missionData)
        {
            return missionData.removeCount == 0;
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
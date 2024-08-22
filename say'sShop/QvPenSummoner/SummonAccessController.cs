using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace say_sShop.QvPenSummoner
{
    public class SummonAccessController : UdonSharpBehaviour
    {
        [Header("【特定の人だけにペンを呼び出すシステムを使わせたい場合はAccessLimitedにチェックを入れる(イベントキャスト用)】")]
        public bool AccessLimited;
        public string[] AllowedUserNames;
        public GameObject[] ExclusiveObjects;

        public bool IsUserAllowed()
        {
            if (!AccessLimited)
            {
                return true;
            }
            
            string currentUserName = Networking.LocalPlayer.displayName;
            if (AllowedUserNames != null)
            {
                for (int i = 0; i < AllowedUserNames.Length; i++)
                {
                    if (currentUserName.Equals(AllowedUserNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // ユーザーに割り当てられた専用のペンを取得
        public GameObject GetExclusiveObjectForUser(string userName)
        {
            if (AllowedUserNames != null && ExclusiveObjects != null)
            {
                for (int i = 0; i < AllowedUserNames.Length; i++)
                {
                    if (userName.Equals(AllowedUserNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return i < ExclusiveObjects.Length ? ExclusiveObjects[i] : null;
                    }
                }
            }
            return null;
        }
    }
}
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace say_sShop.QvPenSummoner
{
    public class PenArrayManager : UdonSharpBehaviour
    {
        [Header("【呼び出したいQVPenの親オブジェクトをRootGameObjectに入れる】")]
        [Tooltip("呼び出したいペンの親のオブジェクトを入れる")]
        public GameObject RootGameObject;
        [Tooltip("呼び出す必要のないペンがある場合は右クリック > DeleteArrayElementで削除する")]
        public GameObject[] PenArray;

        const int MaxPens = 100;
        int _currentPenCount;
        int _validPenCount;

        void OnValidate()
        {
            InitializePenArray();
        }

        void InitializePenArray()
        {
            PenArray = new GameObject[MaxPens];
            _currentPenCount = 0;

            if (RootGameObject)
            {
                RecursivelyFindPen(RootGameObject.transform);
            }

            _validPenCount = _currentPenCount;

            GameObject[] newPenArray = new GameObject[_validPenCount];
            for (int i = 0; i < _validPenCount; i++)
            {
                newPenArray[i] = PenArray[i];
            }

            PenArray = newPenArray;
        }

        void RecursivelyFindPen(Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                if (child.name == "Pen" && _currentPenCount < MaxPens)
                {
                    PenArray[_currentPenCount] = child.gameObject;
                    _currentPenCount++;
                }
                
                RecursivelyFindPen(child);
            }
        }
    }
}
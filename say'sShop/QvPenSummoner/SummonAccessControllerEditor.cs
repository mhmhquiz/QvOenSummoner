#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using say_sShop.QvPenSummoner;

[CustomEditor(typeof(SummonAccessController))]
public class SummonAccessControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "AllowedUserNames", "ExclusiveObjects");

        // AllowedUserNames配列のサイズ変更UIとカスタムラベル表示
        SerializedProperty allowedUserNames = serializedObject.FindProperty("AllowedUserNames");
        ShowArrayWithCustomLabel(allowedUserNames, "【呼び出し機能を使わせたいユーザーのVRChat名を入力】", "ユーザー");

        // ExclusiveObjects配列のカスタムラベル表示
        SerializedProperty exclusiveObjects = serializedObject.FindProperty("ExclusiveObjects");
        ShowExclusiveObjectsArray(exclusiveObjects, allowedUserNames, "【呼び出される専用のペンをアタッチ】");

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowArrayWithCustomLabel(SerializedProperty arrayProperty, string label, string elementPrefix)
    {
        EditorGUILayout.LabelField(label);
        EditorGUI.indentLevel++;

        // 配列のサイズ調整UI
        int newSize = EditorGUILayout.IntField("Size", arrayProperty.arraySize);
        if (newSize != arrayProperty.arraySize)
        {
            arrayProperty.arraySize = newSize;
        }

        // 配列のカスタムラベル表示
        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            SerializedProperty elementProperty = arrayProperty.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(elementProperty, new GUIContent($"{elementPrefix} {i + 1}"));
        }

        EditorGUI.indentLevel--;
    }

    private void ShowExclusiveObjectsArray(SerializedProperty exclusiveObjects, SerializedProperty allowedUserNames, string label)
    {
        EditorGUILayout.LabelField(label);
        EditorGUI.indentLevel++;

        // exclusiveObjects配列のサイズをallowedUserNames配列に合わせる
        if (allowedUserNames.arraySize > 0 && exclusiveObjects.arraySize < allowedUserNames.arraySize)
        {
            exclusiveObjects.arraySize = allowedUserNames.arraySize;
        }

        // ExclusiveObjects配列の要素にユーザー名をラベルとして表示
        int arraySize = Mathf.Min(exclusiveObjects.arraySize, allowedUserNames.arraySize);
        for (int i = 0; i < arraySize; i++)
        {
            SerializedProperty elementProperty = exclusiveObjects.GetArrayElementAtIndex(i);
            string userName = (i < allowedUserNames.arraySize) ? allowedUserNames.GetArrayElementAtIndex(i).stringValue : $"ユーザー {i + 1}";
            EditorGUILayout.PropertyField(elementProperty, new GUIContent($"{userName} 専用ペン"));
        }

        EditorGUI.indentLevel--;
    }
}
#endif

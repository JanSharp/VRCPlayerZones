using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    public static class PlayerZonesManagerOnBuild
    {
        public const int TotalUnityLayerCount = 32;
        private const string PlayerZoneLayerName = "PlayerZone";
        private static int playerZoneLayerIndex;

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterType<PlayerZonesManager>(OnBuild, order: -20);
        }

        private static bool OnBuild(PlayerZonesManager manager)
        {
            if (!GetOrDeterminePlayerZoneLayerIndex(manager))
                return false;
            SetZoneIndexes(manager);
            return true;
        }

        private static void SetZoneIndexes(PlayerZonesManager manager)
        {
            if (manager.allZones == null)
                return;
            for (int i = 0; i < manager.allZones.Length; i++)
            {
                SerializedObject so = new(manager.allZones[i]);
                so.FindProperty(nameof(PlayerZone.indexInAllZones)).intValue = i;
                so.ApplyModifiedProperties();
            }
        }

        private static bool GetOrDeterminePlayerZoneLayerIndex(PlayerZonesManager manager)
        {
            if (!manager.AutoZonesLayerIndex)
                playerZoneLayerIndex = manager.ZonesLayerIndex;
            else
            {
                SerializedObject so = new(manager);
                SerializedProperty zonesLayerIndexProp = so.FindProperty(PlayerZonesManager.ZonesLayerIndexPropName);
                if (!TryDetermineAndApplyLayer(manager, zonesLayerIndexProp))
                    return false;
                playerZoneLayerIndex = zonesLayerIndexProp.intValue;
                so.ApplyModifiedProperties();
            }
            return true;
        }

        public static bool TryDetermineAndApplyLayer(PlayerZonesManager manager, SerializedProperty zonesLayerIndexProp)
        {
            if (!TryDetermineAndSetupLayer(manager, out playerZoneLayerIndex))
            {
                Debug.LogError("[PlayerZones] No layer available for the Player Zones system to use automatically."
                    + $"Configure it manually on the {nameof(PlayerZonesManager)}", manager);
                return false;
            }
            zonesLayerIndexProp.intValue = playerZoneLayerIndex;
            return true;
        }

        public static bool TryDetermineAndSetupLayer(PlayerZonesManager manager, out int layerIndex)
        {
            layerIndex = System.Math.Clamp(manager.ZonesLayerIndex, 0, TotalUnityLayerCount - 1);
            if (LayerMask.LayerToName(layerIndex) == PlayerZoneLayerName || !LayerCollidesWithAnything(layerIndex))
                return true;

            layerIndex = LayerMask.NameToLayer(PlayerZoneLayerName);
            if (layerIndex != -1)
                return true;

            foreach (int existingLayerIndex in EnumerateNamedLayers())
            {
                if (!LayerCollidesWithAnything(existingLayerIndex))
                {
                    layerIndex = existingLayerIndex;
                    return true;
                }
            }

            if (TryGetFirstUnnamedLayer(out layerIndex))
            {
                EditorUtil.SetLayerName(layerIndex, PlayerZoneLayerName);
                // Intentionally and very specifically only run MakeLayerNotCollideWithAnything
                // when setting up the layer for the first time when auto layer management is enabled.
                // If a user chooses to change the collision settings for this layer afterwards,
                // that is a decision they must be able to make.
                MakeLayerNotCollideWithAnything(layerIndex);
                return true;
            }

            return false;
        }

        private static bool TryGetFirstUnnamedLayer(out int layerIndex)
        {
            for (layerIndex = 0; layerIndex < TotalUnityLayerCount; layerIndex++)
                if (LayerMask.LayerToName(layerIndex) == "")
                    return true;
            layerIndex = -1;
            return false;
        }

        private static IEnumerable<int> EnumerateNamedLayers()
        {
            for (int layerIndex = 0; layerIndex < TotalUnityLayerCount; layerIndex++)
                if (LayerMask.LayerToName(layerIndex) != "")
                    yield return layerIndex;
        }

        public static bool LayerCollidesWithAnything(int layerIndex)
        {
            foreach (int otherLayer in EnumerateNamedLayers())
                if (!Physics.GetIgnoreLayerCollision(layerIndex, otherLayer))
                    return true;
            return false;
        }

        public static void MakeLayerNotCollideWithAnything(int layerIndex)
        {
            for (int otherLayer = 0; otherLayer < TotalUnityLayerCount; otherLayer++)
                Physics.IgnoreLayerCollision(layerIndex, otherLayer, true); // Modifies project settings.
        }

        public static void SetPlayerZoneLayerAndApply(GameObject go) => SetPlayerZoneLayerAndApply(new SerializedObject(go));
        public static void SetPlayerZoneLayerAndApply(GameObject[] gos)
        {
            if (gos.Length != 0)
                SetPlayerZoneLayerAndApply(new SerializedObject(gos));
        }
        private static void SetPlayerZoneLayerAndApply(SerializedObject gameObjectSo)
        {
            gameObjectSo.FindProperty("m_Layer").intValue = playerZoneLayerIndex;
            gameObjectSo.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(PlayerZonesManager))]
    public class PlayerZonesManagerEditor : Editor
    {
        private SerializedProperty autoZonesLayerProp;
        private SerializedProperty zonesLayerIndexProp;

        private void OnEnable()
        {
            autoZonesLayerProp = serializedObject.FindProperty(PlayerZonesManager.AutoZonesLayerPropName);
            zonesLayerIndexProp = serializedObject.FindProperty(PlayerZonesManager.ZonesLayerIndexPropName);
            EnsureLayerIsWithinBounds();
        }

        private void EnsureLayerIsWithinBounds()
        {
            int layerIndex = zonesLayerIndexProp.intValue;
            int clamped = System.Math.Clamp(layerIndex, 0, PlayerZonesManagerOnBuild.TotalUnityLayerCount - 1);
            if (layerIndex == clamped)
                return;
            zonesLayerIndexProp.intValue = clamped;
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            serializedObject.Update();
            EditorGUILayout.PropertyField(autoZonesLayerProp);

            using (new EditorGUI.DisabledGroupScope(disabled: !autoZonesLayerProp.boolValue))
                if (GUILayout.Button(new GUIContent("Run Auto Layer Logic", "This is also the logic which runs OnBuild")))
                    PlayerZonesManagerOnBuild.TryDetermineAndApplyLayer((PlayerZonesManager)target, zonesLayerIndexProp);

            using (new EditorGUI.DisabledGroupScope(disabled: autoZonesLayerProp.boolValue))
            {
                EditorGUI.BeginChangeCheck();
                EditorUtil.DrawLayerField(zonesLayerIndexProp, new GUIContent("Zones Layer"));
                if (EditorGUI.EndChangeCheck())
                    EnsureLayerIsWithinBounds();

                using (new EditorGUI.DisabledGroupScope(disabled: !PlayerZonesManagerOnBuild.LayerCollidesWithAnything(zonesLayerIndexProp.intValue)))
                {
                    if (GUILayout.Button(new GUIContent(
                        "Make Layer Not Collide With Anything",
                        "Be very careful, only click this button for custom layers")))
                    {
                        PlayerZonesManagerOnBuild.MakeLayerNotCollideWithAnything(zonesLayerIndexProp.intValue);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

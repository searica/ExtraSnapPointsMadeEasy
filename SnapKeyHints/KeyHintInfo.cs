
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.UI;

namespace ExtraSnapsMadeEasy.SnapKeyHints;
internal sealed class KeyHintInfo
{
    public enum KeyHintVisibility
    {
        DoNotChange,
        Show,
        Hide
    }

    internal sealed class HorizontalLayoutSettings
    {
        public float minHeight = -1;
        public float preferredHeight = -1;
        public float flexibleHeight = -1;

        public float minWidth = -1;
        public float preferredWidth = -1;
        public float flexibleWidth = -1;
    }

    internal sealed class KeyHintConfig
    {
        internal string TransformPath;

        /// <summary>
        ///     ConfigEntry that this key binding bound to.
        /// </summary>
        internal ConfigEntry<KeyCode> KeyConfig;

        /// <summary>
        ///     Raw string to use instead of KeyConfig if KeyConfig is null.
        /// </summary>
        internal string RawKeyText;

        /// <summary>
        ///     Whether the element should be made visible or hidden when updating the config.
        /// </summary>
        internal KeyHintVisibility Visibility = KeyHintVisibility.DoNotChange;

        public KeyHintConfig() { }

        public KeyHintConfig(
            string transformPath,
            ConfigEntry<KeyCode> keyConfig,
            string rawKeyText = "",
            KeyHintVisibility visibility = KeyHintVisibility.DoNotChange
        )
        {
            TransformPath = transformPath;
            KeyConfig = keyConfig;
            RawKeyText = rawKeyText;
            Visibility = visibility;
        }

        public bool ShouldChangeVisibility => Visibility != KeyHintVisibility.DoNotChange;
        public bool ShouldHide => Visibility == KeyHintVisibility.Hide;
        public bool ShouldShow => Visibility == KeyHintVisibility.Show;

        /// <summary>
        ///     Get Key hint text from either KeyConfig or RawKeyText as a fall back.
        /// </summary>
        public string KeyText => KeyConfig != null ? KeyConfig.Value.ToString() : RawKeyText;

    }

    /// <summary>
    ///     The game object that this attached to in the UI
    /// </summary>
    public GameObject BoundObject;
    public string TransformPath;
    public HorizontalLayoutSettings HoriLayoutSettings = new();
    public Dictionary<string, string> HintText;
    public int PreferredTextWidth = -1;
    public List<KeyHintConfig> KeyHintConfigs;

    public bool IsValid()
    {
        return BoundObject != null;
    }

    public void SetParentObject(KeyHints keyHints)
    {
        Transform parentTransform = keyHints.transform.Find(TransformPath);
        if (parentTransform == null)
        {
            Log.LogWarning($"{TransformPath} is no longer valid, could not find Snap key hint");
            return;
        }
        BoundObject = parentTransform.gameObject;
    }

    /// <summary>
    ///     Add UpdateKeyHintUI() to SettingChanged events for all bound key configs.
    /// </summary>
    public void SubscribeToKeyConfigUpdates(bool performInitialUpdate)
    {
        if (performInitialUpdate)
        {
            UpdateKeyHintUI();
        }

        foreach (KeyHintConfig keyHintConfig in KeyHintConfigs)
        {
            keyHintConfig.KeyConfig.SettingChanged += (obj, attr) =>
            {
                UpdateKeyHintUI();
            };
        }
    }

    /// <summary>
    ///     Update Key hint Text and keys in UI
    /// </summary>
    public void UpdateKeyHintUI()
    {
        if (!IsValid())
        {
            return;
        }

        foreach (KeyValuePair<string, string> pair in HintText)
        {
            Transform child = BoundObject.transform.Find(pair.Key);
            if (child == null || !child.TryGetComponent(out TextMeshProUGUI tmpText))
            {
                continue;
            }
            tmpText.text = pair.Value;

            if (PreferredTextWidth > 0 && child.TryGetComponent(out LayoutElement layout))
            {
                layout.preferredWidth = PreferredTextWidth;
            }
        }

        foreach (KeyHintConfig keyHintConfig in KeyHintConfigs)
        {
            Transform child = BoundObject.transform.Find(keyHintConfig.TransformPath);
            if (child == null || !child.TryGetComponent(out TextMeshProUGUI tmpText))
            {
                continue;
            }

            // update key hint text
            tmpText.text = keyHintConfig.KeyText;

            if (keyHintConfig.ShouldChangeVisibility)  // update visibilty of key and it's background
            {
                if (!TryGetKeyHintTransformPathParent(keyHintConfig, out Transform keyHintParent))
                {
                    continue;
                }
                keyHintParent.gameObject.SetActive(keyHintConfig.ShouldShow);
            }
        }
    }

    /// <summary>
    ///     Set entire key hint (text and keys) visibility.
    /// </summary>
    public void SetVisibility(bool visible)
    {
        if (!IsValid()) { return; }
        BoundObject.SetActive(visible);
    }

    /// <summary>
    ///     Get the top level parent of the transform path for the key hint
    /// </summary>
    /// <param name="keyHintConfig"></param>
    /// <param name="keyHintParent"></param>
    /// <returns></returns>
    private bool TryGetKeyHintTransformPathParent(KeyHintConfig keyHintConfig, out Transform keyHintParent)
    {
        string parentName = keyHintConfig.TransformPath.Split('/')[0];
        keyHintParent = BoundObject.transform.Find(parentName);
        return keyHintParent != null;
    }

    /// <summary>
    ///     Creates nested key objects for KeyHintConfigs if needed.
    ///     This will throw exceptions if all keys do not have 
    ///     the same number of nested children or if any keys have branching children.
    ///     Only intended for my current use case with altering the AltPlace key hint.
    /// </summary>
    internal void CreateKeysIfNeeded()
    {
        if (!IsValid()) { return; }

        // find a template key to copy
        Transform templateKey = null;
        string templateKeyTransformPath = null;
        foreach (var keyHintConfig in KeyHintConfigs)
        {
            Transform childKey = BoundObject.transform.Find(keyHintConfig.TransformPath);
            if (childKey && TryGetKeyHintTransformPathParent(keyHintConfig, out templateKey))
            {
                templateKeyTransformPath = keyHintConfig.TransformPath;
                break;
            }
        }
        if (templateKey is null) {
            Log.LogError($"Could not create a template key for {BoundObject.name}");
        }

        // update keys as needed
        foreach (KeyHintConfig keyHintConfig in KeyHintConfigs)
        {
            if (keyHintConfig.TransformPath == templateKeyTransformPath)
            {
                continue; // is template key
            }

            Transform childKey = BoundObject.transform.Find(keyHintConfig.TransformPath);
            if (childKey)
            {
                continue; // already exists
            }

            // copy template 
            GameObject newKey = UnityEngine.Object.Instantiate(templateKey.gameObject, templateKey.parent);

            // rename based on TransformPath
            Transform childTransform = newKey.transform;
            string[] transformNames = keyHintConfig.TransformPath.Split('/');
            for (int i = 0; i < transformNames.Length; i++)
            {
                int childCount = childTransform.childCount;

                if (childCount > 1)
                {
                    Log.LogError($"TransformPath {templateKeyTransformPath} has branching children.");
                    throw new ArgumentException($"TransformPath {templateKeyTransformPath} has branching children.");
                }

                childTransform.name = transformNames[i];

                if (childTransform.childCount == 1)
                {
                    childTransform = childTransform.GetChild(0);
                }
                else if (i != transformNames.Length - 1)
                {
                    Log.LogError($"TransformPath {templateKeyTransformPath} is missing children.");
                    throw new ArgumentException($"TransformPath {templateKeyTransformPath} is missing children.");
                }   
            }
        }
    }
}

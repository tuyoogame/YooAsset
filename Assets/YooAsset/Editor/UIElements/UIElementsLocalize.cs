#if UNITY_2019_4_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	public static class UIElementsLocalize
    {
        public static void Localize(TextField element)
		{
            ELanguageKey key = Localization.Convert(element.label);
            element.label = Localization.Language(key);
        }

        public static void Localize(Button element)
        {
            ELanguageKey key = Localization.Convert(element.text);
            element.text = Localization.Language(key);
        }

        public static void Localize(EnumField element)
        {
            ELanguageKey key = Localization.Convert(element.label);
            element.label = Localization.Language(key);
        }

        public static void Localize(PopupField<Enum> element)
        {
            ELanguageKey key = Localization.Convert(element.label);
            element.label = Localization.Language(key);
        }

        public static void Localize(PopupField<Type> element)
        {
            ELanguageKey key = Localization.Convert(element.label);
            element.label = Localization.Language(key);
        }
    }
}
#endif
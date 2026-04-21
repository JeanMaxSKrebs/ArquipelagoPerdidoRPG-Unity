using System;
using ArquipelagoPerdidoRPG.Core;
using UnityEngine;

namespace ArquipelagoPerdidoRPG.Settings
{
    public enum SupportedLanguage
    {
        PtBr = 0,
        En = 1
    }

    public class LanguageManager : SingletonBehaviour<LanguageManager>
    {
        private const string LanguageKey = "settings.language.code";

        public SupportedLanguage CurrentLanguage { get; private set; } = SupportedLanguage.PtBr;

        public event Action<SupportedLanguage> OnLanguageChanged;

        protected override void Awake()
        {
            base.Awake();
            LoadLanguage();
        }

        public void SetLanguage(SupportedLanguage language)
        {
            if (CurrentLanguage == language)
            {
                return;
            }

            CurrentLanguage = language;
            PlayerPrefs.SetInt(LanguageKey, (int)CurrentLanguage);
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }

        public string GetLanguageCode()
        {
            return CurrentLanguage == SupportedLanguage.PtBr ? "PT-BR" : "EN";
        }

        private void LoadLanguage()
        {
            int raw = PlayerPrefs.GetInt(LanguageKey, (int)SupportedLanguage.PtBr);
            if (Enum.IsDefined(typeof(SupportedLanguage), raw))
            {
                CurrentLanguage = (SupportedLanguage)raw;
            }
            else
            {
                CurrentLanguage = SupportedLanguage.PtBr;
            }

            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }
}

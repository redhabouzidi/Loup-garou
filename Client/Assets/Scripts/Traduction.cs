using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization;

	/// <summary>
	/// Asset usage example.
	/// </summary>
	public class Traduction : MonoBehaviour
	{
        public Button FrenchB,EnglishB;
		/// <summary>
		/// Called on app start.
		/// </summary>
		void Start()
		{
			LocalizationManager.Read();
            FrenchB.onClick.AddListener(()=>SetLocalizationFr());
            EnglishB.onClick.AddListener(()=>SetLocalizationEn());
		}
        void SetLocalizationFr()
		{
			LocalizationManager.Language = "French";
		}
        void SetLocalizationEn(){
            LocalizationManager.Language = "English";
        }
	}
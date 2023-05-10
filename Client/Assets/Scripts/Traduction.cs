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
		private Image FrenchI, EnglishI;
		/// <summary>
		/// Called on app start.
		/// </summary>
		void Start()
		{
			FrenchI = FrenchB.gameObject.GetComponent<Image>();
			EnglishI = EnglishB.gameObject.GetComponent<Image>();

			LocalizationManager.Read();
            FrenchB.onClick.AddListener(()=>SetLocalizationFr());
            EnglishB.onClick.AddListener(()=>SetLocalizationEn());
		}
        void SetLocalizationFr()
		{
			LocalizationManager.Language = "French";
			FrenchI.fillCenter = true;
			EnglishI.fillCenter = false;
		}
        void SetLocalizationEn(){
            LocalizationManager.Language = "English";
			FrenchI.fillCenter = false;
			EnglishI.fillCenter = true;
        }
	}
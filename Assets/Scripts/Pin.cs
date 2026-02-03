using UnityEngine;

namespace Assets.Scripts
{
	[System.Serializable]
	public class Pin : MonoBehaviour
	{
		// Хранит локальные данные которые нужны в PinPreviewController 
		public string textPreview;
		public string textDetail;
		public Sprite sprite;
		public string spriteId;

		public void DisablingAnObject() => gameObject.SetActive(false);
	}
}

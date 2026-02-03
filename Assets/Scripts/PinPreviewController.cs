using DG.Tweening;
using SFB;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class PinPreviewController : MonoBehaviour
	{
		[Header("Set Panels")]
		[SerializeField] private GameObject previewPanel; // Панель предпросмотра
		[SerializeField] private GameObject detailPanel; // Панель полного просмотра

		[Header("Set Buttons")]
		[SerializeField] private Button readMoreButton; // Кнопка "Читать дальше"
		[SerializeField] private Button closeButton; // Кнопка закрытия подробного просмотра
		[SerializeField] private Button closePreviewButton; // Кнопка закрытия предпросмотра
		[SerializeField] private Button saveTextButton; // Кнопка сохранения текста
		[SerializeField] private Button loadImageButton; // Кнопка загрузки картинки
		[SerializeField] private Button deletePinButton; // Кнопка загрузки картинки

		[Header("Set Image")]
		[SerializeField] private Image previewImage; // Изображение предпросмотра
		[SerializeField] private Image detailImage; // Изображение полного просмотра

		[Header("Set InputField")]
		[SerializeField] private TMP_InputField textPreview; // Вводимый текст для предпросмотра
		[SerializeField] private TMP_InputField textDetail; // Вводимый текст для полного просмотра

		private Pin pin;

		// Векторы для сохранения начальных скейлов
		private Vector3 previewPanelScale;
		private Vector3 detailPanelScale;

		private void Start()
		{
			previewPanelScale = previewPanel.transform.localScale;
			detailPanelScale = detailPanel.transform.localScale;

			// Изначально скрываем панели
			previewPanel.SetActive(false);
			detailPanel.SetActive(false);

			// Назначаем кнопки
			readMoreButton.onClick.AddListener(ShowDetail);
			closeButton.onClick.AddListener(HideDetail);
			closePreviewButton.onClick.AddListener(HidePreview);
			saveTextButton.onClick.AddListener(SaveChangedText);
			loadImageButton.onClick.AddListener(LoadNewImage);
			deletePinButton.onClick.AddListener(DeletePin);
		}

		// Метод для отображения предпросмотра
		public void ShowPreview(Pin pin)
		{
			this.pin = pin;

			textPreview.text = pin.textDetail.Length > 30 ? pin.textDetail.Substring(0, 30) : pin.textDetail;
			textDetail.text = pin.textDetail;

			previewImage.sprite = pin.sprite;
			detailImage.sprite = pin.sprite;

			Debug.Log(textPreview);
			Debug.Log(textDetail);

			previewPanel.SetActive(true);
			previewPanel.transform.DOScale(previewPanelScale, 0.3f).SetEase(Ease.OutBack);

		}

		// Сохраняем измененный текст
		private void SaveChangedText()
		{
			if (pin != null)
				pin.textDetail = textDetail.text;
		}

		// Загружаем новую картинку
		private void LoadNewImage()
		{
			if (pin != null)
			{
				var extensions = new[] {
			new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
		};

				var paths = StandaloneFileBrowser.OpenFilePanel("Выберите изображение", "", extensions, true);
				if (paths.Length > 0)
				{
					string path = paths[0];

					byte[] fileData = File.ReadAllBytes(path);
					Texture2D tex = new Texture2D(2, 2);

					if (tex.LoadImage(fileData))
					{
						// Показываем
						
						Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
						pin.sprite = sprite;
					}
				}
				ShowPreview(pin);
			}
		}

		private void DeletePin()
		{
			if (pin != null)
			{
				pin.DisablingAnObject();
				Debug.Log("Disable pin: " + pin.isActiveAndEnabled);
				HideDetail();
				pin = null;
			}
		}

		// Скрываем предпросмотр
		private void HidePreview()
		{
			previewPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
			{
				previewPanel.SetActive(false);
			});
		}

		// Показываем подробный просмотр
		private void ShowDetail()
		{
			// Анимация исчезновения предпросмотра
			detailPanel.transform.localScale = detailPanelScale;
			previewPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
			{
				previewPanel.SetActive(false);
				detailPanel.SetActive(true);
				detailPanel.transform.localScale = Vector3.zero;
				detailPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
			});
		}

		// Скрываем детальный просмотр
		private void HideDetail()
		{
			detailPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
			{
				detailPanel.SetActive(false);
			});
		}
	}
}
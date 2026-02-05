using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class SaveLoadPin : MonoBehaviour
	{
		[SerializeField] private GameObject prefabPin; // Нужен для загрузки pin

		[SerializeField] private Button saveButton;
		[SerializeField] private Button loadButton;

		private List<PinData> pinData = new List<PinData>(); // Нужен для сохранения данных 

		private GameObject[] taggedObjectsPin; // Нужен для хранения данных с тегом

		private string path;

		private void Start()
		{
			path = Path.Combine(Application.persistentDataPath, "pins.json");

			saveButton.onClick.AddListener(() => { var _ = SaveAsync(); });
			loadButton.onClick.AddListener(() => { var _ = LoadAsync(); });
		}

		private async Task SaveAsync()
		{
			pinData.Clear();
			taggedObjectsPin = GameObject.FindGameObjectsWithTag("Pin");

			foreach (GameObject obj in taggedObjectsPin)
			{
				var pinComponent = obj.GetComponent<Pin>();
				var spriteId = GenerateUniqueId();

				Debug.Log("Объект с тегом: " + pinComponent.name);

				PinData pin = new PinData
				{
					name = obj.name,
					positionX = obj.transform.position.x,
					positionY = obj.transform.position.y,
					positionZ = obj.transform.position.z,
					textPreview = pinComponent.textPreview,
					textDetail = pinComponent.textDetail,
					spriteId = spriteId
				};

				await SaveSpriteForPinAsync(pinComponent.sprite, spriteId);
				pinData.Add(pin);

			}

			string json = JsonUtility.ToJson(new ScenePinsWrapper { pins = pinData }, true);
			Debug.Log("JSON: " + json);
			await File.WriteAllTextAsync(path, json);
			Debug.Log("Данные сохранены в " + path);

		}

		private async Task LoadAsync()
		{
			taggedObjectsPin = GameObject.FindGameObjectsWithTag("Pin");

			// Деактивируем текущие объекты с тегом
			foreach (GameObject obj in taggedObjectsPin)
				obj.SetActive(false);

			if (!File.Exists(path))
			{
				Debug.LogWarning("Файл не найден: " + path);
				return;
			}

			string json = await File.ReadAllTextAsync(path);
			Debug.Log("Загруженный JSON: " + json);

			ScenePinsWrapper wrapper = JsonUtility.FromJson<ScenePinsWrapper>(json);
			if (wrapper == null || wrapper.pins == null)
			{
				Debug.LogWarning("Данные не найдены или некорректны");
				return;
			}

			foreach (PinData pin in wrapper.pins)
			{
				GameObject newPin = Instantiate(prefabPin, new Vector3(pin.positionX, pin.positionY, pin.positionZ), Quaternion.identity);
				newPin.name = pin.name;

				Pin pinComponent = newPin.GetComponent<Pin>();

				if (pinComponent != null)
				{
					pinComponent.textPreview = pin.textPreview;
					pinComponent.textDetail = pin.textDetail;
					pinComponent.sprite = await LoadSpriteForPinAsync(pin.spriteId);
				}

				else
					Debug.LogWarning("Объект " + pin.name + " не содержит компонента Pin");
			}

		}

		private async Task SaveSpriteForPinAsync(Sprite sprite, string pinId)
		{
			Debug.Log(pinId);

			Texture2D tex = sprite.texture;
			Texture2D readableTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);

			readableTexture.SetPixels32(tex.GetPixels32());
			readableTexture.Apply();

			byte[] bytes = readableTexture.EncodeToPNG();
			string folderPath = Application.persistentDataPath;
			string filePath = Path.Combine(folderPath, pinId + ".png");
			await File.WriteAllBytesAsync(filePath, bytes);

			Debug.Log($"Асинхронное сохранение изображения для {pinId} завершено по пути: {filePath}");
		}

		private async Task<Sprite> LoadSpriteForPinAsync(string pinId)
		{
			string filePath = Path.Combine(Application.persistentDataPath, pinId + ".png");

			if (!File.Exists(filePath))
			{
				Debug.LogWarning("Файл не найден: " + filePath);
				return null;
			}

			byte[] fileData = await File.ReadAllBytesAsync(filePath);
			Texture2D tex = new Texture2D(2, 2);

			if (tex.LoadImage(fileData))
			{
				Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
				return sprite;
			}
			else
			{
				Debug.LogError("Ошибка при создании Texture из файла");
				return null;
			}
		}

		[System.Serializable]
		public class ScenePinsWrapper
		{
			public List<PinData> pins;
		}

		[System.Serializable]
		public class PinData
		{
			public string name;
			public float positionX;
			public float positionY;
			public float positionZ;
			public string textPreview;
			public string textDetail;
			public string spriteId;
		}

		// Генерация уникального ID для каждого пина
		private string GenerateUniqueId() => Guid.NewGuid().ToString();
	}
}

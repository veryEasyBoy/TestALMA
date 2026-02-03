using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
	public class PinController : MonoBehaviour
	{
		[SerializeField] private GameObject prefabPin;
		[SerializeField] private Camera mainCamera;
		[SerializeField] private PinPreviewController pinPreviewController;

		[SerializeField] private float longPressThreshold = 1f; // Время для перемещения пин после длительного нажатия на нее.

		private Pin selectedPin;

		private float pressStartTime;
		private bool isLongPress = false;
		private bool isDragging = false;

		private void Update()
		{
			if (EventSystem.current.IsPointerOverGameObject()) return; // Не мешать UI

			Vector2 mouseScreenPosition = Input.mousePosition;
			Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

			if (Input.GetMouseButtonDown(0))
			{
				OnMouseButtonDown(worldPosition);
			}
			else if (Input.GetMouseButton(0))
			{
				OnMouseButtonHold(worldPosition);
			}
			else if (Input.GetMouseButtonUp(0))
			{
				OnMouseButtonUp();
			}
		}

		private void OnMouseButtonDown(Vector2 worldPosition)
		{
			pressStartTime = Time.time;

			RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

			if (hit.collider != null)
			{
				// Выбираем существующий пин
				selectedPin = hit.collider.GetComponent<Pin>();
				pinPreviewController.ShowPreview(selectedPin);
			}
			else
			{
				// Создаём новый пин
				selectedPin = Instantiate(prefabPin, worldPosition, Quaternion.identity).GetComponent<Pin>();
			}
		}

		private void OnMouseButtonHold(Vector2 worldPosition)
		{
			if (selectedPin == null) return;

			float elapsedTime = Time.time - pressStartTime;

			// Перемещение пин после длительного нажатия на нее.
			if (!isLongPress && elapsedTime >= longPressThreshold)
			{
				isLongPress = true;
				isDragging = true;
			}

			// Перетаскивание
			if (isDragging)
			{
				Vector2 newPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
				selectedPin.transform.position = newPos;
			}
		}

		private void OnMouseButtonUp()
		{
			// Очистка состояния
			if (selectedPin != null)
			{
				isDragging = false;
				isLongPress = false;
				selectedPin = null;
			}
		}
	}
}
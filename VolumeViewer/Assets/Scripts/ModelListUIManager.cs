using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelListUIManager : MonoBehaviour {
    public static ModelListUIManager Instance { get; private set; }
    public Button expandButton;
    public GameObject contentGO;
    public GameObject dummyEntry;
    private bool expanded = false;
    private Coroutine currCoroutine;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    public void ToggleModelList() {
        if (currCoroutine == null) {
            expanded = !expanded;

            if (expanded) {
                currCoroutine = StartCoroutine(ShowModelListPanel());
                expandButton.transform.Find("Icon").localScale = new Vector3(-1, 1, 1);
            } else {
                currCoroutine = StartCoroutine(HideModelListPanel());
                expandButton.transform.Find("Icon").localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private IEnumerator ShowModelListPanel() {
        RectTransform rect = GetComponent<RectTransform>();
        float maxX = rect.localPosition.x + 250;

        while (rect.localPosition.x < maxX) {
            rect.localPosition += new Vector3(Time.deltaTime * 600, 0, 0);

            yield return null;
        }

        rect.localPosition = new Vector3(maxX, rect.localPosition.y, rect.localPosition.z);
        currCoroutine = null;
    }

    private IEnumerator HideModelListPanel() {
        RectTransform rect = GetComponent<RectTransform>();
        float minX = rect.localPosition.x - 250;

        while (rect.localPosition.x > minX) {
            rect.localPosition -= new Vector3(Time.deltaTime * 600, 0, 0);

            yield return null;
        }

        rect.localPosition = new Vector3(minX, rect.localPosition.y, rect.localPosition.z);
        currCoroutine = null;
    }

    public void AddEntry (int modelInstanceId, string modelName) {
        GameObject newEntry = Instantiate(dummyEntry);
        newEntry.name = "Entry";
        newEntry.transform.SetParent(dummyEntry.transform.parent, false);
        newEntry.transform.Find("EntryText").GetComponent<TextMeshProUGUI>().text = modelName;
        newEntry.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(delegate() { DeleteEntry(newEntry, modelInstanceId); });
        newEntry.SetActive(true);

        contentGO.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 30);
    }

    public void DeleteEntry(GameObject entry, int modelInstanceId) {
        ModelManager.Instance.DeleteModel(modelInstanceId);
        contentGO.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 30);

        Destroy(entry);
    }
}

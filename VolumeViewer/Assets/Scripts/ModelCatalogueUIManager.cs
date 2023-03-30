using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModelCatalogueUIManager : MonoBehaviour {
    public static ModelCatalogueUIManager Instance { get; private set; }
    public GameObject contentGO;
    public GameObject rowGO;
    public GameObject dummyEntry;
    public Sprite defaultIcon;
    private bool expanded = false;
    private Coroutine currCoroutine;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }

        SetupCatalogue();
    }

    private void SetupCatalogue() {
        GameObject currRow = rowGO;
        int itemCount = 0;

        foreach (GameObject item in ModelManager.Instance.modelPrefabs) {
            ModelInfo info = item.GetComponent<ModelInfo>();
            if (info == null) { continue; }

            if (itemCount % 4 == 0) {
                currRow = Instantiate(rowGO);
                currRow.name = "Row";
                currRow.transform.SetParent(dummyEntry.transform.parent.parent, false);
                contentGO.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 100);
                Destroy(currRow.transform.Find("DummyEntry").gameObject);
                currRow.SetActive(true);
            }

            GameObject newEntry = Instantiate(dummyEntry);
            newEntry.name = "Entry";
            newEntry.transform.SetParent(currRow.transform, false);
            if (info.icon != null) { newEntry.transform.Find("Icon").GetComponent<Image>().sprite = info.icon; }
            else { newEntry.transform.Find("Icon").GetComponent<Image>().sprite = defaultIcon; }
            newEntry.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = info.modelName;
            newEntry.GetComponent<Button>().onClick.AddListener(delegate () {
                ModelManager.Instance.SpawnModel(item);
            });
            newEntry.SetActive(true);

            itemCount++;
        }
    }

    public void ToggleModelCatalogue(bool expanded) {
        if (this.expanded != expanded && currCoroutine == null) {
            this.expanded = expanded;
            
            if (expanded) { currCoroutine = StartCoroutine(ShowModelCataloguePanel()); }
            else { currCoroutine = StartCoroutine(HideModelCataloguePanel()); }
        }
    }

    private IEnumerator ShowModelCataloguePanel() {
        RectTransform rect = GetComponent<RectTransform>();
        float maxY = rect.localPosition.y + 250;

        while (rect.localPosition.y < maxY) {
            rect.localPosition += new Vector3(0, Time.deltaTime * 600, 0);

            yield return null;
        }

        rect.localPosition = new Vector3(rect.localPosition.x, maxY, rect.localPosition.z);
        currCoroutine = null;
    }

    private IEnumerator HideModelCataloguePanel() {
        RectTransform rect = GetComponent<RectTransform>();
        float minY = rect.localPosition.y - 250;

        while (rect.localPosition.y > minY) {
            rect.localPosition -= new Vector3(0, Time.deltaTime * 600, 0);

            yield return null;
        }

        rect.localPosition = new Vector3(rect.localPosition.x, minY, rect.localPosition.z);
        currCoroutine = null;
    }
}
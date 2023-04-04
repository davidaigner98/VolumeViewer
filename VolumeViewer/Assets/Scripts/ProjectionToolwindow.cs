using UnityEngine;
using UnityEngine.UI;

public class ProjectionToolwindow : MonoBehaviour {
    public static ProjectionToolwindow Instance { get; private set; }
    private bool clippingBoxEnabled = false;
    public Toggle clippingBoxToggler;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start() {
        AssumePosition();
    }

    private void AssumePosition() {
        GameObject currCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        GameObject currSize = DisplayProfileManager.Instance.GetCurrentDisplaySize();
        Vector2 ownSize = GetComponent<RectTransform>().sizeDelta;

        transform.SetParent(currCenter.transform);
        float newPosX = -currSize.transform.localScale.x / 2 - ownSize.x / 2 - 5;
        float newPosY = -currSize.transform.localScale.y / 2 + ownSize.y / 2;
        transform.localPosition = new Vector3(newPosX, newPosY, 0);
        transform.eulerAngles = currCenter.transform.eulerAngles;
    }

    public void ToggleClippingBox() {
        clippingBoxEnabled = !clippingBoxEnabled;
        clippingBoxToggler.isOn = clippingBoxEnabled;
        ClippingBox.Instance.SetActive(clippingBoxEnabled);
    }
}

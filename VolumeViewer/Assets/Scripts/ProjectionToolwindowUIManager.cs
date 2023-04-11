using UnityEngine;
using UnityEngine.UI;

public class ProjectionToolwindowUIManager : MonoBehaviour {
    public static ProjectionToolwindowUIManager Instance { get; private set; }
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
        Vector3 ownScale = GetComponent<RectTransform>().localScale;

        transform.SetParent(currCenter.transform);
        float newPosX = currSize.transform.localScale.x / 2 + ownSize.x / 2 * ownScale.x;
        float newPosY = -currSize.transform.localScale.y / 2;
        transform.localPosition = new Vector3(newPosX, newPosY, 0);
        transform.localEulerAngles = new Vector3(0, 180, 0);
    }

    public void ToggleClippingBox() {
        clippingBoxEnabled = !clippingBoxEnabled;
        clippingBoxToggler.isOn = clippingBoxEnabled;
        ClippingBox.Instance.SetActive(clippingBoxEnabled);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class ProjectionToolwindowUIManager : MonoBehaviour {
    public static ProjectionToolwindowUIManager Instance { get; private set; }
    private bool clippingBoxEnabled = false;
    public Toggle clippingBoxToggler;
    public bool manualToggle = false;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start() {
        AssumePosition();
    }

    void Update() {
        // debug option for manually toggling the clipping box checkbox
        if (manualToggle) {
            manualToggle = false;
            ToggleClippingBox();
        }
    }

    // makes the projection toolwindow position itself to the left of the display projection
    private void AssumePosition() {
        // get current display center and size
        GameObject currCenter = DisplayProfileManager.Instance.GetCurrentDisplayCenter();
        GameObject currSize = DisplayProfileManager.Instance.GetCurrentDisplaySize();
        
        // calculate size and scale of the projection toolwindow
        Vector2 ownSize = GetComponent<RectTransform>().sizeDelta;
        Vector3 ownScale = GetComponent<RectTransform>().localScale;

        // reposition projection toolwindow
        transform.SetParent(currCenter.transform);
        float newPosX = -currSize.transform.localScale.x / 2 - ownSize.x * 0.1f * ownScale.x;
        float newPosY = -currSize.transform.localScale.y / 2;
        transform.localPosition = new Vector3(newPosX, newPosY, 0);
        transform.localEulerAngles = new Vector3(0, 345, 0);
    }

    // toggle the clipping box via checkbox
    public void ToggleClippingBox() {
        clippingBoxEnabled = !clippingBoxEnabled;
        clippingBoxToggler.isOn = clippingBoxEnabled;
        ClippingBox.Instance.SetActive(clippingBoxEnabled);
    }
}

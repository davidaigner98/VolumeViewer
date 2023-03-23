using System.Collections.Generic;
using UnityEngine;

public class DisplayProfileManager : MonoBehaviour {
    public static DisplayProfileManager Instance { get; private set; }
    public List<DisplayProfile> displayProfiles = new List<DisplayProfile>();

    public enum DisplayProfileEnum {
        Laptop,
        TouchTV
    };

    public DisplayProfileEnum currentProfile = DisplayProfileEnum.TouchTV;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    public DisplayProfile GetCurrentDisplayProfile() {
        foreach (DisplayProfile profile in displayProfiles) {
            if (profile.name.Equals(currentProfile.ToString())) {
                return profile;
            }
        }

        return null;
    }

    public GameObject GetCurrentDisplayCenter() {
        DisplayProfile currentProfile = GetCurrentDisplayProfile();

        if (currentProfile == null) { return null; }
        else { return currentProfile.displayCenter; }
    }

    public GameObject GetCurrentDisplaySize() {
        DisplayProfile currentProfile = GetCurrentDisplayProfile();

        if (currentProfile == null) { return null; }
        else { return currentProfile.displaySize; }
    }
}

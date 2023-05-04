using System.Collections.Generic;
using UnityEngine;

public class DisplayProfileManager : MonoBehaviour {
    public static DisplayProfileManager Instance { get; private set; }
    public List<DisplayProfile> displayProfiles = new List<DisplayProfile>();

    // enumerations of all implemented display profiles
    public enum DisplayProfileEnum {
        Laptop,
        TouchTV
    };

    // the currently selected display profile
    public DisplayProfileEnum currentProfile = DisplayProfileEnum.TouchTV;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    // returns the currently selected display profile
    public DisplayProfile GetCurrentDisplayProfile() {
        // compare enum with profiles
        foreach (DisplayProfile profile in displayProfiles) {
            if (profile.profileName.Equals(currentProfile.ToString())) {
                return profile;
            }
        }

        return null;
    }

    // returns the display center game object of the currently selected display profile
    public GameObject GetCurrentDisplayCenter() {
        DisplayProfile currentProfile = GetCurrentDisplayProfile();

        if (currentProfile == null) { return null; }
        else { return currentProfile.displayCenter; }
    }

    // returns the display size game object of the currently selected display profile
    public GameObject GetCurrentDisplaySize() {
        DisplayProfile currentProfile = GetCurrentDisplayProfile();

        if (currentProfile == null) { return null; }
        else { return currentProfile.displaySize; }
    }
}

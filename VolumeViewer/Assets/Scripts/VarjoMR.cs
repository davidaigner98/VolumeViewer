using UnityEngine;
using Varjo.XR;

public class VarjoMR : MonoBehaviour {
    void Start() {
        // start the varjo HMD rendering
        VarjoMixedReality.StartRender();
    }
}

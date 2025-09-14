using UnityEngine;

public interface ISavePayloadProvider
{
    // Return JSON-serializable object; you convert to/from JSON outside
    object CapturePayload();             // e.g., return a POCO struct/class
    void RestorePayload(object payload); // payload is already deserialized to the right type
    string GetPrefabKey();               // the Registry key for this object
    string GetKind();                    // "Counter" / "Helper" / "Decor" ...
}

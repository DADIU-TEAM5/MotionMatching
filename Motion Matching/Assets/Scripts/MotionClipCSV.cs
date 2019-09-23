using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MotionClipCSV : MotionClip
{
    public TextAsset CSVFile;

    void OnEnable() {
        if (Frames == null || Frames.Count == 0) {
            var importer = new AnimationCSVImporter(CSVFile);

            Frames = importer.ParseCSV();
        }
    }
}

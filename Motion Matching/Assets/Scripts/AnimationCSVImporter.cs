using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

public class AnimationCSVImporter
{
    // The CSV file to parse
    private readonly string[] fileLines;

    private Regex JointNameRegex = new Regex(@".+:(\w+)_\w_\w");

    private int numberOfColumns = 0;

    private Dictionary<string, int> namesToHeader = new Dictionary<string, int>();

    private AnimationClip AnimationClip = new AnimationClip();

    public AnimationCSVImporter (TextAsset csvFile) {
        fileLines = TextToLines(csvFile);

        Debug.Log($"Number of lines in CSV: {fileLines?.Length}");
    }

    public AnimationClip ParseCSV()
    {
        if (AnimationClip.Frames != null) return AnimationClip;

        AnimationClip.Frames = new List<AnimationFrame>();

        if (fileLines == null) return AnimationClip;
        
        // Read header to generate dictionary between joint names to index in csv line
        ParseHeader(); 
        
        for (var i = 1; i < fileLines.Length; i++) {
            ParseLine(i);
        }
        
        return AnimationClip;
    }

    private void ParseHeader() {
        var header = fileLines[0];
        var headerSplit = header.Split(',');
        numberOfColumns = headerSplit.Length;        

        Debug.Log($"Number of columns in CSV: {numberOfColumns}"); 

        for (var i = 0; i < numberOfColumns; i++) {
            var headerFragment = headerSplit[i]; 

            // Match the fragment against the regex, and go to next fragment if it fails
            var match = JointNameRegex.Match(headerFragment);
            if (!match.Success) continue; 

            var groups = match.Groups;
            var jointName = groups[1].Value;

            // If we already added the joint name, go to next fragment.
            if (namesToHeader.ContainsKey(jointName)) continue;

            //Debug.Log($"Found joint name: {jointName}");
            namesToHeader.Add(jointName, i);
        }

        Debug.Log($"Found a total of {namesToHeader.Count} joints");
    }
   
    private void ParseLine(int index) {
        var line = fileLines[index]; 
        if (String.IsNullOrWhiteSpace(line)) return;

        var lineSplit = line.Split(',');
        Assert.AreEqual(numberOfColumns, lineSplit.Length, $"Line does not contain {numberOfColumns} columns... It is line #{index}: \n {line}");

        var timeString = lineSplit[0];
        var time = float.Parse(timeString);

        var frame = new AnimationFrame { Time = time, JointPoints = new List<AnimationJointPoint>() };
        foreach (var nameIndex in namesToHeader) {
            var jointPoint = new AnimationJointPoint();
            jointPoint.Name = nameIndex.Key;

            ParseJointPoint(lineSplit, nameIndex.Value, ref jointPoint);

            frame.JointPoints.Add(jointPoint);
        }

        AnimationClip.Frames.Add(frame);
    } 

    private void ParseJointPoint(string[] lineSplit, int jointIndex, ref AnimationJointPoint jointPoint) {
        var pxString = lineSplit[jointIndex];
        var pyString = lineSplit[jointIndex + 1];
        var pzString = lineSplit[jointIndex + 2];

        var rxString = lineSplit[jointIndex + 3];
        var ryString = lineSplit[jointIndex + 4];
        var rzString = lineSplit[jointIndex + 5];
        var rwString = lineSplit[jointIndex + 6];

        var px = float.Parse(pxString); 
        var py = float.Parse(pyString);
        var pz = float.Parse(pzString);

        var rx = float.Parse(rxString);
        var ry = float.Parse(ryString);
        var rz = float.Parse(rzString);
        var rw = float.Parse(rwString);

        Assert.AreEqual(pxString, px.ToString());
        Assert.AreEqual(pyString, py.ToString());
        Assert.AreEqual(pzString, pz.ToString());
        Assert.AreEqual(rxString, rx.ToString());
        Assert.AreEqual(ryString, ry.ToString());
        Assert.AreEqual(rzString, rz.ToString());
        Assert.AreEqual(rwString, rw.ToString());

        jointPoint.Position = new Vector3(px, py, pz);
        jointPoint.Rotation = new Quaternion(rx, ry, rz, rw);
    }
     
    private string[] TextToLines(TextAsset text) {
        var lines = text?.text.Split(new [] {
            "\r\n", "\r", "\n" 
        }, StringSplitOptions.None);

        return lines;
    } 
}

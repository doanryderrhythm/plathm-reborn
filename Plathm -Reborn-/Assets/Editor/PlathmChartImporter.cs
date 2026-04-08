#if UNITY_EDITOR
using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;

[ScriptedImporter(1, "ptmf")]
public class PlathmChartImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string text = File.ReadAllText(ctx.assetPath);

        TextAsset asset = new TextAsset(text);
        ctx.AddObjectToAsset("text", asset);
        ctx.SetMainObject(asset);
    }
}

[ScriptedImporter(1, "ptminf")]
public class PlathmInformationImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string text = File.ReadAllText(ctx.assetPath);

        TextAsset asset = new TextAsset(text);
        ctx.AddObjectToAsset("text", asset);
        ctx.SetMainObject(asset);
    }
}
#endif

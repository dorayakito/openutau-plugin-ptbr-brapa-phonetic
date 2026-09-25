using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core;
using OpenUtau.Core.DiffSinger;

namespace OpenUtau.Plugin.PtBrBrapa;

[Phonemizer("DiffSinger BRAPA Phonetic (xiao)", "DIFFS PT BRAPA PHONETIC", language: "PT", author: "xiao")]
public sealed class DiffSingerBrapaPhoneticPhonemizer : DiffSingerBasePhonemizer {
    public override string GetLangCode() => string.Empty;

    BrapaPhoneticG2p? bankG2p;
    protected override IG2p LoadG2p(string rootPath, bool useLangId = false) => bankG2p = LoadForBank(rootPath);

    protected override void ProcessPart(Note[][] phrase) {
        var prepared = phrase.Select(group => (Note[])group.Clone()).ToArray();
        var errors = new Dictionary<int, string>();
        foreach (var group in prepared) {
            try {
                if (!string.IsNullOrWhiteSpace(group[0].phoneticHint)) {
                    bankG2p!.UnpackHint(group[0].phoneticHint);
                } else {
                    bankG2p!.Query(group[0].lyric);
                }
            } catch (InvalidDataException e) {
                errors[group[0].position] = e.Message;
                group[0].lyric = "SP";
                group[0].phoneticHint = string.Empty;
            }
        }
        base.ProcessPart(prepared);
        foreach (var error in errors) { unrecognizedLyrics[error.Key] = error.Value; }
    }

    public static BrapaPhoneticG2p LoadForBank(string durationDirectory) {
        var configs = Path.GetFullPath(Path.Combine(durationDirectory, ".."));
        var paths = new[] {
            Path.Combine(configs, "dsconfig.yaml"),
            Path.Combine(durationDirectory, "dsconfig.yaml"),
            Path.Combine(configs, "dspitch", "dsconfig.yaml"),
            Path.Combine(configs, "dsvariance", "dsconfig.yaml")
        };
        HashSet<string>? supported = null;
        foreach (var path in paths) {
            if (!File.Exists(path)) {
                if (path == paths[0] || path == paths[1]) {
                    throw new FileNotFoundException("Configuração DiffSinger ausente", path);
                }
                continue;
            }
            var config = Yaml.DefaultDeserializer.Deserialize<DsConfig>(File.ReadAllText(path));
            var directory = Path.GetDirectoryName(path)!;
            var tokens = DiffSingerUtils.LoadPhonemes(Path.GetFullPath(Path.Combine(directory, config.phonemes)));
            var valid = new HashSet<string>(tokens.Keys, StringComparer.Ordinal);
            if (config.use_lang_id) {
                var languages = DiffSingerUtils.LoadLanguageIds(Path.GetFullPath(Path.Combine(directory, config.languages)));
                valid.RemoveWhere(token => token.Contains('/') && !languages.ContainsKey(token.Split('/')[0]));
            }
            if (supported == null) { supported = valid; } else { supported.IntersectWith(valid); }
        }

        var builder = G2pDictionary.NewBuilder();
        var dsdictPaths = new[] {
            Path.Combine(durationDirectory, "dsdict-brapa.yaml"),
            Path.Combine(durationDirectory, "dsdict.yaml"),
            Path.Combine(durationDirectory, "dsdict-pt.yaml")
        };
        string? loadedDict = dsdictPaths.FirstOrDefault(File.Exists);
        if (loadedDict != null) {
            builder.Load(File.ReadAllText(loadedDict));
        }
        builder.AddSymbol("SP", true);
        builder.AddSymbol("AP", true);

        return new BrapaPhoneticG2p(builder.Build(), supported ?? new HashSet<string>());
    }
}

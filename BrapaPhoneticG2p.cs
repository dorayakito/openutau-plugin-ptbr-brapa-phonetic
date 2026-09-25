using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenUtau.Api;

namespace OpenUtau.Plugin.PtBrBrapa;

public sealed class BrapaPhoneticG2p : IG2p {
    readonly IG2p symbols;
    readonly HashSet<string> supported;

    // Normalizations/aliases for user convenience
    static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase) {
        ["c"] = "k",
        ["q"] = "k",
        ["rr"] = "rr",
        ["r"] = "r",
        ["ch"] = "ch",
        ["tx"] = "ch",
        ["tch"] = "ch",
        ["dj"] = "dj",
        ["dx"] = "dj",
        ["lh"] = "lh",
        ["nh"] = "nh",
        ["sh"] = "sh",
        ["x"] = "x",
        ["j"] = "j",
        ["h"] = "h",
        ["hr"] = "hr",
        ["rh"] = "rh",
        ["rw"] = "rw",
        ["ng"] = "ng",
        ["eh"] = "eh",
        ["oh"] = "oh",
        ["an"] = "an",
        ["en"] = "en",
        ["in"] = "in",
        ["on"] = "on",
        ["un"] = "un",
        ["ae"] = "ae",
        ["ax"] = "ax",
        ["ah"] = "ax", // BRAPA alias support
        ["i0"] = "i0",
        ["u0"] = "u0",
        ["w"] = "w",
        ["y"] = "y",
        ["wn"] = "w",
        ["yn"] = "y",
        ["cl"] = "cl",
        ["vf"] = "vf",
    };

    public BrapaPhoneticG2p(IG2p symbols, IEnumerable<string> supported) {
        this.symbols = symbols;
        this.supported = new HashSet<string>(supported, StringComparer.Ordinal);
        ValidateToken("SP", "pausas");
        ValidateToken("AP", "pausas");
    }

    void ValidateToken(string target, string context) {
        if (!IsValidSymbol(target)) {
            throw new InvalidDataException($"{context}: token '{target}' ausente do banco de voz BRAPA.");
        }
    }

    public bool IsValidSymbol(string symbol) => supported.Contains(symbol) && symbols.IsValidSymbol(symbol);
    public bool IsVowel(string symbol) => IsValidSymbol(symbol) && symbols.IsVowel(symbol);
    public bool IsGlide(string symbol) => IsValidSymbol(symbol) && symbols.IsGlide(symbol);

    public string[] UnpackHint(string hint, char separator = ' ') {
        var tokens = hint.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();
        foreach (var raw in tokens) {
            var token = ResolveToken(raw);
            ValidateToken(token, "fonética manual");
            result.Add(token);
        }
        return result.ToArray();
    }

    string ResolveToken(string raw) {
        var clean = raw.Trim().ToLowerInvariant();
        if (clean is "sp" or "-") return "SP";
        if (clean is "ap" or "br") return "AP";
        if (clean is "cl" or "vf") {
            if (IsValidSymbol(clean)) return clean;
            if (IsValidSymbol($"pt/{clean}")) return $"pt/{clean}";
            return clean;
        }

        if (clean.StartsWith("pt/")) {
            return clean;
        }

        if (Aliases.TryGetValue(clean, out var mapped)) {
            clean = mapped;
        }

        // Try direct pt/ prefix
        var prefixed = $"pt/{clean}";
        if (IsValidSymbol(prefixed)) {
            return prefixed;
        }

        // If voicebank doesn't use pt/ prefix or uses bare tokens
        if (IsValidSymbol(clean)) {
            return clean;
        }

        return prefixed;
    }

    public string[] Query(string grapheme) {
        if (string.IsNullOrWhiteSpace(grapheme)) return null!;

        var trimmed = grapheme.Trim();
        if (trimmed.Equals("br", StringComparison.OrdinalIgnoreCase)) return ["AP"];
        if (trimmed is "SP" or "AP" or "-") return [trimmed == "AP" ? "AP" : "SP"];

        // Split by whitespace
        var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();

        foreach (var raw in parts) {
            var clean = raw.Trim('!', '?', '.', ',', ';', ':', '"', '“', '”', '„', '(', ')');
            if (clean.Length == 0) continue;

            var token = ResolveToken(clean);
            if (!IsValidSymbol(token)) {
                throw new InvalidDataException($"Fone BRAPA inválido ou não suportado pelo banco: '{clean}' (resolvido para '{token}'). Fones válidos: a, e, i, o, u, eh, oh, an, en, in, on, un, k, s, ch, dj, r, rr, rh, etc.");
            }
            result.Add(token);
        }

        return result.Count == 0 ? null! : result.ToArray();
    }
}

# OpenUtau DiffSinger BRAPA Phonetic Phonemizer

Phonemizer fonético direto para DiffSinger com bancos de voz em Português Brasileiro (BRAPA / Saturno) para o [OpenUtau](https://github.com/stakira/OpenUtau).

Este phonemizer dispensa o uso de dicionários G2P baseados em palavras escritas, permitindo que você digite os próprios fones (separados por espaço ou diretamente no lyric), garantindo total precisão fonética e controle sobre a pronúncia.

---

## Funcionalidades

- **Entrada Direta por Fones**: Digite sequências fonéticas como `oh r a s`, `k a z a`, `b on`, etc.
- **Suporte ao conjunto de fones BRAPA**:
  - **Vogais e Nasais**: `a`, `e`, `i`, `o`, `u`, `eh`, `oh`, `an`, `en`, `in`, `on`, `un`, `ae`, `ax`, `i0`, `u0`, etc.
  - **Semivogais**: `w`, `y`
  - **Consoantes**: `b`, `p`, `d`, `t`, `g`, `k`, `v`, `f`, `z`, `s`, `j`, `x`, `ch`, `dj`, `m`, `n`, `nh`, `lh`, `l`, `r` (tap), `rr` (velar/gutural), `rh`, `hr`, `rw`, `ng`, `cl`, `vf`, etc.
- **Pausas e Respirações**: `SP` (pausa / silêncio), `AP` / `br` (respiração / aspiração).
- **Tratamento Automático de Prefixo**: Converte e mapeia automaticamente para bancos que usam `pt/` ou fones puros.

---

## Como Instalar

1. Baixe ou compile a `.dll` (`OpenUtau.Plugin.PtBrBrapa.dll`).
2. Copie a DLL para a pasta `Plugins` do seu OpenUtau:
   - **macOS**: `~/Library/Application Support/OpenUtau/Plugins/` (ou dentro do App)
   - **Windows**: `%AppData%/OpenUtau/Plugins/`
   - **Linux**: `~/.config/OpenUtau/Plugins/`
3. No OpenUtau, selecione a trilha DiffSinger e escolha o phonemizer **`DiffSinger BRAPA Phonetic (xiao)`** (código: `DIFFS PT BRAPA PHONETIC`).

---

## Compilação

Para compilar via linha de comando com .NET 8 SDK:

```bash
dotnet build OpenUtau.Plugin.PtBrBrapa.csproj -c Release
```

---

## Licença

Distribuído sob licença MIT.

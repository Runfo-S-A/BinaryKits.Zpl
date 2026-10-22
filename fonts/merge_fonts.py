"""Run with Python and FontTools: python fonts/merge_fonts.py."""

from pathlib import Path
from tempfile import TemporaryDirectory

from fontTools import subset
from fontTools.merge import Merger
from fontTools.ttLib import TTFont
from fontTools.ttLib.scaleUpem import scale_upem
from fontTools.unicodedata import script


ROOT = Path(__file__).resolve().parent
OUTPUT = ROOT / "Unified-CondensedBold.ttf"
ASIAN_SCRIPTS = {
    "Hani", "Hira", "Kana", "Hrkt", "Hang", "Bopo", "Yiii",
    "Deva", "Beng", "Guru", "Gujr", "Orya", "Taml", "Telu", "Knda",
    "Mlym", "Sinh", "Thai", "Laoo", "Tibt", "Mymr", "Geor", "Armn",
    "Khmr", "Mong", "Limb", "Tale", "Bugi", "Talu", "Bali", "Sund",
    "Batk", "Lepc", "Olck", "Saur", "Kali", "Rjng", "Cham",
}


def is_asian(codepoint):
    return script(chr(codepoint)) in ASIAN_SCRIPTS or any(
        start <= codepoint <= end
        for start, end in (
            (0x2E80, 0x303F), (0x3190, 0x33FF),
            (0xFE10, 0xFE1F), (0xFE30, 0xFE4F), (0xFF00, 0xFFEF),
        )
    )


def main():
    sources = [
        ("ATTriumvirate-CondensedBold.ttf", lambda cp: True),
        ("ANMDS.ttf", is_asian),
        ("AssistB.ttf", lambda cp: script(chr(cp)) == "Hebr"),
        ("ARIALNB.TTF", lambda cp: script(chr(cp)) == "Cyrl"),
    ]
    expected = {}
    counts = {}
    notices = {0: [], 13: [], 14: []}
    with TemporaryDirectory() as temporary:
        paths = []
        for index, (filename, select) in enumerate(sources):
            font = TTFont(ROOT / filename)
            for name_id, values in notices.items():
                texts = sorted({
                    record.toUnicode() for record in font["name"].names
                    if record.nameID == name_id
                })
                if texts:
                    values.append(filename + ":\n" + "\n".join(texts))
            original_cmap = font.getBestCmap()
            selected = {
                cp for cp in original_cmap if select(cp) and cp not in expected
            }
            options = subset.Options()
            options.hinting = False
            subsetter = subset.Subsetter(options=options)
            subsetter.populate(unicodes=selected)
            subsetter.subset(font)
            scale_upem(font, 2048)
            cmap = font.getBestCmap()
            for cp in selected:
                expected[cp] = font["hmtx"][cmap[cp]]
            counts[filename] = len(selected)
            path = Path(temporary) / f"{index}.ttf"
            font.save(path)
            font.close()
            paths.append(str(path))

        merged = Merger().merge(paths)
        # Merger keeps the largest vertical metrics (ANMDS); keep the primary font's so line spacing stays Latin-sized.
        with TTFont(paths[0]) as primary:
            for table, fields in {
                "hhea": ("ascent", "descent", "lineGap"),
                "OS/2": ("sTypoAscender", "sTypoDescender", "sTypoLineGap", "usWinAscent", "usWinDescent"),
            }.items():
                for field in fields:
                    setattr(merged[table], field, getattr(primary[table], field))
        names = merged["name"]
        names.names = []
        for name_id, values in notices.items():
            if values:
                names.setName("\n\n".join(values), name_id, 3, 1, 0x409)
        for name_id, value in {
            1: "Unified Condensed", 2: "Bold",
            3: "UnifiedCondensed-Bold-1.0", 4: "Unified Condensed Bold",
            5: "Version 1.0", 6: "UnifiedCondensed-Bold",
        }.items():
            names.setName(value, name_id, 3, 1, 0x409)
        merged.save(OUTPUT)
        merged.close()

    with TTFont(OUTPUT, checkChecksums=2) as result:
        cmap = result.getBestCmap()
        assert set(cmap) == set(expected), "Character coverage changed during merge"
        for cp, metrics in expected.items():
            assert result["hmtx"][cmap[cp]] == metrics, hex(cp)
        for text in ("Español English ¡Hola! ¿Qué? 0123456789", "中文日本語한국어", "שלום", "Русский"):
            assert all(ord(char) in cmap for char in text), text
        for tag in result.keys():
            result[tag]  # Decode every table to check the saved font.
    for filename, count in counts.items():
        print(f"{filename}: {count} characters")
    print(f"Verified {len(expected)} characters: {OUTPUT}")


if __name__ == "__main__":
    main()

from pathlib import Path
import re
from xml.sax.saxutils import escape

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (BaseDocTemplate, Frame, PageBreak, PageTemplate,
    Paragraph, Spacer, ListFlowable, ListItem, HRFlowable, Image)


ROOT = Path(__file__).resolve().parents[2]
CAPA_DATOS = ROOT / "src" / "CapaDatos"
DOCS = ROOT / "docs" / "manuales"
TEST_DOCS = ROOT / "docs" / "tests"
FONT_DIR = CAPA_DATOS / "CapaPresentacion" / "Fonts"
pdfmetrics.registerFont(TTFont("Inter", str(FONT_DIR / "Inter-Regular.ttf")))
pdfmetrics.registerFont(TTFont("Inter-Bold", str(FONT_DIR / "Inter-Bold.ttf")))
pdfmetrics.registerFontFamily("Inter", normal="Inter", bold="Inter-Bold", italic="Inter", boldItalic="Inter-Bold")

NAVY = colors.HexColor("#17263E")
BLUE = colors.HexColor("#356FD1")
MUTED = colors.HexColor("#64748B")
PALE = colors.HexColor("#EEF4FC")

styles = getSampleStyleSheet()
styles.add(ParagraphStyle(name="CoverTitle", fontName="Inter-Bold", fontSize=27, leading=34,
    textColor=NAVY, alignment=TA_LEFT, spaceAfter=14))
styles.add(ParagraphStyle(name="CoverSub", fontName="Inter", fontSize=13, leading=19,
    textColor=MUTED, alignment=TA_LEFT))
styles.add(ParagraphStyle(name="H1x", fontName="Inter-Bold", fontSize=18, leading=24,
    textColor=NAVY, spaceBefore=9, spaceAfter=9, keepWithNext=True))
styles.add(ParagraphStyle(name="H2x", fontName="Inter-Bold", fontSize=12.5, leading=17,
    textColor=BLUE, spaceBefore=9, spaceAfter=5, keepWithNext=True))
styles.add(ParagraphStyle(name="Bodyx", fontName="Inter", fontSize=9.2, leading=14,
    textColor=colors.HexColor("#253247"), spaceAfter=6, alignment=TA_LEFT))
styles.add(ParagraphStyle(name="Bulletx", parent=styles["Bodyx"], leftIndent=12, firstLineIndent=-8,
    bulletIndent=0, spaceAfter=3))
styles.add(ParagraphStyle(name="TocHead", fontName="Inter-Bold", fontSize=18, leading=23,
    textColor=NAVY, spaceAfter=13))
styles.add(ParagraphStyle(name="Smallx", fontName="Inter", fontSize=8, leading=11, textColor=MUTED))
styles.add(ParagraphStyle(name="Captionx", fontName="Inter", fontSize=8, leading=11,
    textColor=MUTED, alignment=TA_CENTER, spaceBefore=4, spaceAfter=10))


class ManualDoc(BaseDocTemplate):
    def __init__(self, filename, title):
        super().__init__(str(filename), pagesize=A4, leftMargin=21*mm, rightMargin=21*mm,
            topMargin=22*mm, bottomMargin=19*mm, title=title, author="Programa de Concentración")
        self._bookmark = 0
        frame = Frame(self.leftMargin, self.bottomMargin, self.width, self.height, id="main")
        self.addPageTemplates([PageTemplate(id="standard", frames=frame, onPage=self.decorate)])

    def decorate(self, canvas, doc):
        if doc.page > 1:
            canvas.saveState()
            canvas.setStrokeColor(colors.HexColor("#D8E1ED"))
            canvas.line(self.leftMargin, A4[1]-15*mm, A4[0]-self.rightMargin, A4[1]-15*mm)
            canvas.setFont("Inter", 7.5)
            canvas.setFillColor(MUTED)
            canvas.drawString(self.leftMargin, A4[1]-12*mm, "PROGRAMA DE CONCENTRACIÓN  |  DOCUMENTACIÓN 1.0")
            canvas.drawRightString(A4[0]-self.rightMargin, 10*mm, f"{doc.page}")
            canvas.restoreState()

    def afterFlowable(self, flowable):
        if isinstance(flowable, Paragraph) and flowable.style.name in ("H1x", "H2x"):
            level = 0 if flowable.style.name == "H1x" else 1
            text = flowable.getPlainText()
            self._bookmark += 1
            key = f"sec-{self._bookmark}"
            self.canv.bookmarkPage(key)
            self.canv.addOutlineEntry(text, key, level=level, closed=False)
            self.notify("TOCEntry", (level, text, self.page, key))


def markdown_body(path):
    lines = path.read_text(encoding="utf-8").splitlines()
    flow = []
    bullet_group = []
    in_code_block = False

    def flush_bullets():
        nonlocal bullet_group
        if bullet_group:
            items = [ListItem(Paragraph(format_inline(item), styles["Bodyx"]), leftIndent=10) for item in bullet_group]
            flow.append(ListFlowable(items, bulletType="bullet", start="circle", leftIndent=13,
                bulletFontName="Inter", bulletFontSize=6, bulletColor=BLUE, spaceAfter=6))
            bullet_group = []

    for line in lines:
        s = line.strip()
        if s.startswith("```"):
            flush_bullets()
            in_code_block = not in_code_block
            continue
        if in_code_block:
            continue
        if not s:
            flush_bullets()
            continue
        if s.startswith("# "):
            flush_bullets()
            continue
        if s.startswith("## "):
            flush_bullets()
            title = escape(s[3:])
            flow.append(Paragraph(title, styles["H1x"]))
            flow.append(HRFlowable(width="100%", thickness=0.7, color=colors.HexColor("#D8E1ED"),
                spaceBefore=0, spaceAfter=7))
            continue
        if s.startswith("### "):
            flush_bullets()
            flow.append(Paragraph(escape(s[4:]), styles["H2x"]))
            continue
        image_match = re.match(r"!\[(.*?)\]\((.*?)\)", s)
        if image_match:
            flush_bullets()
            caption, source = image_match.groups()
            image_path = (path.parent / source).resolve()
            image = Image(str(image_path))
            max_width, max_height = 160*mm, 180*mm
            scale = min(max_width / image.imageWidth, max_height / image.imageHeight)
            image.drawWidth = image.imageWidth * scale
            image.drawHeight = image.imageHeight * scale
            image.hAlign = "CENTER"
            flow.append(image)
            flow.append(Paragraph(escape(caption), styles["Captionx"]))
            continue
        if s.startswith("- "):
            bullet_group.append(s[2:])
            continue
        if re.match(r"^\d+\. ", s):
            flush_bullets()
            number, text = s.split(". ", 1)
            flow.append(Paragraph(f"<font color='#356FD1'><b>{number}.</b></font>  {format_inline(text)}", styles["Bodyx"]))
            continue
        if s.startswith("|") or s.startswith("---"):
            flush_bullets()
            continue
        flush_bullets()
        flow.append(Paragraph(format_inline(s), styles["Bodyx"]))
    flush_bullets()
    return flow


def format_inline(text):
    escaped = escape(text)
    escaped = re.sub(r"`([^`]+)`", r"<font name='Inter-Bold'>\1</font>", escaped)
    escaped = re.sub(r"\*\*([^*]+)\*\*", r"<b>\1</b>", escaped)
    return escaped


def build(source, output, subtitle):
    title = source.read_text(encoding="utf-8").splitlines()[0].lstrip("# ")
    doc = ManualDoc(output, title)
    section_lines = [line.strip() for line in source.read_text(encoding="utf-8").splitlines()
        if line.startswith("## ") or line.startswith("### ")]
    toc_items = [ListItem(Paragraph(escape(item.lstrip("# ")), styles["Bodyx"]), leftIndent=10)
        for item in section_lines]
    story = [Spacer(1, 50*mm),
        Paragraph("PROGRAMA DE CONCENTRACIÓN", ParagraphStyle("brand", fontName="Inter-Bold", fontSize=10,
            textColor=BLUE, spaceAfter=12)),
        Paragraph(escape(title.replace(" - Programa de Concentración", "")), styles["CoverTitle"]),
        Paragraph(escape(subtitle), styles["CoverSub"]), Spacer(1, 16*mm),
        HRFlowable(width="40%", thickness=3, color=BLUE, hAlign="LEFT"),
        Spacer(1, 8*mm), Paragraph("Edición documental 1.0 · 25 de noviembre de 2026", styles["Smallx"]),
        PageBreak(), Paragraph("Contenido", styles["TocHead"]),
        ListFlowable(toc_items, bulletType="bullet", leftIndent=13, bulletColor=BLUE, bulletFontName="Inter", spaceAfter=8),
        PageBreak()]
    story.extend(markdown_body(source))
    doc.build(story)


if __name__ == "__main__":
    tests = TEST_DOCS / "Inventario_pruebas_v1.0.md"
    technical = DOCS / "manual_tecnico_v1.0.md"
    user = DOCS / "manual_usuario_v1.0.md"
    build(tests, TEST_DOCS / "Inventario_pruebas_v1.0.pdf", "Inventario y resultados de pruebas")
    build(technical, DOCS / "manual_tecnico_v1.0.pdf", "Instalación, arquitectura, configuración y diagnóstico")
    build(user, DOCS / "manual_usuario_v1.0.pdf", "Guía de uso para personas usuarias")
    print("PDFs generados en docs/manuales/ y docs/tests/.")

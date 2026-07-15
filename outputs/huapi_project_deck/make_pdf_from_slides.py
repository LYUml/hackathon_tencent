from pathlib import Path

from reportlab.lib.utils import ImageReader
from reportlab.pdfgen import canvas


OUT = Path(r"C:\Users\lyuml\Desktop\hackathon_tencent\outputs\huapi_project_deck")
PNG_DIR = OUT / "rendered_slides"
PDF = OUT / "画皮_Project_Deck.pdf"

PAGE_W = 1280
PAGE_H = 720


def main() -> None:
    slides = sorted(PNG_DIR.glob("slide-*.png"))
    if len(slides) != 9:
        raise RuntimeError(f"Expected 9 slide PNGs, found {len(slides)}")

    deck = canvas.Canvas(str(PDF), pagesize=(PAGE_W, PAGE_H))
    for slide in slides:
        image = ImageReader(str(slide))
        deck.drawImage(image, 0, 0, width=PAGE_W, height=PAGE_H)
        deck.showPage()
    deck.save()
    print(PDF)


if __name__ == "__main__":
    main()

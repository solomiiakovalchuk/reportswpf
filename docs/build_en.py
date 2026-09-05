# -*- coding: utf-8 -*-
"""English Technical Specification (GOST 19.106-78) + lab report — two .docx files."""
import os
from docx import Document
from docx.shared import Pt, Mm, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING, WD_BREAK
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

FONT = "Times New Roman"
SIZE = Pt(14)
DOCS = r"D:\ХНУ\3kurs\кпз\ReportsWpf\docs"

# ======================================================================
#  helpers bound to the "current document" (set per build)
# ======================================================================
class Doc:
    def __init__(self):
        self.d = Document()
        n = self.d.styles["Normal"]
        n.font.name = FONT
        n.font.size = SIZE
        n._element.rPr.rFonts.set(qn("w:eastAsia"), FONT)
        pf = n.paragraph_format
        pf.line_spacing_rule = WD_LINE_SPACING.ONE_POINT_FIVE
        pf.space_before = Pt(0)
        pf.space_after = Pt(0)
        s = self.d.sections[0]
        s.page_width, s.page_height = Mm(210), Mm(297)
        s.left_margin, s.right_margin = Mm(30), Mm(10)
        s.top_margin, s.bottom_margin = Mm(20), Mm(20)

    # ---- run font ----
    @staticmethod
    def _font(run, bold=False, italic=False, size=SIZE):
        run.font.name = FONT
        run.font.size = size
        run.font.bold = bold
        run.font.italic = italic
        rf = run._element.rPr.find(qn("w:rFonts"))
        if rf is None:
            rf = OxmlElement("w:rFonts")
            run._element.rPr.insert(0, rf)
        rf.set(qn("w:eastAsia"), FONT)

    def body(self, text, indent=True, align="justify", bold=False, italic=False, after=0):
        p = self.d.add_paragraph()
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.ONE_POINT_FIVE
        if indent:
            p.paragraph_format.first_line_indent = Mm(12.5)
        p.paragraph_format.space_after = Pt(after)
        p.alignment = {"justify": WD_ALIGN_PARAGRAPH.JUSTIFY, "center": WD_ALIGN_PARAGRAPH.CENTER,
                       "left": WD_ALIGN_PARAGRAPH.LEFT, "right": WD_ALIGN_PARAGRAPH.RIGHT}[align]
        self._font(p.add_run(text), bold=bold, italic=italic)
        return p

    def bullet(self, text):
        p = self.d.add_paragraph()
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.ONE_POINT_FIVE
        p.paragraph_format.first_line_indent = Mm(12.5)
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        self._font(p.add_run(f"\u2013 {text}"))
        return p

    def num(self, n, text):
        p = self.d.add_paragraph()
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.ONE_POINT_FIVE
        p.paragraph_format.first_line_indent = Mm(12.5)
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        self._font(p.add_run(f"{n}. {text}"))
        return p

    def h1(self, text, level=1):
        p = self.d.add_paragraph(style=f"Heading {level}")
        p.paragraph_format.space_before = Pt(12 if level == 1 else 6)
        p.paragraph_format.space_after = Pt(8 if level == 1 else 4)
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.ONE_POINT_FIVE
        p.alignment = WD_ALIGN_PARAGRAPH.LEFT
        r = p.add_run(text.upper() if level == 1 else text)
        self._font(r, bold=True, italic=(level == 3))
        r.font.color.rgb = RGBColor(0, 0, 0)
        return p

    def h2(self, t): return self.h1(t, 2)
    def h3(self, t): return self.h1(t, 3)

    def plain_center(self, text, after=10):
        p = self.d.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(after)
        self._font(p.add_run(text.upper()), bold=True)
        return p

    def table(self, headers, rows, widths):
        t = self.d.add_table(rows=1, cols=len(headers))
        t.style = "Table Grid"
        t.alignment = WD_TABLE_ALIGNMENT.CENTER
        t.autofit = False
        for i, w in enumerate(widths):
            for c in t.columns[i].cells:
                c.width = Mm(w)
        for i, h in enumerate(headers):
            cell = t.rows[0].cells[i]; cell.width = Mm(widths[i])
            pr = cell.paragraphs[0]
            pr.alignment = WD_ALIGN_PARAGRAPH.CENTER
            pr.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
            self._font(pr.add_run(h), bold=True, size=Pt(12))
        for row in rows:
            cells = t.add_row().cells
            for i, v in enumerate(row):
                cells[i].width = Mm(widths[i])
                pr = cells[i].paragraphs[0]
                pr.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
                self._font(pr.add_run(str(v)), size=Pt(12))
        self.d.add_paragraph().paragraph_format.space_after = Pt(0)
        return t

    def page_break(self):
        self.d.add_paragraph().add_run().add_break(WD_BREAK.PAGE)

    def toc(self):
        p = self.d.add_paragraph()
        run = p.add_run()
        for spec in [("begin", None), ("instr", r'TOC \o "1-3" \h \z \u'),
                     ("sep", None), ("text", "Update this field: right-click \u2192 Update Field."),
                     ("end", None)]:
            kind, val = spec
            if kind == "instr":
                el = OxmlElement("w:instrText"); el.set(qn("xml:space"), "preserve"); el.text = val
            elif kind == "text":
                el = OxmlElement("w:t"); el.text = val
            else:
                el = OxmlElement("w:fldChar")
                el.set(qn("w:fldCharType"), {"begin": "begin", "sep": "separate", "end": "end"}[kind])
            run._element.append(el)
        self._font(run)

    def page_numbers(self, skip_first=True):
        sec = self.d.sections[0]
        if skip_first:
            sec.different_first_page_header_footer = True
        p = sec.footer.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        run = p.add_run()
        for k in ("begin", "instr", "end"):
            if k == "instr":
                el = OxmlElement("w:instrText"); el.set(qn("xml:space"), "preserve"); el.text = "PAGE"
            else:
                el = OxmlElement("w:fldChar"); el.set(qn("w:fldCharType"), k)
            run._element.append(el)
        self._font(run, size=Pt(12))

    def img(self, fname, caption):
        path = os.path.join(DOCS, fname)
        if not os.path.exists(path):
            return
        p = self.d.add_paragraph(); p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(path, width=Mm(165))
        c = self.d.add_paragraph(); c.alignment = WD_ALIGN_PARAGRAPH.CENTER
        c.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
        c.paragraph_format.space_after = Pt(10)
        self._font(c.add_run(caption), size=Pt(12))


# ======================================================================
#  Shared content: the Technical Specification body
# ======================================================================
TERMS = [
    ["UAV", "Unmanned aerial vehicle."],
    ["Report (document)", "A service document of the established form on the use (expenditure) of materiel over a reporting period."],
    ["Operator", "A user who submits sortie data through the electronic form."],
    ["Commander (signatory)", "A user who compiles the consolidated report for the period and signs it."],
    ["Episode (submission)", "One form submission: one sortie or one operator working day."],
    ["Reference", "A table that maps a short name (shown in the form) to a full name (inserted into the report text)."],
    ["Consolidation", "Computing the summary lists of UAVs, munitions and targets for the selected period."],
    ["Template tag", "A placeholder string in the template document (e.g. {{DATE_FROM}}) that the program replaces with a computed value."],
    ["Google Forms", "Google's cloud service for building electronic data-collection forms."],
    ["Google Sheets API", "Google's programming interface for reading and writing spreadsheets."],
    ["OAuth 2.0", "An authorization protocol that grants an application access to a user's data without passing the password."],
    ["SQLite", "An embedded relational database stored in a single file."],
    ["EF Core", "Entity Framework Core, an object-relational mapping library for .NET."],
    ["OpenXML", "The Office Open XML (.docx) format and a library for producing Word documents programmatically."],
    ["MVVM", "Model-View-ViewModel, an architectural pattern that separates presentation logic from the view."],
    ["WPF", "Windows Presentation Foundation, a technology for building desktop interfaces on Windows."],
    ["USPD", "Unified System of Program Documentation (GOST 19.xxx)."],
    ["SP", "Software product."],
]

FUNCS = [
    "Maintaining the references of UAVs, target types, munitions, settlements and people: adding, "
    "editing and deleting records. Each reference record holds a short name (shown to the operator in "
    "the form) and a full name (inserted into the report text); for the target reference the full name "
    "is stored in the genitive case, for the settlement reference the second column holds the region.",
    "Retrieving data from an external source \u2014 importing the electronic form responses through the "
    "Google Sheets API. Columns are located by their headings rather than by ordinal number, so "
    "reordering the form questions does not break the import. A repeated import creates no duplicates "
    "because each source row has a stable key.",
    "Cancelling an erroneous submission \u2014 the record is not deleted but marked with the status "
    "\u00abCANCELLED\u00bb and is then excluded from consolidation; the trace of the submission remains.",
    "Consolidating data for a period \u2014 selecting episodes within the date bounds and by optional "
    "filters (by operator, by settlement); summing the UAV count by type; building the unique lists of "
    "munitions and target types; substituting regions from the settlement reference.",
    "Previewing the report text before the document file is created.",
    "Generating the report document in .docx format by replacing the tags in a template document with "
    "the computed values.",
    "Keeping a log of generated reports (generation date, period, signatory, episode count, file path).",
    "Uploading back to the external table \u2014 appending new reference entries, new people and the "
    "values of the \u00abStatus\u00bb column; existing rows are neither changed nor deleted.",
    "Configuring the System: document details (classification stamp, addressee, resolution, unit name, "
    "default combat order), data-source parameters, file folders, connection check with the external table.",
    "Demo mode \u2014 operation on built-in fictional data without Internet access.",
]

STACK = [
    ["Platform", ".NET 8, Windows 10/11 x64", "runtime environment"],
    ["Interface", "WPF (MVVM), WPF-UI 3 library", "windows, styling, light and dark themes"],
    ["Database", "SQLite, Entity Framework Core 8", "local data storage"],
    ["Documents", "DocumentFormat.OpenXml 3.1", "producing .docx without Microsoft Word"],
    ["Integration", "Google Sheets API v4, OAuth 2.0", "data exchange with the external table"],
]

STAGES = [
    ["Development of the Technical Specification", "Lab 1", "an approved Technical Specification"],
    ["Draft design: data-flow, entity-relationship and state-transition diagrams; network schedule",
     "Lab 2", "the \u00abDraft Design\u00bb document"],
    ["Technical design: structure and function charts, Constantine and Jackson structure charts, module specifications",
     "Lab 3", "the \u00abTechnical Design\u00bb document"],
    ["Implementation and debugging of the software product; preparation of program documentation",
     "Lab 4", "a working SP, a set of USPD documentation"],
    ["Project estimation: LOC and FP metrics, the COCOMO II model, object-oriented metrics",
     "Labs 5-7", "calculations and quality conclusions"],
]

RISKS = [
    ["Change in the external form structure (renamed or reordered questions)", "Medium",
     "columns are located by heading, not by ordinal number"],
    ["Unavailability of the external Google service", "Medium",
     "report generation runs from the local database; demo mode"],
    ["Operator error in quantity or type", "High",
     "the record is marked \u00abCANCELLED\u00bb and resubmitted"],
    ["Duplicate submissions (double form send)", "Medium",
     "a unique source-row key on import"],
    ["Concurrent editing of the table by an external script and the System", "Medium",
     "the upload only appends; existing rows are not changed"],
    ["Storing operational data in a cloud service", "High",
     "demonstration on fictional data; the prospect of a self-hosted deployment"],
]

SHOTS = [
    ("UI_формування_рапорту.png", "Figure B.1 \u2014 The \u00abReport generation\u00bb screen"),
    ("UI_довідники.png", "Figure B.2 \u2014 The \u00abReferences\u00bb screen"),
    ("UI_налаштування.png", "Figure B.3 \u2014 The \u00abSettings\u00bb screen"),
]


def write_terms(x):
    x.body("This document uses the following terms and abbreviations:")
    x.table(["Term / abbreviation", "Definition"], TERMS, [45, 125])


def write_intro(x):
    x.body("The Technical Specification (hereinafter \u2014 TS) sets the requirements for the software "
           "product \u00abSystem for Generating Reports on the Use of UAVs and Munitions\u00bb (hereinafter "
           "\u2014 the System, SP): its purpose, function set, operating conditions, development stages "
           "and acceptance procedure. The TS follows GOST 19.106-78.")
    x.body("The System serves the accounting and documentary recording of materiel expenditure \u2014 "
           "unmanned aerial vehicles and munitions \u2014 in a field unit. The unit periodically reports "
           "the used materiel with a write-off report.")
    x.body("Reports are now written by hand. A document takes 20\u201330 minutes, gives inconsistent "
           "wording and grammatical cases, and complicates reconciliation with warehouse records. "
           "General document-management systems do not know the domain references and need lengthy "
           "configuration. Cloud document generators do not work offline and do not consolidate data "
           "over a period. The System removes the manual text work: the operator submits data through "
           "an electronic form, and the commander gets a finished report in Word format from a single request.")
    x.body("Note. The data in this document and in the demo mode of the System are fictional. The "
           "System is treated as a means of accounting for the consumable materiel of a field unit.", italic=True)


def write_sections(x, top="1"):
    """Emit sections 1-7 + appendices. `top` prefixes numbers when embedded in a report."""
    def hn(n, title):
        x.h1(f"{n} {title}")

    hn(f"{top}.1" if top != "1" else "1", "Name and field of application") if top != "1" else x.h1("1 Name and field of application")
    # simpler: keep plain numbers 1..7 in both docs
    return


def write_body_sections(x):
    x.h1("1 Name and field of application")
    x.body("Full name: \u00abSystem for Generating Reports on the Use of UAVs and Munitions\u00bb.")
    x.body("Conventional (working) designation: ReportsWpf.")
    x.body("Field of application \u2014 accounting and documentary recording of materiel expenditure in a "
           "unit, automation of write-off report preparation. The System is intended for the "
           "commander's desktop workstation; operators interact with the System indirectly, through the "
           "data-collection electronic form.")

    x.h1("2 Basis for development")
    x.body("The basis for development is the curriculum of the discipline \u00abSoftware Construction\u00bb "
           "and the assignment for laboratory work No. 1.")
    x.body("The document that defines the assignment is approved by the department of the "
           "Khmelnytskyi National University.")
    x.body("Name of the development topic: \u00abDevelopment of a software system for accounting and "
           "generating reports on the use of a unit's materiel\u00bb.")

    x.h1("3 Purpose of development")
    x.h2("3.1 Functional purpose")
    x.body("The System collects the episodes of materiel use submitted by the operator, consolidates "
           "them for the selected period and generates a report in Word format from a template.")
    x.h2("3.2 Operational purpose")
    x.body("Mode of use \u2014 daily work. The commander generates reports as needed: typically 2\u20134 "
           "times a month, with the peak at the end of the reporting period. Operators submit data "
           "after each sortie.")
    x.body("Two user roles are defined:")
    x.bullet("operator \u2014 submits sortie data from a mobile device through the electronic form; needs "
             "no special training;")
    x.bullet("commander (administrator) \u2014 maintains the references, generates and signs the "
             "consolidated report, configures the System; qualification \u2014 a confident PC user.")

    x.page_break()
    x.h1("4 Technical requirements for the software product")

    x.h2("4.1 Requirements for functional characteristics")
    x.h3("4.1.1 Set of functions performed")
    x.body("The software product shall provide the following functions:")
    for i, f in enumerate(FUNCS, 1):
        x.num(i, f)

    x.h3("4.1.2 Organization of input and output data")
    x.body("Input data:")
    x.bullet("electronic form responses \u2014 timestamp, sortie date (optional; if absent, the "
             "submission date is used), operator email address, settlement, UAV type, target type, UAV "
             "count, up to three munitions, record status;")
    x.bullet("references \u2014 four blocks on a single sheet of the external table (UAVs, targets, "
             "munitions, places), data from the third row; the left column of a block is the short "
             "name, the right one is the full name;")
    x.bullet("people reference \u2014 email, surname and initials, rank, position.")
    x.body("Internal data is stored in a local SQLite database (tables of submissions, target "
           "engagements, munition uses, references, people and the report log).")
    x.body("Output data:")
    x.bullet("the report file in .docx format, formatted per subsection 4.8;")
    x.bullet("an entry in the log of generated reports;")
    x.bullet("rows appended to the external table (new reference entries, people, values of the "
             "\u00abStatus\u00bb column).")

    x.h3("4.1.3 Timing characteristics")
    x.bullet("report generation for a period up to 30 days \u2014 no more than 5 seconds;")
    x.bullet("synchronization of an external table of up to 5000 rows \u2014 no more than 30 seconds "
             "over a stable connection.")

    x.h2("4.2 Reliability requirements")
    x.body("For reliable operation the SP shall:")
    x.bullet("check the correctness of input data: a value absent from a reference does not interrupt "
             "the import but moves the record to the \u00abneeds attention\u00bb state;")
    x.bullet("prevent a repeated import of the same submission through a unique source-row key;")
    x.bullet("allow error correction without deleting data (the \u00abCANCELLED\u00bb status);")
    x.bullet("record unhandled exceptions in an error log file;")
    x.bullet("keep working when the external service is unavailable: report generation runs from the "
             "local database;")
    x.bullet("show the user clear messages instead of service exceptions (including an explanation of "
             "the cause by the external service response code).")
    x.body("Recovery time after a failure \u2014 restarting the application; saved data is not lost.")

    x.h2("4.3 Operating conditions")
    x.body("The System is operated by one responsible user (the commander) at a workstation. Operators "
           "work only with the electronic form and need no special training. The responsible user's "
           "qualification is confident command of a personal computer.")
    x.body("Servicing the System consists of periodically checking the references and backing up the "
           "data folder. Ambient conditions are those of an ordinary office personal computer; special "
           "requirements for temperature and humidity are not imposed.")

    x.h2("4.4 Requirements for the composition and parameters of technical means")
    x.body("The System requires a personal computer with the following technical means:")
    x.bullet("an x64 processor, at least 2 cores;")
    x.bullet("RAM \u2014 at least 4 GB;")
    x.bullet("free disk space \u2014 at least 500 MB;")
    x.bullet("a monitor with a resolution of at least 1280\u00d7800;")
    x.bullet("an Internet connection \u2014 only for synchronization with the external table.")
    x.body("The operator's device is a smartphone with a web browser and an Internet connection.")

    x.h2("4.5 Requirements for information and software compatibility")
    x.body("Programming language \u2014 C#. The software product runs on the .NET 8 platform under "
           "Windows 10 or Windows 11 (x64). The technology stack is given in Table 4.1.")
    x.body("Table 4.1 \u2014 Technology stack of the System", indent=False, after=2)
    x.table(["Layer", "Technology", "Purpose"], STACK, [30, 70, 70])
    x.bullet("output document format \u2014 Office Open XML (.docx), compatible with Microsoft Word 2007 "
             "and later;")
    x.bullet("exchange format with the external table \u2014 A1-notation ranges of Google Sheets;")
    x.bullet("OAuth credentials are stored locally in the user profile folder and are not included in "
             "the source code repository.")

    x.h2("4.6 Requirements for marking and packaging")
    x.body("Not imposed. The software product is delivered in electronic form.")

    x.h2("4.7 Requirements for transportation and storage")
    x.body("The software product is delivered on a USB drive or through a version control system. "
           "Program documentation is delivered in electronic form. Transportation and storage "
           "conditions for the media are those ordinary for electronic media; the storage period is "
           "not limited.")

    x.h2("4.8 Special requirements")
    x.bullet("the generated report shall be formatted in Times New Roman 14 pt, with 1.5 line spacing, "
             "margins 30/10/20/20 mm, on A4 sheets;")
    x.bullet("the report template is a separate .docx document with tags; the System keeps no "
             "information about formatting and only replaces the tags, so the template can be re-laid "
             "out without changing the program code;")
    x.bullet("the user interface is in Ukrainian, aimed at a user of average qualification;")
    x.bullet("the System supports light and dark themes;")
    x.bullet("the domain logic is separated from the interface and the database, which makes it "
             "possible to extend the System (including a move to a dedicated server) without rewriting "
             "the core.")

    x.h2("4.9 Requirements for program documentation")
    x.body("The composition of the program documentation is defined by the USPD (GOST 19.xxx) and "
           "includes:")
    x.bullet("a user manual;")
    x.bullet("a programmer manual;")
    x.bullet("a system programmer manual;")
    x.bullet("an application description.")

    x.h1("5 Technical and economic indicators")
    x.body("The effect comes from shortening the report preparation time: manual compilation takes "
           "20\u201330 minutes, automatic \u2014 up to 1 minute. Inconsistent wording also disappears "
           "and reconciliation with the warehouse becomes simpler.")
    x.body("Estimated need \u2014 one unit, 2\u20134 reports a month. The System needs no configuration "
           "for the domain and works without the Internet in demo mode.")
    x.body("Development cost is zero: .NET, SQLite, WPF-UI, DocumentFormat.OpenXml and the Google "
           "Sheets API within the free quotas are distributed free of charge.", align="left")

    x.h1("6 Stages of development")
    x.body("Development proceeds in stages; the stages are aligned with the laboratory works of the "
           "discipline. The schedule is given in Table 6.1.")
    x.body("Table 6.1 \u2014 Development schedule", indent=False, after=2)
    x.table(["Stage", "Timeframe", "Stage output"], STAGES, [80, 30, 60])
    x.body("Further development stages (beyond the coursework): a move from the external table to a "
           "dedicated server with a database; offline data submission by the operator with later "
           "synchronization; an electronic signature of the report; munition-count accounting and "
           "automatic reconciliation with warehouse records.")

    x.h1("7 Control and acceptance procedure")
    x.body("Types of testing:")
    x.bullet("checking that every function of subsection 4.1 is performed;")
    x.bullet("an end-to-end test of the processing chain: seeding the database \u2192 importing data "
             "\u2192 consolidating for a period \u2192 generating the .docx \u2192 writing to the log.")
    x.body("Acceptance criteria:")
    x.bullet("every function of subsection 4.1 is performed;")
    x.bullet("the generated document meets the formatting requirements (subsection 4.8);")
    x.bullet("no unreplaced tags remain in the generated report text;")
    x.bullet("a repeated import of the same data creates no duplicates;")
    x.bullet("cancelled records are excluded from consolidation.")
    x.body("The customer may test the delivered module for seven days. Based on the test results the "
           "customer accepts the work for the stage or gives a written justified reason for refusal; "
           "in case of a justified refusal the developer reworks the module.")

    x.page_break()
    x.plain_center("Appendix A (informative). Development risks and mitigation strategies")
    x.table(["Risk", "Level", "Mitigation strategy"], RISKS, [70, 22, 78])

    x.page_break()
    x.plain_center("Appendix B (informative). Screen prototype")
    for f, c in SHOTS:
        x.img(f, c)


# ======================================================================
#  Document 1 — standalone Technical Specification
# ======================================================================
def build_ts():
    x = Doc()
    def tl(text, bold=False, size=SIZE, align="center", after=0):
        p = x.d.add_paragraph()
        p.alignment = {"center": WD_ALIGN_PARAGRAPH.CENTER, "left": WD_ALIGN_PARAGRAPH.LEFT,
                       "right": WD_ALIGN_PARAGRAPH.RIGHT}[align]
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
        p.paragraph_format.space_after = Pt(after)
        x._font(p.add_run(text), bold=bold, size=size)

    tl("Khmelnytskyi National University", size=Pt(13), after=2)
    tl("Department of Computer Engineering and Information Systems", size=Pt(13))
    for _ in range(3):
        tl("")
    ap = x.d.add_paragraph(); ap.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    ap.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
    for t in ["APPROVED", "Supervisor ____________ N. Pravorska", "\u00ab___\u00bb ______________ 2026"]:
        x._font(ap.add_run(t), size=Pt(13)); ap.add_run().add_break()
    for _ in range(4):
        tl("")
    tl("SYSTEM FOR GENERATING REPORTS ON THE USE", bold=True, size=Pt(16), after=4)
    tl("OF UAVs AND MUNITIONS", bold=True, size=Pt(16), after=18)
    tl("TECHNICAL SPECIFICATION", bold=True, size=Pt(15), after=6)
    tl("(for a software product, GOST 19.106-78)", size=Pt(13))
    for _ in range(6):
        tl("")
    tl("Sheets _______", size=Pt(13), align="left", after=24)
    tl("Developed by: student, group IPZs-24-1        S. Kovalchuk", size=Pt(13), align="left", after=4)
    tl("Supervisor: PhD, Assoc. Prof.                          N. Pravorska", size=Pt(13), align="left")
    for _ in range(6):
        tl("")
    tl("Khmelnytskyi \u2014 2026", size=Pt(13))
    x.page_break()

    x.plain_center("Contents")
    x.toc()
    x.h1("List of terms and abbreviations")
    write_terms(x)
    x.page_break()

    x.h1("Introduction")
    write_intro(x)

    write_body_sections(x)

    x.page_numbers()
    out = os.path.join(DOCS, "ЛР1_Technical_Specification_EN.docx")
    x.d.save(out)
    print("saved:", out)


# ======================================================================
#  Document 2 — laboratory report
# ======================================================================
def build_report():
    x = Doc()
    def line(text, bold=False, size=Pt(14), align="center", after=0):
        p = x.d.add_paragraph()
        p.alignment = {"center": WD_ALIGN_PARAGRAPH.CENTER, "left": WD_ALIGN_PARAGRAPH.LEFT}[align]
        p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
        p.paragraph_format.space_after = Pt(after)
        x._font(p.add_run(text), bold=bold, size=size)

    line("Khmelnytskyi National University", size=Pt(13), after=2)
    line("Department of Computer Engineering and Information Systems", size=Pt(13), after=40)
    line("Report", bold=True, size=Pt(20), after=6)
    line("Laboratory work No. 1", bold=True, size=Pt(15), after=24)
    line("Discipline: \u00abSoftware Construction\u00bb", size=Pt(13), after=4)
    x.body("Topic: \u00abSoftware development stages under the structured approach. "
           "The \u00abTechnical Specification\u00bb stage\u00bb", indent=False, align="center", after=40)
    line("Completed by: 3rd-year student, group IPZs-24-1        S. Kovalchuk", size=Pt(13), align="left", after=4)
    line("Checked by:        N. Pravorska", size=Pt(13), align="left", after=60)
    line("Khmelnytskyi \u2014 2026", size=Pt(13))
    x.page_break()

    x.h1("Purpose")
    x.body("To become familiar with the rules for writing a technical specification and to develop a "
           "technical specification for a software product in accordance with GOST 19.106-78.")

    x.h1("Task")
    x.num(1, "Study the theory on the rules and procedure of the \u00abTechnical Specification\u00bb stage.")
    x.num(2, "Develop a technical specification for the software product according to the assigned "
             "variant; format the work in accordance with GOST 19.106-78.")
    x.num(3, "Prepare the report and answer the control questions.")

    x.h1("Task completion")

    x.h2("1 Problem statement")
    x.body("Subject area \u2014 accounting and documentary recording of materiel expenditure (unmanned "
           "aerial vehicles and munitions) in a field unit.")
    x.body("Problem. Write-off reports are compiled by hand. One document takes 20\u201330 minutes, the "
           "wording and grammatical cases differ between reports, and reconciliation with warehouse "
           "records is difficult. Existing tools do not cover the task: general document-management "
           "systems require configuration for the domain, and cloud generators do not work offline "
           "and do not consolidate data over a period.")
    x.body("Solution. A desktop application for the commander. The operator submits sortie data through "
           "an electronic form; the application imports the responses, consolidates the episodes for a "
           "selected period and produces a finished report in .docx format from a template. Errors are "
           "fixed by marking a record as cancelled rather than deleting it. Directories and the "
           "\u00abStatus\u00bb column can be written back to the external table.")
    x.body("Constraints: the operating system is Windows; the report must be formatted in Times New "
           "Roman 14 pt with 1.5 line spacing and A4 margins 30/10/20/20 mm; all demonstration data "
           "are fictional and the System is treated as a means of accounting for the consumable "
           "materiel of a field unit.")

    x.h2("2 Technical specification (GOST 19.106-78)")
    x.body("A technical specification for the software product was developed. Its sections, terms and "
           "appendices follow. The list of terms is given in Table 1, the technology stack in "
           "Table 2, the development schedule in Table 3, the risk analysis in Appendix A, and the "
           "screen prototype in Appendix B.")

    x.h3("List of terms and abbreviations")
    x.body("Table 1 \u2014 Terms and abbreviations", indent=False, after=2)
    x.table(["Term / abbreviation", "Definition"], TERMS, [45, 125])

    x.h3("Introduction")
    write_intro(x)

    write_body_sections(x)

    x.page_break()
    x.h1("Conclusions")
    x.body("During the work I studied the rules and structure of the \u00abTechnical Specification\u00bb "
           "stage and developed a technical specification for the software product \u00abSystem for "
           "Generating Reports on the Use of UAVs and Munitions\u00bb in accordance with GOST 19.106-78. "
           "The specification defines the name and field of application, the basis and purpose of "
           "development, the functional and non-functional requirements, the information and software "
           "compatibility, the technical and economic indicators, the development stages and the "
           "control and acceptance procedure. The set of functions, the input and output data, the "
           "technology stack, the development schedule and the risk analysis are described. The "
           "resulting document is the basis for the next stages of the work: the draft design "
           "(Lab 2) and the technical design (Lab 3).")

    x.h1("Control questions")
    qa = [
        ("What are the stages of software development?",
         "Problem statement and pre-design research; technical specification; draft design; technical "
         "design; implementation and debugging; testing and attestation; operation and maintenance."),
        ("What does the problem statement and pre-design research contain?",
         "A description of the subject area and the object where the product will be used; the problem "
         "to be solved; the set of functions and the input and output data; the environment "
         "(hardware, operating system, related software); the constraints. The pre-design research "
         "analyses existing solutions and the requirements of the customer."),
        ("List the functional and operational requirements for a software product.",
         "Functional requirements describe the set of functions performed, the organization of input "
         "and output data and the timing characteristics. Operational requirements describe the mode "
         "of use, the user roles and their qualification, the operating conditions, servicing and "
         "recovery after a failure."),
        ("List the rules for developing a technical specification.",
         "The TS is drawn up on A4 sheets in accordance with GOST 19.106-78. The title page and the "
         "approval sheet are formatted per the standard. Sheets are numbered above the text. Changes "
         "and additions on later stages are issued as supplements and approved in the same way as the "
         "TS itself. If a requirement is not imposed, the corresponding place states \u00abNot "
         "imposed\u00bb."),
        ("Name the main sections of a technical specification.",
         "Introduction; name and field of application; basis for development; purpose of development; "
         "technical requirements for the software product (functional characteristics, reliability, "
         "operating conditions, technical means, information and software compatibility, marking and "
         "packaging, transportation and storage, special requirements); technical and economic "
         "indicators; stages of development; control and acceptance procedure; appendices."),
    ]
    for i, (q, a) in enumerate(qa, 1):
        x.body(f"{i}. {q}", bold=True, after=2)
        x.body(a, after=8)

    x.page_numbers()
    out = os.path.join(DOCS, "ЛР1_Report_EN.docx")
    x.d.save(out)
    print("saved:", out)


build_ts()
build_report()
